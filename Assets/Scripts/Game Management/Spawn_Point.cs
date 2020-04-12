using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn_Point : MonoBehaviour
{
   
    [Tooltip("Which game types does this spawn point show up in?")]
    public string[] allowedGames = {Game_Controller.GameType.Destruction,Game_Controller.GameType.TeamDeathmatch,Game_Controller.GameType.CTF,Game_Controller.GameType.Meltdown,Game_Controller.GameType.Soccer};
    public int team;

    public void Start(){
        bool _setActive = false;
        for(int i = 0; i < allowedGames.Length; i++){
            //TODO: Don't set the object to inactive if its on the wrong team. The bots need to find these objects too!
            if(Game_Controller.Instance.GetLocalTeam() == team && Game_Controller.Instance.gameMode == allowedGames[i]){
                _setActive = true;
                break;
            }
        }
        this.gameObject.SetActive(_setActive);
    }
    
}
