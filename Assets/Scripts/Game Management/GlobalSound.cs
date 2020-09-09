using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalSound : MonoBehaviour
{
    public AudioClip[] jukeBox;
    public AudioSource effectsSource;
    public AudioClip painClip;
    public static GlobalSound instance;
    private Game_Controller gameController;
    MetworkView metview;

    public enum JukeBox{
        HitEnemy = 0
    }
    // Start is called before the first frame update
    void Start()
    {
        if(instance == null)
            instance = this;
        else{
            Destroy(instance.gameObject);
            instance = this;
        }
        metview = this.GetComponent<MetworkView>();   
        gameController = GameObject.FindObjectOfType<Game_Controller>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public static void HurtSound(){
        instance.effectsSource.PlayOneShot(instance.painClip);
    }

    public static void SendRemoteSound(JukeBox _sound, int _destinationPlayer){
        if(Metwork.peerType  != MetworkPeerType.Disconnected){
            instance.metview.RPC("RPC_RemoteSound", MRPCMode.All, new object[]{(int)_sound, _destinationPlayer});
        }
        else{
            instance.RPC_RemoteSound((int)_sound, _destinationPlayer);
        }
    } 

    [MRPC]
    public void RPC_RemoteSound(int _sound, int _destinationPlayer){
        if(_destinationPlayer == gameController.localPlayer.GetComponent<Metwork_Object>().owner){
            effectsSource.PlayOneShot(jukeBox[_sound]);
        }
    }


}
