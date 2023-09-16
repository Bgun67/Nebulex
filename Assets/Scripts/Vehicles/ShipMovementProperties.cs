using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Data", menuName = "ScriptableObjects/ShipMovementProperties", order = 1)]
public class ShipMovementProperties : ScriptableObject
{
    public float cruiseVelocity = 1;
	[Tooltip("Factor of the thrust that can rotate the ship")]
	public float angularSpeed = 10f;

    [Header("Force")]
    public float thrustFactor = 2;
    public float hoverForceFactor = 50f;
    public float movementLerpRate = 0.3f;
    public float boostFactor = 3f;

    [Header("Angular")]
    public float pitchFactor = 0.2f;
    public float yawFactor = 0.2f;
    public float rollFactor = 0.2f;
    public float angularLerpRate = 0.5f;

}
