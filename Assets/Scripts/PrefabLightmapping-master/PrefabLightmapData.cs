using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]
public class PrefabLightmapData : MonoBehaviour
{
	public string filePath = "untitled";
	[System.Serializable]
	struct RendererInfo
	{
		public Renderer 	renderer;
		public int 			lightmapIndex;
		public Vector4 		lightmapOffsetScale;
	}

	[SerializeField]
	RendererInfo[]	m_RendererInfo;
	[SerializeField]
	Texture2D[] 	m_Lightmaps;

	public float lightmapProgress = 0;
	public bool cancelBake = false;
	public bool overrideClustering = false;
	


    

    void Start ()
	{
		Invoke("CreateLightmaps", Random.Range(1f,1f));
	}

	#if UnityEditor || UNITY_EDITOR
	void OnGUI(){
		if(overrideClustering){
			overrideClustering = false;
			OnLightmapCompletion();
			CreateLightmaps();
			print("Lightmapping Complete");
		}
		if(UnityEditor.Lightmapping.isRunning && !Application.isPlaying){
			lightmapProgress = Lightmapping.buildProgress;
			if(cancelBake){
				cancelBake = false;
				UnityEditor.Lightmapping.Cancel();
			}
			return;
		}
	}
	#endif

	void CreateLightmaps(){
		
		#if UNITY_EDITOR
		if(UnityEditor.Lightmapping.isRunning && !Application.isPlaying){
			lightmapProgress = Lightmapping.buildProgress;
			if(cancelBake){
				cancelBake = false;
				UnityEditor.Lightmapping.Cancel();
			}
			return;
		}
		#endif
		
		if (m_RendererInfo == null || m_RendererInfo.Length == 0)
			return;
		
		var lightmaps = LightmapSettings.lightmaps;
        int[] offsetsindexes = new int[m_Lightmaps.Length];
        int counttotal = lightmaps.Length;        
        List<LightmapData> combinedLightmaps = new List<LightmapData>();
        
        for (int i = 0; i < m_Lightmaps.Length; i++)
        {
            bool exists = false;
            for (int j = 0; j < lightmaps.Length; j++)
            {
               
                if (m_Lightmaps[i] == lightmaps[j].lightmapColor)
                {
                    exists = true;
                    offsetsindexes[i] = j;
                    
                }
                
            }
            if (!exists)
            {
                offsetsindexes[i] = counttotal;
                var newlightmapdata = new LightmapData();
                newlightmapdata.lightmapColor = m_Lightmaps[i];
                combinedLightmaps.Add(newlightmapdata);

                counttotal += 1;


            }

        }

		var combinedLightmaps2 = new LightmapData[counttotal];
        
		lightmaps.CopyTo(combinedLightmaps2, 0);
        combinedLightmaps.ToArray().CopyTo(combinedLightmaps2, lightmaps.Length);
        LightmapSettings.lightmapsMode = LightmapsMode.NonDirectional;
        ApplyRendererInfo(m_RendererInfo, offsetsindexes);
		LightmapSettings.lightmaps = combinedLightmaps2;
		
		
	}

	
	static void ApplyRendererInfo (RendererInfo[] infos, int[] lightmapOffsetIndex)
	{
		for (int i=0;i<infos.Length;i++)
		{
			var info = infos[i];
			info.renderer.lightmapIndex = lightmapOffsetIndex[info.lightmapIndex];
			info.renderer.lightmapScaleOffset = info.lightmapOffsetScale;
		}


    }

#if UNITY_EDITOR
	[UnityEditor.MenuItem("Assets/Bake Per-Object Lightmaps")]
	static void GenerateLightmapInfo ()
	{
		
		if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
		{
			Debug.LogError("ExtractLightmapData requires that you have baked you lightmaps and Auto mode is disabled.");
			return;
		}
		
		UnityEditor.Lightmapping.BakeAsync();

		UnityEditor.Lightmapping.completed += OnLightmapCompletion;
		
		
	}

	static void OnLightmapCompletion(){
		PrefabLightmapData[] prefabs = FindObjectsOfType<PrefabLightmapData>();

		foreach (var instance in prefabs)
		{
			if(!instance.gameObject.isStatic){
				continue;
			}
			var gameObject = instance.gameObject;
			var rendererInfos = new List<RendererInfo>();
			var lightmaps = new List<Texture2D>();
			var tempLightMaps = new List<Texture2D>();
			
			GenerateLightmapInfo(gameObject, rendererInfos, lightmaps, tempLightMaps);
			
			instance.m_RendererInfo = rendererInfos.ToArray();
			instance.m_Lightmaps = lightmaps.ToArray();

			//var targetPrefab = UnityEditor.PrefabUtility.GetCorrespondingObjectFromSource(gameObject) as GameObject;
			//if (targetPrefab != null)
			//{
				//UnityEditor.Prefab
			//	UnityEditor.PrefabUtility.ReplacePrefab(gameObject, targetPrefab);
			//}
			
		}
	}

	static void GenerateLightmapInfo (GameObject root, List<RendererInfo> rendererInfos, List<Texture2D> lightmaps, List<Texture2D> tempLightMaps)
	{
		var renderers = root.GetComponentsInChildren<MeshRenderer>();
		foreach (MeshRenderer renderer in renderers)
		{
			if (renderer.lightmapIndex != -1)
			{
				RendererInfo info = new RendererInfo();
				info.renderer = renderer;
				info.lightmapOffsetScale = renderer.lightmapScaleOffset;
				print(LightmapSettings.lightmaps.Length);
				print("renderindex");
				print(renderer.lightmapIndex);
				Texture2D lightmap = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
				print(LightmapSettings.lightmaps.Length);

				info.lightmapIndex = tempLightMaps.IndexOf(lightmap);
				if (info.lightmapIndex == -1)
				{

					info.lightmapIndex = tempLightMaps.Count;
					
					if(!System.IO.Directory.Exists("Assets/Lighting")){
						System.IO.Directory.CreateDirectory("Assets/Lighting");
					}
					if(!System.IO.Directory.Exists("Assets/Lighting/" + SceneManager.GetActiveScene().name)){
						System.IO.Directory.CreateDirectory("Assets/Lighting/" + SceneManager.GetActiveScene().name);
					}
					//System.IO.File.WriteAllBytes("Assets/Lighting/" + SceneManager.GetActiveScene().name + "/" + lightmap.name,lightmap.EncodeToEXR());
					//This gets around the "lock" put onto my texture by the read/write checkbox
					lightmap.filterMode = FilterMode.Point;
					RenderTexture rt = RenderTexture.GetTemporary(lightmap.width, lightmap.height);
					rt.filterMode = FilterMode.Point;
					RenderTexture.active = rt;
					Graphics.Blit(lightmap, rt);
					Texture2D img2 = new Texture2D(lightmap.width, lightmap.height);
					img2.ReadPixels(new Rect(0, 0, lightmap.width, lightmap.height), 0, 0);
					img2.Apply();
					RenderTexture.active = null;
					string fileName = "Assets/Lighting/" + SceneManager.GetActiveScene().name + "/" + root.GetComponent<PrefabLightmapData>().filePath + info.lightmapIndex.ToString() + ".asset";
					AssetDatabase.CreateAsset(img2, fileName);
					//System.IO.File.WriteAllBytes(fileName,img2.EncodeToJPG());
					print("loading from: " + fileName);
					Texture2D persistentLightmap = (Texture2D)AssetDatabase.LoadAssetAtPath(fileName, typeof(Texture2D));

					
					
					//lightmaps.Add((Texture2D)AssetDatabase.LoadAssetAtPath("Assets/Lighting/" + SceneManager.GetActiveScene().name + "/" + lightmap.name + ".jpg", typeof(Texture2D)));
					lightmaps.Add(persistentLightmap);
					tempLightMaps.Add(lightmap);
				}

				rendererInfos.Add(info);
			}
		}
	}
#endif

}