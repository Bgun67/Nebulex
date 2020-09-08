using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LHX_Ultima : Map_Manager
{
    protected override void DelayedSetup()
    {
        foreach( Player_Controller _player in players){
            _player.airTime = 36000;
        }
    }

    // Update is called once per frame
    void Update()
    {
        foreach( Player_Controller _player in players){
            _player.airTime = 36000f;
            _player.suffocationTime = 36000f;
            if(_player.rb.useGravity){
                if (!_player.enteringGravity)
				{
                    _player.rb.useGravity = false;
					_player.StopCoroutine("EnterGravity");
					_player.StartCoroutine("ExitGravity");

                }
            }
        }
    }
}
