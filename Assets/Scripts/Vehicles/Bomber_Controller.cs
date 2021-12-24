using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

/*Michael Gunther: 2018-02-05
 * Purpose: Ship class to be derived from to make the fighter ship, bombers etc
 * Notes: 
 * Improvements/Fixes:
* */

public class Bomber_Controller : Ship_Controller
{

	public enum WeaponType
	{
		Primary,
		Secondary,
		Tertiary
	}

	WeaponType weaponType = WeaponType.Primary;


	public Fire bombFireScript;

	public bool isPilot;
	public bool landMode;
	
	public GameObject carrierPointer;
	public Transform carrierOne;
	public Transform carrierTwo;

	Coroutine addDamage;

	

	[MRPC]
	public override void RPC_Exit()
	{
		base.RPC_Exit();
		if (anim != null)
		{
			anim.SetBool("Should Close", false);
		}
	}

	[MRPC]
	public override void RPC_Activate(int _pilot)
	{
		base.RPC_Activate(_pilot);
		
		if (anim != null) {
			anim.SetBool ("Should Close", true);
		} 
	}
	

	// Update is called once per frame
	protected override void ShipUpdate ()
	{

		base.ShipUpdate();
		if(Input.GetButton("Switch Weapons")){
			SwitchWeapons ();
		}
		if(Input.GetButtonDown("Jump")){
			if (Metwork.peerType != MetworkPeerType.Disconnected) {
				if (anim.GetBool ("Lower Gear")) {
					netObj.netView.RPC ("RPC_LowerGear", MRPCMode.AllBuffered, new object[]{ false });
				} else {
					netObj.netView.RPC ("RPC_LowerGear", MRPCMode.AllBuffered, new object[]{ true });

				}
			} else {
				if (anim.GetBool ("Lower Gear")) {
					RPC_LowerGear (false);
				} else {
					RPC_LowerGear (true);
				}
			}
		}



	}
			[MRPC]
	void RPC_LowerGear(bool lower){
		
		anim.SetBool ("Lower Gear", lower);
	}


	public void SwitchWeapons(){
		//Cycle through the weapons
		switch (weaponType) {
			case WeaponType.Primary:
				weaponType = WeaponType.Secondary;
				break;
			case WeaponType.Secondary:
				//TODO: Change this to tertiary when a tertiary weapon becomes available
				weaponType = WeaponType.Primary;
				break;
			case WeaponType.Tertiary:
				weaponType = WeaponType.Primary;
				break;
			default:
				break;
		}
	}
	public override void Fire ()
	{
		if (weaponType == WeaponType.Primary) {
			fireScriptLeft.FireWeapon(fireScriptLeft.shotSpawn.transform.position, fireScriptLeft.shotSpawn.transform.forward);
			Cmd_FireWeapon(fireScriptLeft.shotSpawn.transform.position, fireScriptLeft.shotSpawn.transform.forward, 0);
			fireScriptRight.FireWeapon (fireScriptRight.shotSpawn.transform.position, fireScriptRight.shotSpawn.transform.forward);
			Cmd_FireWeapon(fireScriptRight.shotSpawn.transform.position, fireScriptRight.shotSpawn.transform.forward, 1);
		}
		if (weaponType == WeaponType.Secondary) {
			bombFireScript.FireWeapon (bombFireScript.shotSpawn.transform.position, bombFireScript.shotSpawn.transform.forward);
			Cmd_FireWeapon(bombFireScript.shotSpawn.transform.position, bombFireScript.shotSpawn.transform.forward, 2);
		}
	}
	[Command]
	void Cmd_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward, int gunNum){
		Fire fireScript;
		switch(gunNum){
			case 0:
				fireScript = fireScriptLeft;
				break;
			case 1:
				fireScript = fireScriptRight;
				break;
			case 2:
				fireScript = bombFireScript;
				break;
			default:
				fireScript = fireScriptLeft;
				break;
		}
		if(isServerOnly) fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
		Rpc_FireWeapon(shotSpawnPosition, shotSpawnForward, gunNum);
	}
	[ClientRpc(includeOwner=false)]
	void Rpc_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward, int gunNum){
		Fire fireScript;
		switch(gunNum){
			case 0:
				fireScript = fireScriptLeft;
				break;
			case 1:
				fireScript = fireScriptRight;
				break;
			case 2:
				fireScript = bombFireScript;
				break;
			default:
				fireScript = fireScriptLeft;
				break;
		}
		fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
	}


}


