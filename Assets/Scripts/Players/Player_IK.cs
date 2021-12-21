﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_IK : MonoBehaviour
{
    public Transform rhTarget;
    [HideInInspector]
    public Vector3 rhOffset = Vector3.zero;
    public Transform lhTarget;
    public float footOffset = 0.1f;

    Vector3 rfTargetPos;
    Quaternion rfTargetRot;
    Vector3 lfTargetPos;
    Quaternion lfTargetRot;
    

    private Animator anim;
    private Player_Controller player;
    private bool isBot = false;

    // Start is called before the first frame update
    void Start()
    {
        anim = this.GetComponent<Animator>();   
        player = this.GetComponent<Player_Controller>();
        if(player==null)
            isBot = true;
    }

    void Update(){
        


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
            anim.SetIKRotationWeight(AvatarIKGoal.RightHand,rhBlend);
            if(player != null)  
                anim.SetIKPosition(AvatarIKGoal.RightHand,rhTarget.position + rhOffset.z* player.finger.transform.forward);
            else
                anim.SetIKPosition(AvatarIKGoal.RightHand,rhTarget.position);
            anim.SetIKRotation(AvatarIKGoal.RightHand,rhTarget.rotation);
        }
        if(lhTarget != null) {
            anim.SetIKPositionWeight(AvatarIKGoal.LeftHand,lhBlend);
            anim.SetIKRotationWeight(AvatarIKGoal.LeftHand,lhBlend);  
            anim.SetIKPosition(AvatarIKGoal.LeftHand,lhTarget.position);
            anim.SetIKRotation(AvatarIKGoal.LeftHand,lhTarget.rotation);
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
