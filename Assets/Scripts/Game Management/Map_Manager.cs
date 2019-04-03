using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map_Manager : MonoBehaviour
{
    public enum Map{
        LHX_Ultima
    }

    public Map map;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        switch(map){
            case Map.LHX_Ultima:
                LHXUltimaUpdate();
                break;
            default:
                break;
        }
    }

    void LHXUltimaUpdate(){
        foreach( Player_Controller _player in GameObject.FindObjectsOfType<Player_Controller>()){
            _player.airTime = 1000;
        }
    }
}
