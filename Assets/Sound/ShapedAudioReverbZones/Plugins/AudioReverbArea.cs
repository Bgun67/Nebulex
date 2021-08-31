using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public static class VectorExtensions
{
    public static Vector3 FlipX(this Vector3 v) => new Vector3(-v.x, v.y, v.z);
    public static Vector3 FlipY(this Vector3 v) => new Vector3(v.x, -v.y, v.z);
    public static Vector3 FlipZ(this Vector3 v) => new Vector3(v.x, v.y, -v.z);
}

public class AudioReverbArea : MonoBehaviour
{

    // Static so you can control all of them from any component.
    // Set in OnValidate
    public static bool drawGizmos;
    public static bool drawFade = true;

    public AudioReverbPreset preset;
    private AudioReverbPreset previousPreset;
    
    

    [Range(0.01f, 10f)]
    public float fadeDistance = 1f;

    [HideInInspector]
    public AudioReverbData data;
    [Range(0, 3.0f)]
    public float scaleEffect = 1f;
    public int priority = 0;
    

    public bool drawGizmo;

    [SerializeField, HideInInspector]
    protected Collider shape;
    protected virtual Color gizmoColour => new Color(1, 0.75f, 0, 0.7f);

    private void Start()
    {
        LoadReverbData();

        //From audio reverb consumer
        defaultSettings = new AudioReverbData();
        bool check = true;
        check = check && target.GetFloat("HF Reference", out defaultSettings.hfReference);
        check = check && target.GetFloat("Density", out defaultSettings.density);
        check = check && target.GetFloat("Diffusion", out defaultSettings.diffusion);
        check = check && target.GetFloat("Reverb Delay", out defaultSettings.reverbDelay);
        check = check && target.GetFloat("Reflections", out defaultSettings.reflections);
        check = check && target.GetFloat("Decay HF Ratio", out defaultSettings.decayHFRatio);
        check = check && target.GetFloat("Decay Time", out defaultSettings.decayTime);
        check = check && target.GetFloat("Room HF", out defaultSettings.roomLF);
        check = check && target.GetFloat("Room", out defaultSettings.room);
        check = check && target.GetFloat("Room LF", out defaultSettings.roomLF);
        check = check && target.GetFloat("LF Reference", out defaultSettings.lfReference);
        if (!check)
        {
            throw new UnityException("Audio reverb variables need to be exposed within the target mixer! (They are not.)");
        }
    }

    protected virtual void OnValidate()
    {
        // Collider
        if (!shape)
            shape = GetComponent<Collider>();

        if (shape)
            shape.isTrigger = true;

        drawGizmos = drawGizmo;
    }

    protected virtual void OnDrawGizmos()
    {
        if (!drawGizmos || !shape)
            return;

        Matrix4x4 originalMatrix = Gizmos.matrix;

        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = gizmoColour;

        if (shape is BoxCollider boxCollider)
        {
            Gizmos.DrawWireCube(boxCollider.center, boxCollider.size);

            Gizmos.color *= new Color(1, 1, 1, 0.5f);
            Gizmos.DrawCube(boxCollider.center, boxCollider.size);

            if (drawFade)
            {
                Gizmos.DrawWireCube(boxCollider.center, boxCollider.size + (Vector3.one * fadeDistance));

                DrawCubeCornerLine(boxCollider.size, Vector3.one);
                DrawCubeCornerLine(boxCollider.size.FlipX(), Vector3.one.FlipX());
                DrawCubeCornerLine(boxCollider.size.FlipY(), Vector3.one.FlipY());
                DrawCubeCornerLine(boxCollider.size.FlipZ(), Vector3.one.FlipZ());
            }

            void DrawCubeCornerLine(Vector3 size, Vector3 direction)
            {
                Gizmos.DrawLine(boxCollider.center + size / 2, boxCollider.center + (size / 2) + (direction * fadeDistance / 2));
                Gizmos.DrawLine(boxCollider.center - size / 2, boxCollider.center - (size / 2) - (direction * fadeDistance / 2));
            }
        }
        else if (shape is SphereCollider sphereCollider)
        {
            Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius);

            Gizmos.color *= new Color(1, 1, 1, 0.5f);
            Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);

            if (drawFade)
            {
                Gizmos.DrawWireSphere(sphereCollider.center, sphereCollider.radius + fadeDistance);

                DrawSphereFadeLine(Vector3.left);
                DrawSphereFadeLine(Vector3.right);
                DrawSphereFadeLine(Vector3.up);
                DrawSphereFadeLine(Vector3.down);
                DrawSphereFadeLine(Vector3.forward);
                DrawSphereFadeLine(Vector3.back);
            }

            void DrawSphereFadeLine(Vector3 direction)
            {
                Gizmos.DrawLine(sphereCollider.center + direction * sphereCollider.radius, direction * (sphereCollider.radius + fadeDistance));
            }
        }
        else if (shape is CapsuleCollider capsuleCollider)
        {
            Gizmos.DrawWireSphere(capsuleCollider.center + Vector3.up * (capsuleCollider.height / 2 - capsuleCollider.radius), capsuleCollider.radius);
            Gizmos.DrawWireSphere(capsuleCollider.center + Vector3.down * (capsuleCollider.height / 2 - capsuleCollider.radius), capsuleCollider.radius);

            DrawCapsuleLine(Vector3.forward);
            DrawCapsuleLine(Vector3.back);
            DrawCapsuleLine(Vector3.right);
            DrawCapsuleLine(Vector3.left);

            Gizmos.color *= new Color(1, 1, 1, 0.5f);
            Gizmos.DrawSphere(capsuleCollider.center + Vector3.up * (capsuleCollider.height / 2 - capsuleCollider.radius), capsuleCollider.radius);
            Gizmos.DrawSphere(capsuleCollider.center + Vector3.down * (capsuleCollider.height / 2 - capsuleCollider.radius), capsuleCollider.radius);

            if (drawFade)
            {
                Gizmos.DrawWireSphere(capsuleCollider.center + Vector3.up * (capsuleCollider.height / 2 - capsuleCollider.radius), capsuleCollider.radius + fadeDistance);
                Gizmos.DrawWireSphere(capsuleCollider.center + Vector3.down * (capsuleCollider.height / 2 - capsuleCollider.radius), capsuleCollider.radius + fadeDistance);
            }

            void DrawCapsuleLine(Vector3 side)
            {
                Gizmos.DrawLine(capsuleCollider.center + Vector3.up * (capsuleCollider.height / 2 - capsuleCollider.radius) + side * capsuleCollider.radius,
                capsuleCollider.center + Vector3.down * (capsuleCollider.height / 2 - capsuleCollider.radius) + side * capsuleCollider.radius);
            }
        }
        else if (shape is MeshCollider meshCollider)
        {
            Gizmos.DrawWireMesh(meshCollider.sharedMesh);

            Gizmos.color *= new Color(1, 1, 1, 0.5f);
            Gizmos.DrawMesh(meshCollider.sharedMesh);
        }

        Gizmos.matrix = originalMatrix;
    }

    private void LoadReverbData(){
        AudioReverbZone zone = gameObject.AddComponent<AudioReverbZone>();
        zone.reverbPreset = preset;
        zone.minDistance = 0f;
        zone.maxDistance = 0f;
        data = new AudioReverbData(zone);
        data.priority = priority;
        data.shape = shape;
        data.fadeDistance = fadeDistance;

        previousPreset = preset;
        
        Destroy(zone);
    }



    ///From audio reverb consumer

    public LayerMask reverbLayer;
    public AudioMixer target;
    public AudioReverbData defaultSettings;

    public static AudioReverbData globalData;
    
    Collider[] colliders = new Collider[10];

    
    void FixedUpdate()
    {
        //Reload the reverb data if it has changed during playmode
        if(previousPreset != preset)
            LoadReverbData();

        if(globalData == null){
            globalData = new AudioReverbData(defaultSettings);
        }

        
        //AudioReverbData data = new AudioReverbData(defaultSettings);
        //List<AudioReverbData> l = new List<AudioReverbData>();
        //Physics.OverlapSphereNonAlloc(transform.position, 10f, colliders, reverbLayer, QueryTriggerInteraction.Collide);

        //foreach (Collider c in colliders)
        //{
        //    if (c == null)
        //        break;

        //    AudioReverbArea d = c.GetComponent<AudioReverbArea>();

        //    if (d == null)
        //        continue;

        //    l.Add(d.data);
        //}

        //l.Sort((a, b) => a.priority.CompareTo(b.priority));

        //foreach (AudioReverbData d in l)
        //{
        //    Vector3 closestPoint = d.shape.ClosestPoint(transform.position);
        //    float dist = Vector3.Distance(closestPoint, transform.position);
        //    data = AudioReverbData.Lerp(data, d, Mathf.Clamp01((d.fadeDistance - dist) / d.fadeDistance));
        //}
        
        AudioListener mainListener = FindObjectOfType<AudioListener>();
        //float distanceToClosestPoint = Vector3.Distance(shape.bounds.center, shape.ClosestPoint(mainListener.transform.position));
        //float distanceToPoint = Vector3.Distance(shape.bounds.center, mainListener.transform.position);
        //if(distanceToPoint - distanceToClosestPoint < 0.1f){
            
        //    tmpData = data;//
        //}
        //else{
            Vector3 closestPoint = shape.ClosestPoint(mainListener.transform.position);
            float dist = Vector3.Distance(closestPoint, mainListener.transform.position);
            globalData = AudioReverbData.Lerp(globalData, this.data, Mathf.Clamp01((this.data.fadeDistance - dist) / this.data.fadeDistance) * scaleEffect);
            //(Mathf.Clamp01((this.data.fadeDistance - dist) / this.data.fadeDistance));
        //}
        
        
      

        target.SetFloat("HF Reference", globalData.hfReference);
        target.SetFloat("Density", globalData.density);
        target.SetFloat("Diffusion", globalData.diffusion);
        target.SetFloat("Reverb Delay", globalData.reverbDelay);
        target.SetFloat("Reverb", globalData.reverb);
        target.SetFloat("Reflect Delay", globalData.reflectDelay);
        target.SetFloat("Reflections", globalData.reflections);
        target.SetFloat("Decay HF Ratio", globalData.decayHFRatio);
        target.SetFloat("Decay Time", globalData.decayTime);
        target.SetFloat("Room HF", globalData.roomHF);
        target.SetFloat("Room", globalData.room);
        //target.SetFloat("Dry Level", data);
        target.SetFloat("Room LF", globalData.roomLF);
        target.SetFloat("LF Reference", globalData.lfReference);
    }

    void LateUpdate(){
        globalData = null;
    }
}
