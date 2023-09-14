using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "Difficulty Level Easy", menuName = "ScriptableObjects/Com Difficulty", order = 2)]
public class ComDifficulty : ScriptableObject
{
    [field: SerializeField] public float AttentionSpan { get; private set;}= 10;
    [field: SerializeField] public float ViewDistance { get; private set;}= 50;
    [Tooltip("Boresight View Angle")]
    [field: SerializeField] public float ViewAngle{ get; private set;} = 70;
    [field: SerializeField] public float ListenDistance{ get; private set;} = 70;
    [field: SerializeField] public float PatrolRotationSpeed{ get; private set;} = 0.75f;

    
    [field: SerializeField] public float FiringRange{ get; private set;} = 20f;
    [field: SerializeField] public float AdvanceDistance{ get; private set;} = 10;
    [field: SerializeField] public float FightingRotationSpeed{ get; private set;} = 0.75f;
    [field: SerializeField] public float LockOnRate{ get; private set;} = 0.4f;

    [field: SerializeField] public float MaxAimOffsetBeforeFiring{ get; private set;} = 20f;
    [field: SerializeField] public AnimationCurve AccuracyOverTime{ get; private set;}
    [field: SerializeField] public float DamageFactor{ get; private set;} = 0.3f;

}
