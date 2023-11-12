using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IKCurve : StateMachineBehaviour
{
    [SerializeField] AnimationCurve curve;
    [SerializeField] AvatarIKGoal[] goals;
    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    //override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    //override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    //override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    
    //}

    // OnStateMove is called right after Animator.OnAnimatorMove()
    //override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    //{
    //    // Implement code that processes and affects root motion
    //}

    // OnStateIK is called right after Animator.OnAnimatorIK()
    override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        float targetIkWeight = curve.Evaluate(stateInfo.normalizedTime);
        float t = 1;
        if(animator.IsInTransition(layerIndex)){
            bool forwardTransition = animator.GetNextAnimatorStateInfo(layerIndex).fullPathHash == stateInfo.fullPathHash?true:false;
            if(forwardTransition){
                t = animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime;
            }
            else{
                t = 1 - animator.GetAnimatorTransitionInfo(layerIndex).normalizedTime;
            }
        }
        
        for(int i=0; i<goals.Length; i++){
            float ikWeight = Mathf.Lerp(animator.GetIKPositionWeight(goals[i]), targetIkWeight, t);
            animator.SetIKPositionWeight (goals[i], ikWeight);
            animator.SetIKRotationWeight (goals[i], ikWeight);
        }
    }
}
