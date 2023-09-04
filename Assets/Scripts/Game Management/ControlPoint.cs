using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GameItem : MonoBehaviour{ 
    public enum ControlStatus{
        TeamA,
        TeamB,
        Contested,
        Available
    }
    public abstract string m_GameType{get;}
    public abstract void Initialize(string gameType);
}

public class ControlPoint : GameItem
{
    //30.0 Seconds to load
    const float LOAD_SPEED = 1.0f / 30f;
    const float UNLOAD_SPEED = 1.0f / 50f;

    public float m_Progress = 0.0f;
    //Accessor
    public ControlStatus m_Status = ControlStatus.Available;
    private Collider m_Collider;
    [SerializeField]
    private MeshRenderer m_MeshInstance;

    private int m_TeamAPlayers = 0;
    private int m_TeamBPlayers = 0;



    public override string m_GameType{
        get{
            return Game_Controller.GameType.ControlPoint;
        }
    }
    
    public override void Initialize(string gameType){
        if(m_GameType != gameType){
            this.gameObject.SetActive(false);
            return;
        } 
        m_Collider = this.GetComponent <Collider>();
    }

    void Update(){
        //TODO: Publish results to game_controller on server only
        //TODO: Bots navigate to control points
        int direction = m_TeamAPlayers - m_TeamBPlayers;
        m_Progress += direction * Time.deltaTime * LOAD_SPEED;

        if(m_Progress >= 1.0f){
            m_Status = ControlStatus.TeamA;
        }
        else if(m_Progress >= 1.0f){
            m_Status = ControlStatus.TeamB;
        }
        else if (m_TeamAPlayers + m_TeamBPlayers == 0){
            m_Status = ControlStatus.Available;
            m_Progress -= Mathf.Sign(m_Progress) * Time.deltaTime * UNLOAD_SPEED;
        }
        else{
            m_Status = ControlStatus.Contested;
        }

        m_Progress = Mathf.Clamp(m_Progress, -1f, 1f);
        m_MeshInstance.material.SetFloat("_Progress", m_Progress);
        m_MeshInstance.material.SetFloat("_Changing", m_Status == ControlStatus.Contested ? 1.0f : 0.0f);
    }

    void FixedUpdate(){
        m_TeamAPlayers = 0;
        m_TeamBPlayers = 0;
    }
    void OnTriggerStay(Collider other){
        Player otherPlayer = other.GetComponent<Player>();
        if (otherPlayer == null){
            return;
        }
        
        if (otherPlayer.GetTeam() == 0){
            m_TeamAPlayers += 1;
        }else{
            m_TeamBPlayers += 1;
        }

    }


}
