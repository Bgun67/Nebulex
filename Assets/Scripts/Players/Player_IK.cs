using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_IK : MonoBehaviour
{
    public Transform rhTarget;
    [HideInInspector]
    public Vector3 rhOffset = Vector3.zero;
	public Vector3 rhPosition = Vector3.zero;

	public Transform lhTarget;
	public Transform lhHint;
	public float scopeOffset = 0.2f;
	float scopedFactor;

	public float footOffset = 0.1f;

	Vector3 rfTargetPos;
    Quaternion rfTargetRot;
    Vector3 lfTargetPos;
    Quaternion lfTargetRot;

	Transform grabTarget;
	Vector3 grabPoint;
	Quaternion grabNormal;


	private Animator anim;
    private Player_Controller player;
    private bool isBot = false;

	[Header("Recoil")]
	float recoilStartTime;
	float recoilMagnitude;
	[SerializeField]
	AnimationCurve recoilCurve;

	// Start is called before the first frame update
	void Start()
    {
        anim = this.GetComponent<Animator>();   
        player = this.GetComponent<Player_Controller>();
        if(player==null)
            isBot = true;
    }
	void Update()
	{
		if (grabTarget)
		{
			if ((grabPoint - player.mainCamObj.transform.position).magnitude > 1f)
			{
				grabTarget = null;
			}
		}
		//CheckGrab();
	}

	public void Scope(bool isScoped)
	{
		if (isScoped)
		{
			scopedFactor = Mathf.Lerp(scopedFactor, scopeOffset, 0.5f);
		}
		else
		{
			scopedFactor = Mathf.Lerp(scopedFactor, 0f, 0.5f);
		}
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
		return -player.transform.forward * recoilMagnitude * recoilCurve.Evaluate(Time.time - recoilStartTime);
	}

	/*void CheckGrab()
	{
		if (player)
		{
			if (player.rb.useGravity)
			{
				return;
			}
			if (grabTarget == null)
			{
				RaycastHit hit;
				Physics.Raycast(player.mainCamObj.transform.position + player.transform.right * -0.1f, player.mainCamObj.transform.forward * 0.1f, out hit, 1f);
				if (hit.transform != null)
				{
					grabTarget = hit.transform;
					grabPoint = hit.point;
					grabNormal = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;
				}
			}

		}
	}*/

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
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand,rhBlend);
            if(player != null){
				Vector3 targetPosition = rhTarget.position + rhOffset.z * player.finger.transform.forward+player.transform.up*scopedFactor+GetRecoil();
				rhPosition = Vector3.Lerp(rhPosition, targetPosition, 0.5f);
			}
			else
			{
				rhPosition =  rhTarget.position;
			}
			anim.SetIKPosition(AvatarIKGoal.RightHand,rhPosition);
			anim.SetIKRotation(AvatarIKGoal.RightHand,rhTarget.rotation);
        }
		if (grabTarget != null)
		{
			anim.SetIKPositionWeight(AvatarIKGoal.LeftHand,lhBlend);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand,lhBlend);  
			anim.SetIKPosition(AvatarIKGoal.LeftHand,grabPoint);
            anim.SetIKRotation(AvatarIKGoal.LeftHand,lhTarget.rotation);
		}
		else if(lhTarget != null) {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand,lhBlend);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand,lhBlend);  
            anim.SetIKPosition(AvatarIKGoal.LeftHand,lhTarget.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand,lhTarget.rotation);
			if (lhHint)
			{
				anim.SetIKHintPositionWeight(AvatarIKHint.LeftElbow,lhBlend);  
				anim.SetIKHintPosition(AvatarIKHint.LeftElbow, lhHint.position);
			}
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
