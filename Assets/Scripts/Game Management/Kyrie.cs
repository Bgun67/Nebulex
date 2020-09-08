using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kyrie : Map_Manager
{
    protected override void DelayedSetup()
    {
    }

    // Update is called once per frame
    void Update()
    {
        foreach( Player_Controller _player in players){
            
            if(!_player.rb.useGravity){
                //TODO: Update these
                _player.airTime = 36000f;
                _player.suffocationTime = 36000f;
            }
        }
    }
    //TODO: Move stuff here
    public void DeOrbit(){

    }
}
