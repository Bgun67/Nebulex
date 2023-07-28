﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_IK : MonoBehaviour
{	
	public Transform gunPosition;
	Transform gunTarget;
	[HideInInspector] public  Transform gripPosition;
	Transform gripTarget;

    [HideInInspector] public Transform rhTarget;
    [HideInInspector]
    public Vector3 rhOffset = Vector3.zero;
	public Vector3 rhPosition = Vector3.zero;
	Quaternion rh2GoalOffset;
	Quaternion lh2GoalOffset;

	[HideInInspector] public Transform lhTarget;
	public Transform lhHint;
	public Vector3 scopeOffset = Vector3.zero;
	float scopedFactor;
	float sprintFactor;

	public float footOffset = 0.1f;

	Vector3 rfTargetPos;
    Quaternion rfTargetRot;
	Vector3 lfTargetPos;
    Quaternion lfTargetRot;
	Vector3 currentLocalVelocity;
	float currentTurning;

	private Animator anim;
    private Player_Controller player;
    private bool isBot = false;

	[Header("Recoil")]
	float recoilStartTime;
	float recoilMagnitude;
	[SerializeField]
	AnimationCurve recoilCurve;

	// Start is called before the first frame update
	void Awake()
    {
        anim = this.GetComponentInChildren<Animator>();
		anim.GetComponent<IK_Forwarder>().onAnimatorIK += OnAnimatorIK;   
        player = this.GetComponent<Player_Controller>();
		gripTarget = new GameObject("Grip Target").transform;
		gripTarget.transform.parent = transform;
		gunTarget = new GameObject("Gun Target").transform;
		gunTarget.transform.parent = transform;

        if(player==null)
            isBot = true;
    }

	public void Scope(bool isScoped)
	{
		if (isScoped)
		{
			scopedFactor = Mathf.Lerp(scopedFactor, 1, 0.5f);
		}
		else
		{
			scopedFactor = Mathf.Lerp(scopedFactor, 0f, 0.5f);
		}
	}
	public void Sprint(bool isSprinting)
	{
		sprintFactor = Mathf.Lerp(sprintFactor,isSprinting?1f:0f, 0.3f);
	}
	public bool IsSprinting()
	{
		return sprintFactor>0.1f;
	}
	
	public void Recoil(float magnitude)
	{
		recoilMagnitude = magnitude;
		recoilStartTime = Time.time;
	}
	Vector3 GetRecoil()
	{
		if (Time.time - recoilStartTime > 1)
		{
			return Vector3.zero;
		}
		return -Vector3.forward * recoilMagnitude * recoilCurve.Evaluate(Time.time - recoilStartTime);
	}

	public void SetVelocity(Vector3 currentVelocity)
	{
		currentLocalVelocity = transform.InverseTransformVector(currentVelocity);
	}

	public void SetVelocity(Vector3 currentVelocity, float turning = 0)
	{
		currentLocalVelocity = transform.InverseTransformVector(currentVelocity);
		currentTurning = Mathf.Lerp(currentTurning,Mathf.Clamp(turning, -1,1f), 0.5f);
	}

	void CalculateHandPositions(){
		Vector3 offset = gunPosition.TransformVector(GetRecoil()+Vector3.Lerp(Vector3.zero, scopeOffset, scopedFactor))-transform.TransformVector(currentLocalVelocity*0.01f);
		gunTarget.position = gunPosition.position + offset;
		gunTarget.rotation = gunPosition.rotation*
				Quaternion.AngleAxis(currentTurning*15f,-Vector3.right)*
				Quaternion.AngleAxis(GetRecoil().z*30f,Vector3.up)*
				Quaternion.AngleAxis(sprintFactor*45f,Vector3.right);

		print(gripPosition.parent.parent.parent);
		Transform handTransform =  anim.GetBoneTransform(HumanBodyBones.RightHand);
		Vector3 gripOffset = handTransform.InverseTransformVector(gripPosition.position-handTransform.position);
		//Matrix4x4.Rotate(gunTarget.rotation)*Matrix4x4.Rotate(rh2GoalOffset)*Matrix4x4.Translate(gripOffset)
		Debug.DrawLine(handTransform.position, handTransform.position+handTransform.TransformVector(gripOffset), Color.green);
		Debug.DrawLine(gunTarget.position, gunTarget.position+gunTarget.TransformVector(gripOffset) , Color.red);
		gripTarget.position = handTransform.position+handTransform.TransformVector(gripOffset);
		gripTarget.rotation = gripPosition.rotation;
		
		rhTarget = gunTarget;
		lhTarget = gripTarget;
	}

	void FixedUpdate(){
		/*if(rhTarget != null){
            if(player != null){
				Vector3 targetPosition = rhTarget.position + rhOffset.z * player.finger.transform.forward+player.transform.up*scopedFactor+GetRecoil();
				rhPosition = Vector3.Slerp(rhPosition, targetPosition, 0.99f);
			}
			else
			{
				rhPosition =  rhTarget.position;
			}
        }*/
		CalculateHandPositions();

	}
	

	void OnAnimatorIK(){
        //Pull the latest raycast data from the player
        if(!isBot && player.rfHit.distance <= 1f){
            rfTargetPos = player.rfHit.point + 0f*transform.up * footOffset;
            rfTargetRot = Quaternion.FromToRotation(transform.up, player.rfHit.normal) * transform.rotation;
        }
        if(!isBot && player.lfHit.distance <= 1f){
            lfTargetPos = player.lfHit.point + 0f*transform.up * footOffset;
            lfTargetRot = Quaternion.FromToRotation(transform.up, player.lfHit.normal) * transform.rotation;
        }

        float rfBlend = anim.GetFloat("Right Foot IK Blend");
        float lfBlend = anim.GetFloat("Left Foot IK Blend");

        float lhBlend = anim.GetFloat("Left Hand IK Blend");
        float rhBlend = anim.GetFloat("Right Hand IK Blend");
    
        if(rhTarget != null) {
            anim.SetIKPositionWeight(AvatarIKGoal.RightHand,rhBlend);
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand,1);
           
			anim.SetIKPosition(AvatarIKGoal.RightHand,rhTarget.position);
			Debug.DrawRay(rhTarget.position, rhTarget.forward, Color.blue, 0.1f);
			anim.SetIKRotation(
				AvatarIKGoal.RightHand,
				rhTarget.rotation
			);

			Quaternion GR = rhTarget.rotation;
			Quaternion SR = anim.GetBoneTransform(HumanBodyBones.RightHand).rotation;
			rh2GoalOffset = Quaternion.Inverse(SR) * GR;

        }
        if(lhTarget != null) {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand,lhBlend);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand,lhBlend);  
            anim.SetIKPosition(AvatarIKGoal.LeftHand,lhTarget.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand,lhTarget.rotation);
			Debug.DrawLine(lhTarget.position, lhTarget.position + Vector3.up, Color.yellow);
			if (lhHint)
			{
				anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow,lhBlend*(sprintFactor>0.1f?0:1));  
				anim.SetIKHintPosition(AvatarIKHint.LeftElbow, lhHint.position);
			}

			Quaternion GR = lhTarget.rotation;
			Quaternion SR = anim.GetBoneTransform(HumanBodyBones.LeftHand).rotation;
			lh2GoalOffset = Quaternion.Inverse(SR) * GR;
		}
        if(!isBot && player.rfHit.distance <= 1f){
            //Right foot
            //anim.SetIKPositionWeight(AvatarIKGoal.RightFoot,rfBlend);
            //anim.SetIKRotationWeight(AvatarIKGoal.RightFoot,rfBlend);  
            //anim.SetIKPosition(AvatarIKGoal.RightFoot,rfTargetPos);
            //anim.SetIKRotation(AvatarIKGoal.RightFoot,rfTargetRot);
        }
        else{
            //anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot,0);
            //anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot,0);
        }
        if(!isBot && player.lfHitValid){
            //Left foot
            //anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot,lfBlend);
            //anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot,lfBlend);
            //anim.SetIKHintPositionWeight(AvatarIKHint.LeftKnee, 1f);  
            //anim.SetIKPosition(AvatarIKGoal.LeftFoot,lfTargetPos);
            //anim.SetIKRotation(AvatarIKGoal.LeftFoot,lfTargetRot);
            //anim.SetIKHintPosition(AvatarIKHint.LeftKnee, transform.forward);
        }else{
            //anim.SetIKPositionWeight(AvatarIKGoal.LeftFoot,Mathf.Lerp(anim.GetIKPositionWeight(AvatarIKGoal.LeftFoot),0,0.3f));
            //anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot,Mathf.Lerp(anim.GetIKPositionWeight(AvatarIKGoal.LeftFoot),0,0.3f));
            //anim.SetIKRotationWeight(AvatarIKGoal.LeftFoot,0);
        }
    }




}
