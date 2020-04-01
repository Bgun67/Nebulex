using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skybox_Change : MonoBehaviour
{
    public int i = 0;
    public List<Cubemap> textures;
    // Start is called before the first frame update
    void Start()
    {
        InvokeRepeating("ChangeSkybox", 1f, 1f);
    }

    void ChangeSkybox()
    {
        RenderSettings.skybox.SetTexture("_Tex", textures[i]);
        i++;
        if(i>textures.Count-1)
            i=0;
    }
}
