using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] Transform bodyMeshes;
    [SerializeField] Transform fpsMeshes;

    // Start is called before the first frame update
    public void SetTeam()
    {
        
    }

    // Update is called once per frame
    public void SetLocal(bool isLocal)
    {
        if(isLocal){
            foreach(Renderer meshRenderer in bodyMeshes.GetComponentsInChildren<Renderer>()){
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }
        }
    }
}
