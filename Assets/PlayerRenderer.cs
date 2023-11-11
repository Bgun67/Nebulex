using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRenderer : MonoBehaviour
{
    [SerializeField] Transform bodyMeshes;
    [SerializeField] Renderer[] teamMeshes;
    [SerializeField] Transform fpsMeshes;

    [SerializeField] Color teamAColor = Color.white;
    [SerializeField] Color teamBColor = Color.white;

    // Start is called before the first frame update
    public void SetTeam(int team)
    {
        print(team);
        foreach(Renderer renderer in teamMeshes){
            foreach(Material mat in renderer.materials){
                mat.color= team!=0? teamAColor: teamBColor;
            }
        }
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
