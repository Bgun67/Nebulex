using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioWrapper : MonoBehaviour
{
    AudioSource source;

    public AudioClip[] entries;
    public AudioClip[] loops;
    public AudioClip[] interpolation;
    public AudioClip[] exits;

    enum ClipStatus{
        Entry,
        Loop,
        Exit
    }

    struct ClipInfo{
        public int id;
        public float volume;
        public ClipStatus status;
        public float lastTime;
        public float lastTransistionTime;
        public bool isInterpolating;
    }


    List<ClipInfo> runningClips = new List<ClipInfo>();


    // Start is called before the first frame update
    void Start()
    {
        source = this.gameObject.AddComponent<AudioSource>();
        source.rolloffMode = AudioRolloffMode.Linear;
        source.maxDistance = 20f;
    }

    public void PlayOneShot(int _clip, float _volume){
        int _index = runningClips.FindIndex(x => x.id == _clip);

        ClipInfo _info;

        if(_index == -1){
            
            _info.id = _clip;
            _info.volume = _volume;
            _info.status = ClipStatus.Entry;
            _info.lastTime = Time.time;
            _info.lastTransistionTime = Time.time;
            _info.isInterpolating = false;

            runningClips.Add(_info);
            source.PlayOneShot(entries[_clip], _volume);

        }
        else{
            _info = runningClips[_index];
            if(_info.status == ClipStatus.Loop){
                _info.lastTime = Time.time;
                _info.volume = _volume;
            }
            runningClips[_index] = _info;
        }
    }

    // Update is called once per frame
    void Update()
    {
        ClipInfo _info;
        for(int i = 0; i< runningClips.Count; i++){
            _info = runningClips[i];
            if(_info.status == ClipStatus.Entry && Time.time - _info.lastTime > entries[_info.id].length - 0.00f){
                _info.status = ClipStatus.Loop;
                _info.lastTransistionTime = Time.time;

                source.PlayOneShot(loops[_info.id], _info.volume);

                runningClips[i] = _info;
            }
            else if(_info.status == ClipStatus.Loop && Time.time - _info.lastTime > loops[_info.id].length - 0.00f && Time.time - _info.lastTransistionTime > loops[_info.id].length - 0.0f){
                runningClips.Remove(_info);
                _info.status = ClipStatus.Exit;
                _info.lastTransistionTime = Time.time;
                
                source.PlayOneShot(exits[_info.id], _info.volume);

            }
            else if(_info.status == ClipStatus.Loop && Time.time - _info.lastTransistionTime >= loops[_info.id].length){
                
                 _info.lastTransistionTime = Time.time;
                 

                source.PlayOneShot(loops[_info.id], _info.volume * 1f);

                _info.isInterpolating = false;
                runningClips[i] = _info;

            }
            else if(_info.status == ClipStatus.Loop && Time.time - _info.lastTransistionTime >= loops[_info.id].length - interpolation[_info.id].length / 2f && !_info.isInterpolating){
                
                 

                source.PlayOneShot(interpolation[_info.id], _info.volume * 1f);

                _info.isInterpolating = true;
                runningClips[i] = _info;
                
            }
            
            

            
        }
    }
}
