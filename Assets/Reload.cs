using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reload : StateMachineBehaviour
{
	Player_IK player_IK;
	
	// OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
	override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateEnter(animator, stateInfo, layerIndex);
		if (!player_IK)
		{
			player_IK = animator.GetComponent<Player_IK>();
		}

	}

	// OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
	override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateUpdate(animator,stateInfo,layerIndex);
	}

	// OnStateExit is called when a transition ends and the state machine finishes evaluating this state
	override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		base.OnStateExit(animator,stateInfo,layerIndex);
	}

	// OnStateMove is called right after Animator.OnAnimatorMove()
	//override public void OnStateMove(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	//{
	//    // Implement code that processes and affects root motion
	//}

	// OnStateIK is called right after Animator.OnAnimatorIK()
	override public void OnStateIK(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
	    // Implement code that sets up animation IK (inverse kinematics)
		animator.SetFloat("Left Hand IK Blend",0f);

	}
}
