﻿// This file is subject to the MIT License as seen in the root of this folder structure (LICENSE)

using UnityEngine;
using UnityEngine.Rendering;

namespace Crest
{
    /// <summary>
    /// Captures waves/shape that is drawn kinematically - there is no frame-to-frame state. The gerstner
    /// waves are drawn in this way. There are two special features of this particular LodData.
    ///
    ///  * A combine pass is done which combines downwards from low detail lods down into the high detail lods (see OceanScheduler).
    ///  * The textures from this LodData are passed to the ocean material when the surface is drawn (by OceanChunkRenderer).
    ///  * LodDataDynamicWaves adds its results into this LodData. The dynamic waves piggy back off the combine
    ///    pass and subsequent assignment to the ocean material (see OceanScheduler).
    ///  * The LodDataSeaFloorDepth sits on this same GameObject and borrows the camera. This could be a model for the other sim types..
    /// </summary>
    public class LodDataAnimatedWaves : LodData
    {
        public override SimType LodDataType { get { return SimType.AnimatedWaves; } }
        // shape format. i tried RGB111110Float but error becomes visible. one option would be to use a UNORM setup.
        public override RenderTextureFormat TextureFormat { get { return RenderTextureFormat.ARGBHalf; } }
        public override CameraClearFlags CamClearFlags { get { return CameraClearFlags.Color; } }
        public override RenderTexture DataTexture { get { return Cam.targetTexture; } }

        [Tooltip("Read shape textures back to the CPU for collision purposes.")]
        public bool _readbackShapeForCollision = true;

        /// <summary>
        /// Turn shape combine pass on/off. Debug only - idef'd out in standalone
        /// </summary>
        public static bool _shapeCombinePass = true;

        CommandBuffer _bufCombineShapes = null;
        CameraEvent _combineEvent = 0;
        Camera _combineCamera = null;
        Material _combineMaterial;
        Material CombineMaterial { get { return _combineMaterial ?? (_combineMaterial = new Material(Shader.Find("Ocean/Shape/Combine"))); } }

        [SerializeField]
        protected SimSettingsAnimatedWaves _settings;
        public override void UseSettings(SimSettingsBase settings) { _settings = settings as SimSettingsAnimatedWaves; }
        public SimSettingsAnimatedWaves Settings { get { return _settings as SimSettingsAnimatedWaves; } }
        public override SimSettingsBase CreateDefaultSettings()
        {
            var settings = ScriptableObject.CreateInstance<SimSettingsAnimatedWaves>();
            settings.name = SimName + " Auto-generated Settings";
            return settings;
        }

        public void HookCombinePass(Camera camera, CameraEvent onEvent)
        {
            _combineCamera = camera;
            _combineEvent = onEvent;

            if (_bufCombineShapes == null)
            {
                _bufCombineShapes = new CommandBuffer();
                _bufCombineShapes.name = "Combine Displacements";

                var cams = OceanRenderer.Instance._camsAnimWaves;
                for (int L = cams.Length - 2; L >= 0; L--)
                {
                    // accumulate shape data down the LOD chain - combine L+1 into L
                    var mat = OceanRenderer.Instance._lodDataAnimWaves[L].CombineMaterial;
                    _bufCombineShapes.Blit(cams[L + 1].targetTexture, cams[L].targetTexture, mat);
                }
            }

            _combineCamera.AddCommandBuffer(_combineEvent, _bufCombineShapes);
        }

        public void UnhookCombinePass()
        {
            if (_bufCombineShapes != null)
            {
                _combineCamera.RemoveCommandBuffer(_combineEvent, _bufCombineShapes);
                _bufCombineShapes = null;
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            // shape combine pass done by last shape camera - lod 0
            if (LodTransform.LodIndex == 0)
            {
                if (_bufCombineShapes != null && !_shapeCombinePass)
                {
                    UnhookCombinePass();
                }
                else if (_bufCombineShapes == null && _shapeCombinePass)
                {
                    HookCombinePass(_combineCamera, _combineEvent);
                }
            }
        }
#endif

        public float MaxWavelength()
        {
            float oceanBaseScale = OceanRenderer.Instance.transform.lossyScale.x;
            float maxDiameter = 4f * oceanBaseScale * Mathf.Pow(2f, LodTransform.LodIndex);
            float maxTexelSize = maxDiameter / (4f * OceanRenderer.Instance._baseVertDensity);
            return 2f * maxTexelSize * OceanRenderer.Instance._minTexelsPerWave;
        }

        // script execution order ensures this runs after ocean has been placed
        protected override void LateUpdate()
        {
            base.LateUpdate();

            LateUpdateShapeCombinePassSettings();
        }

        // apply this camera's properties to the shape combine materials
        void LateUpdateShapeCombinePassSettings()
        {
            BindResultData(0, CombineMaterial);

            if (LodTransform.LodIndex > 0)
            {
                var ldaws = OceanRenderer.Instance._lodDataAnimWaves;
                BindResultData(1, ldaws[LodTransform.LodIndex - 1].CombineMaterial);
            }
        }

        void OnDisable()
        {
            UnhookCombinePass();
        }

        protected override void BindData(int shapeSlot, IPropertyWrapper properties, Texture applyData, bool blendOut, ref LodTransform.RenderData renderData)
        {
            base.BindData(shapeSlot, properties, applyData, blendOut, ref renderData);

            // need to blend out shape if this is the largest lod, and the ocean might get scaled down later (so the largest lod will disappear)
            bool needToBlendOutShape = LodTransform.LodIndex == LodTransform.LodCount - 1 && OceanRenderer.Instance.ScaleCouldDecrease && blendOut;
            float shapeWeight = needToBlendOutShape ? OceanRenderer.Instance.ViewerAltitudeLevelAlpha : 1f;
            properties.SetVector(_paramsOceanParams[shapeSlot],
                new Vector4(LodTransform._renderData._texelWidth, LodTransform._renderData._textureRes, shapeWeight, 1f / LodTransform._renderData._textureRes));
        }

        /// <summary>
        /// Returns index of lod that completely covers the sample area, and contains wavelengths that repeat no more than twice across the smaller
        /// spatial length. If no such lod available, returns -1. This means high frequency wavelengths are filtered out, and the lod index can
        /// be used for each sample in the sample area.
        /// </summary>
        public static int SuggestDataLOD(Rect sampleAreaXZ)
        {
            return SuggestDataLOD(sampleAreaXZ, Mathf.Min(sampleAreaXZ.width, sampleAreaXZ.height));
        }
        public static int SuggestDataLOD(Rect sampleAreaXZ, float minSpatialLength)
        {
            var ldaws = OceanRenderer.Instance._lodDataAnimWaves;
            for (int lod = 0; lod < ldaws.Length; lod++)
            {
                // Shape texture needs to completely contain sample area
                var ldaw = ldaws[lod];
                var lodRect = ldaw.LodTransform._renderData.RectXZ;
                // Shrink rect by 1 texel border - this is to make finite differences fit as well
                lodRect.x += ldaw.LodTransform._renderData._texelWidth; lodRect.y += ldaw.LodTransform._renderData._texelWidth;
                lodRect.width -= 2f * ldaw.LodTransform._renderData._texelWidth; lodRect.height -= 2f * ldaw.LodTransform._renderData._texelWidth;
                if (!lodRect.Contains(sampleAreaXZ.min) || !lodRect.Contains(sampleAreaXZ.max))
                    continue;

                // The smallest wavelengths should repeat no more than twice across the smaller spatial length. Unless we're
                // in the last LOD - then this is the best we can do.
                var minWL = ldaw.MaxWavelength() / 2f;
                if (minWL < minSpatialLength / 2f && lod < ldaws.Length - 1)
                    continue;

                return lod;
            }

            return -1;
        }
    }
}
