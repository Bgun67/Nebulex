using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Point : MonoBehaviour
{
   
    [Tooltip("Which game types does this spawn point show up in?")]
    public string[] allowedGames = {Game_Controller.GameType.Destruction,Game_Controller.GameType.TeamDeathmatch,Game_Controller.GameType.CTF,Game_Controller.GameType.Meltdown,Game_Controller.GameType.Soccer};
    public int team;

    public void DeactivateSpawn(){
        bool _setActive = false;
        for(int i = 0; i < allowedGames.Length; i++){
            if(Game_Controller.Instance.gameMode == allowedGames[i]){
                _setActive = true;
                break;
            }
        }
        this.gameObject.SetActive(_setActive);
    }
    
}
