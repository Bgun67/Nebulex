using System;
using UnityEngine;
using UnityEngine.PostProcessing;

namespace UnityStandardAssets.ImageEffects
{
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    [AddComponentMenu ("Image Effects/Displacement/Fisheye")]
    public class Fisheye : PostEffectsBase
	{
        [Range(0.0f, 1.5f)]
        public float strengthX = 0.05f;
        [Range(0.0f, 1.5f)]
        public float strengthY = 0.05f;

        public float m_DOFAdaptRate = 0.2f;

        public Shader fishEyeShader = null;
        private Material fisheyeMaterial = null;
        public Shader depthSwapShader = null;
        private Material depthSwapMaterial = null;

        bool texturesInitialized = false;
        RenderTexture depthColor;
        Texture2D depthTexture;



        public override bool CheckResources ()
		{
            CheckSupport (false);
            fisheyeMaterial = CheckShaderAndCreateMaterial(fishEyeShader,fisheyeMaterial);
            /* ADOF use
            depthSwapMaterial = CheckShaderAndCreateMaterial(depthSwapShader,depthSwapMaterial);
            */

            if (!isSupported)
                ReportAutoDisable ();
            return isSupported;
        }

        void OnRenderImage (RenderTexture source, RenderTexture destination)
		{
            if (CheckResources()==false)
			{
                Graphics.Blit (source, destination);
                return;
            }

            float oneOverBaseSize = 80.0f / 512.0f; // to keep values more like in the old version of fisheye

            float ar = (source.width * 1.0f) / (source.height * 1.0f);

            /*For future ADOF use
            if(!texturesInitialized){
                depthColor = new RenderTexture(source);
                depthTexture = new Texture2D(depthColor.width, depthColor.height, source.graphicsFormat, source.mipmapCount, UnityEngine.Experimental.Rendering.TextureCreationFlags.MipChain);
                texturesInitialized = true;
            }

            Graphics.Blit(source, depthColor, depthSwapMaterial);

            RenderTexture.active = depthColor;
            depthTexture.ReadPixels(new Rect(0, 0, depthTexture.width, depthTexture.height),0,0);
            depthTexture.Apply();
            float averageDepth = 0;
            int mipLevel = depthTexture.mipmapCount -1;
            int mipWidth = depthTexture.width / (int)Mathf.Pow(2, mipLevel);
            print(mipWidth);
            int mipHeight = depthTexture.height / (int)Mathf.Pow(2, mipLevel);
            float perc = 0.2f;
            Color[] pixels = depthTexture.GetPixels((int)(mipWidth * (1.0f - perc))/2, (int)(mipHeight * (1.0f - perc))/2, (int)(mipWidth * perc), (int)(mipHeight * perc), mipLevel);
            Array.Sort(pixels, (x, y) => x.r.CompareTo(y.r));
            averageDepth = pixels[pixels.Length / 2].r;
            print(pixels.Length);
            AdjustDOF(averageDepth);*/

            fisheyeMaterial.SetVector ("intensity", new Vector4 (strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize, strengthX * ar * oneOverBaseSize, strengthY * oneOverBaseSize));
            Graphics.Blit (source, destination, fisheyeMaterial);
        }

        private void AdjustDOF(float depth)
        {
            print(depth);
            PostProcessingProfile profile = GetComponent<PostProcessingBehaviour>().profile;
            
            DepthOfFieldModel.Settings settings = profile.depthOfField.settings;
            settings.focusDistance = Mathf.Pow(10, Mathf.Lerp(Mathf.Log10(settings.focusDistance), Mathf.Log10(depth), m_DOFAdaptRate));
            profile.depthOfField.settings = settings;
        }
    }
}
