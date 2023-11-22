using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum FireTypes
{
    SemiAuto,
    FullAuto,
    Burst,
}

[CreateAssetMenu(fileName = "Weapon Properties", menuName = "ScriptableObjects/WeaponProperties", order = 3)]
public class WeaponProperties : ScriptableObject
{
    [field: SerializeField] public string FriendlyName { get; private set; }
    [field: SerializeField] public Sprite PreviewImage { get; private set; }

    [Tooltip("An offset to be applied to the position of the right hand to accomodate longer/shorter guns")]
    [field: SerializeField] public Vector3 RhOffset { get; private set; }
    [field: SerializeField] public float LeftGripSize { get; private set; } = 1f;
    [field: SerializeField] public float ReloadTime { get; private set; } = 2;
    [Tooltip("Rounds Per Minute")]
    [field: SerializeField] public float FireRate { get; private set; } = 600;
    [field: SerializeField] public int MagSize { get; private set; } = 25;
    [field: SerializeField] public int MaxAmmo { get; private set; } = 500;
    [field: SerializeField] public int DamagePower { get; private set; } = 20;
    [field: SerializeField] public float BulletVelocity { get; private set; } = 700;
    [field: SerializeField] public FireTypes FireType { get; private set; } = FireTypes.FullAuto;
    [field: SerializeField] public int SkillLevel { get; private set; } = 0;

    [field: SerializeField] public List<string> UnavailableScopes { get; private set; } = new List<string>();
    [field: SerializeField] public AudioClip TriggerClick { get; private set; }
    [field: SerializeField] public AudioClip CockSound { get; private set; }
    [field: SerializeField] public AudioClip ShootSound { get; private set; }

    [Tooltip("0-1 How much the weapon recoils")]
    [field: SerializeField] public float RecoilAmount { get; private set; } = 0.5f;
    [Tooltip("0-1 How much the weapon drifts")]
    [field: SerializeField] public float Bulk { get; private set; } = 0.5f;

    [Tooltip("DO fired bullets start with the parents velocity?")]
    [field: SerializeField] public bool IgnoreParentVelocity { get; private set; }
}
