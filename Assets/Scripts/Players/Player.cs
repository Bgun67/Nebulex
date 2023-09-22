using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Mirror;
using Cinemachine;
using Unity.Services.Analytics;

public class Player : NetworkBehaviour {
	public Rigidbody rb;
	public CapsuleCollider boundingCollider;
	public Animator anim;
	protected Player_IK player_IK;

	public LayerMask magBootsLayer;
	//Access to the left and right foot data
	[HideInInspector]
	public RaycastHit lfHit;
	//[HideInInspector]
	public bool lfHitValid;
	[HideInInspector]
	public RaycastHit rfHit;

	public bool onLadder = false;
	public float forceFactor;
	public bool inVehicle = false;
	protected Transform lfRaycast;
	protected Transform rfRaycast;
	
	public Game_Controller gameController;
	protected WalkState walkState = WalkState.Walking;
	public bool useGravity;
	//to be used for moving in space
	protected float moveSpeed = 5.2f;
	protected float lookFactor = 1f;
	[SyncVar (hook = nameof(LookTimeHook))]
	protected float lookTime = 0.5f;
	protected float scopeMoveFactor = 1f;
	protected float moveFactor = 1f;
	public float currentStepHeight;
	[HideInInspector]
	public bool enteringGravity = false;
	protected bool exitingGravity = false;
	protected float jumpWait;
	protected float jumpHeldTime;
	
	public float jetpackFuel = 1f;
	public ParticleSystem[] jetpackJets;

	public bool refueling = false;
	//controls whether the player has toggle the magboots on or off
	protected bool magBootsOn;
	protected bool magBootsLock = false;

	public float airTime;
	public float suffocationTime;
	public Damage damageScript;
	public LadderController ladder;
	public MeshRenderer icon;
	

	
	[SyncVar]
	public int playerID;

	public string playerName = "Fred";
	public GameObject ragdoll;

	public Animator knifeAnim;
	protected bool counterKnife;
	public Transform flagPosition;
	public Rigidbody shipRB;

	/// <summary>
	/// The player's team. 0 being team A and 1 being team B
	/// </summary>
	public PlayerRenderer playerRenderer;
	public Material[] teamMaterials;
	public SkinnedMeshRenderer[] jerseyMeshes;

	#region CrouchHeights
	[SyncVar (hook = nameof(CrouchHook))]
	protected bool isCrouched = false;
	protected float capsule_originalHeight1 = 1f;
	protected float capsule_originalHeight2 = 1f;
	protected float capsule_originalY1 = -0.035f;
	protected float capsule_originalY2 = -0.01f;
	protected float capsule_crouchHeight1 = 0.8f;
	protected float capsule_crouchHeight2 = 0.8f;
	protected float capsule_crouchY1 = -0.132f;
	protected float capsule_crouchY2 = -0.1f;
	#endregion


	[Header("Cameras")]
	#region cameras
	public CinemachineVirtualCamera virtualCam;
	//Reference to the Cinemachine brain camera, moves around.
	[HideInInspector] public Camera mainCam;
	
	#endregion
	[Header("Sound")]
	#region sound
	protected AudioWrapper wrapper;
	protected float thrusterSoundFactor = 0f;
	public AudioSource walkSound;
	public AudioClip[] walkClips;

	#endregion
	[Space(5)]
	[Header("Weapons")]
	#region weapons
	protected bool switchWeapons = false;
	public Fire fireScript;
	public Transform finger;
	public Transform rightHandPosition;
	public GameObject magGO;
	[SyncVar (hook = "SwitchWeapons")]
	public bool primarySelected = true;
	[Header("Primary")]
	public GameObject primaryWeapon;
	public GameObject primaryWeaponPrefab;
	//Weapon identifiers
	[SyncVar (hook = "LoadWeaponData")]
	public int primaryWeaponNum;
	[SyncVar (hook = "LoadWeaponData")]
	public int secondaryWeaponNum;
	[SyncVar (hook = "LoadWeaponData")]
	public int primaryScopeNum;
	[SyncVar (hook = "LoadWeaponData")]
	public int secondaryScopeNum;
	public Vector3 primaryLocalPosition;
	public Vector3 primaryLocalRotation;
	
	protected float muzzleClimb = 0f;

	[Header("Secondary")]
	public GameObject secondaryWeapon;
	public GameObject secondaryWeaponPrefab;
	public Vector3 secondaryLocalPosition;
	public Vector3 secondaryLocalRotation;
	
	[Header("Grenades")]
	protected int grenadesNum = 4;
	public GameObject grenadePrefab;
	public GameObject grenadeModelPrefab;
	protected GameObject grenadeModel;
	protected bool throwingGrenade = false;

	public Transform grenadeSpawn;
	[Header("Grapple")]
	public bool grappleActive;

	#endregion

	//Used to lerp the space rotation
	protected Vector3 previousRot = Vector3.zero;
	protected Vector3 previousVelocity = Vector3.zero;

	public enum WalkState{
		Walking,
		Crouching,
		Running
	}
	protected virtual void Awake(){
		playerRenderer = GetComponentInChildren<PlayerRenderer>();
	}

	public override void OnStartClient(){
		playerRenderer.SetLocal(isLocalPlayer);
	}

	protected void Reload(){
		if(isLocalPlayer){
			anim.SetTrigger ("Reload");
			if (fireScript.magGO)
			{
				/*anim.MatchTarget(
					fireScript.magGO.transform.position,
					fireScript.magGO.transform.rotation,
					AvatarTarget.LeftHand,
					new MatchTargetWeightMask(Vector3.one, 1f),
					0.5f,
					1f
				);*/
			}
			Cmd_Reload();
		}
	}
	[Command]
	protected void Cmd_Reload(){
		Rpc_Reload();
	}
	[ClientRpc (includeOwner = false)]
	protected void Rpc_Reload(){
		anim.SetTrigger ("Reload");
	}
	protected virtual void LoadWeaponData(int oldValue, int newValue){

	} 

	protected void ThrowGrenade(){
		throwingGrenade = true;
		Cmd_ThrowGrenade();
	}
	[Command]
	//Start the grenage throwing animation
	protected void Cmd_ThrowGrenade(){
		Rpc_ThrowGrenade();
	}
	[ClientRpc]
	protected void Rpc_ThrowGrenade(){
		print ("Throwing Grenade");
		anim.SetTrigger ("Throw Grenade");
	}
	
	public void SpawnGrenadeModel(){
		grenadeModel = (GameObject)Instantiate (grenadeModelPrefab, grenadeSpawn);
	}

	public void SpawnGrenade(float _dropGrenadeFactor = 1f){
		
		throwingGrenade = false;
		Destroy (grenadeModel);
		
		if(!isLocalPlayer)
			return;

		//if(Input.GetButton ("Ctrl")){
			//If we are scoped, drop the grenade beside us
		//	_dropGrenadeFactor = 0.05f;
		//}
		
		Cmd_SpawnGrenade(virtualCam.transform.forward * 10.0f * _dropGrenadeFactor, grenadeSpawn.position, grenadeSpawn.rotation);
	}
	[Command]
	public void Cmd_SpawnGrenade(Vector3 _velocity, Vector3 _position, Quaternion _rotation){
		Rigidbody _grenade = ((GameObject)Instantiate (grenadePrefab, _position, _rotation)).GetComponent<Rigidbody> ();
		_grenade.velocity = _velocity;
		_grenade.GetComponent<Frag_Grenade>().fromID = playerID;
		_grenade.GetComponent<Frag_Grenade>().initialVelocity = _velocity;
		NetworkServer.Spawn(_grenade.gameObject);
		StartCoroutine(Co_DestroyGrenade(_grenade.gameObject));
	}
	public IEnumerator Co_DestroyGrenade(GameObject _grenade){
		yield return new WaitForSecondsRealtime(20.0f);
		NetworkServer.Destroy(_grenade);
	}
	
    
    //Gains air relatively slowly
    public void GainAir()
    {
        StartCoroutine(CoGainAir());
    }
    protected IEnumerator CoGainAir()
    {
        while (airTime < suffocationTime)
        {
            airTime += Time.deltaTime * 10f;
            yield return new WaitForSeconds(0.1f);
        }
        airTime = suffocationTime;
    }
	//TODO
    public virtual void LoseAir(){
		airTime -= Time.deltaTime;
	
		if (airTime < 0) {
			airTime = airTime + 1f;
			damageScript.TakeDamage (10,0, transform.position + transform.forward, true);
		}
	}
	
	
	public void OpenKnife()
	{
		knifeAnim.SetBool("Knife", true);
	}
	public void RetractKnife()
	{
		knifeAnim.SetBool("Knife", false);
	}
	public void Knife(){
		print("Attempting to knife");
		if (isLocalPlayer)
		{
			return;
		}
		RaycastHit hit;
		if (Physics.SphereCast (virtualCam.transform.position,0.01f, virtualCam.transform.forward, out hit, 2f)) {
			if (hit.transform.root.tag == "Player") {
				if (hit.transform.root.GetComponent<Animator>().GetBool("Knife"))
				{
					counterKnife = true;
					return;
				}
				StartCoroutine(Stab(hit.transform.root.gameObject));
				

			}

		}
		//TODO:
		/*
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			netView.RPC("RPC_Knife", MRPCMode.AllBuffered, new object[] { });
		}
		else
		{
			RPC_Knife();
		}*/
	}
	protected IEnumerator Stab(GameObject _otherPlayer)
	{
		float i = 0;
		while (i < 1.1f)
		{
			if (!_otherPlayer.activeInHierarchy)
			{
				break;
			}
			rb.MovePosition(Vector3.Lerp(transform.position, _otherPlayer.transform.position-transform.forward*1.1f, 0.3f));
			yield return new WaitForEndOfFrame();
			i += Time.deltaTime;
		}
		//TODO
		/*
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{
			_otherPlayer.GetComponent<MetworkView>().RPC("RPC_GetKnifed", MRPCMode.AllBuffered, new object[] {
						netObj.owner});
		}
		else
		{
			_otherPlayer.GetComponent<Player_Controller>().RPC_GetKnifed(netObj.owner);
			print("FOund player stabbing now");
		}*/
	}
	[MRPC]
	public void RPC_Knife(){
		anim.SetBool ("Knife",true);
		Invoke("StopKnife", 0.2f);
	}
	public void StopKnife()
	{
		anim.SetBool("Knife", false);

	}
	#region Region1

	protected virtual void Aim(){
		
	}
	[MRPC]
	public void RPC_Aim(bool scoped){
		
		anim.SetBool ("Scope", scoped);
	}
	
	[Command]
	public void Cmd_Jump(bool jumping, bool jets)
	{
		Rpc_Jump(jumping, jets);
	}
	[ClientRpc]
	public void Rpc_Jump(bool jumping, bool jets){
		if (jets) {
			foreach (ParticleSystem jet in jetpackJets) {
				jet.Play ();
			}
		} else {
			foreach (ParticleSystem jet in jetpackJets) {
				jet.Stop ();
			}
		}
		anim.SetBool ("Jump", jumping);
	}
	public virtual void Jump(){
		
	}
	
	

	public virtual void UseItem(){
		
	}

	
	public virtual void AnimateMovement(){
		
	}
	public virtual void FootstepAnim(){
		
		walkSound.PlayOneShot(walkClips[Random.Range(0,walkClips.Length)]);//[Mathf.Clamp(Time.frameCount % 4,0,walkClips.Length-1)]);
	
	}

	[Command]
	public void Cmd_ChangeCrouch(bool _isCrouched){
		isCrouched = _isCrouched;
	}

	public virtual void CrouchHook(bool oldVal, bool isCrouched){
		if(isCrouched){
			CapsuleCollider[] capsules = this.GetComponents<CapsuleCollider> ();
			Vector3 center = capsules [0].center;
			center.y = capsule_crouchY1;
			capsules [0].center = center;
			capsules [0].height = capsule_crouchHeight1;
			Vector3 center2 = capsules [1].center;
			center2.y = capsule_crouchY2;
			capsules [1].center = center2;
			capsules [1].height = capsule_crouchHeight2;
		}
		else{
			CapsuleCollider[] capsules = this.GetComponents<CapsuleCollider> ();
			Vector3 center = capsules [0].center;
			center.y = capsule_originalY1;
			capsules [0].center = center;
			capsules [0].height = capsule_originalHeight1;
			Vector3 center2 = capsules [1].center;
			center2.y = capsule_originalY2;
			capsules [1].center = center2;
			capsules [1].height = capsule_originalHeight2;

			walkState = WalkState.Walking;
		}
	}
	
	[MRPC]
	public virtual void RPC_ExitVehicle()
	{
		airTime = suffocationTime;
		/*TODO
		if (Metwork.peerType != MetworkPeerType.Disconnected)
		{

			netView.RPC("RPC_Sit", MRPCMode.AllBuffered, new object[] { false });
		}
		else
		{
			RPC_Sit(false);
		}*/
		inVehicle = false;
		//player.rb.isKinematic = false;
		//TODO: This may suffer an IEnumerator problem
		ExitGravity();
		CapsuleCollider[] capsules = GetComponents<CapsuleCollider>();
		capsules[0].enabled = true;
		capsules[1].enabled = true;
	}


	[Command]
	public void Cmd_SetLookTime(float time){
		lookTime = time;
	}
	
	public void LookTimeHook(float oldVal, float _lookTime)
	{
		if(!isLocalPlayer){
			float _time = anim.GetFloat("Look Speed");
			anim.SetFloat("Look Speed", Mathf.Lerp(_time, _lookTime, 0.5f));
		}
	}
	


	[MRPC]
	public void RPC_ClimbLadder(){
		anim.SetBool ("Climb Ladder", true);
	}
	[MRPC]
	public void RPC_LeaveLadder(){
		anim.SetBool ("Climb Ladder", false);
	}

	public virtual void Attack(){
		if (!grappleActive)
		{
			//Spread of the gun
			fireScript.shotSpawn.transform.forward = this.virtualCam.transform.forward 
													+ muzzleClimb * fireScript.recoilAmount * virtualCam.transform.up * 0.3f
													+ fireScript.recoilAmount * (0.1f + muzzleClimb) * (Vector3)(Random.insideUnitCircle) * (anim.GetBool ("Scope") ? 0.1f : 0.3f)
													;
			if(muzzleClimb < 0.6f){
				muzzleClimb += 0.05f;
			}
			bool fired = fireScript.FireWeapon(fireScript.shotSpawn.transform.position, fireScript.shotSpawn.transform.forward);
			Cmd_FireWeapon(fireScript.shotSpawn.transform.position, fireScript.shotSpawn.transform.forward);

			if (fired) { player_IK.Recoil(fireScript.recoilAmount); };
		}
		//TODO:
		//UpdateUI ();
	}
	[Command]
	protected void Cmd_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward){
		if(isServerOnly) fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
		Rpc_FireWeapon(shotSpawnPosition, shotSpawnForward);
	}
	[ClientRpc(includeOwner=false)]
	protected void Rpc_FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward){
		if(fireScript){
			fireScript.FireWeapon(shotSpawnPosition, shotSpawnForward);
			//TODO show the icon here
			Color _originalColor = icon.material.color;
			_originalColor.a = 1.0f;
			icon.material.color = _originalColor;
		}
	}

	#endregion
	#region Region2
	[Command]
	public void Cmd_KillPlayer(){
		damageScript.TakeDamage (1000, 0, transform.position, true);
	}
	public virtual void Die(){
		
	}

	[ClientRpc]
	public virtual void Rpc_Die(){
		
	}
	public virtual void CoDie(){
		
	}

	//Called by grav controller when entering / exiting gravity;
	public virtual IEnumerator ExitGravity(){
		yield return new WaitForSeconds(0.1f);
	}
	public virtual IEnumerator EnterGravity(){
		yield return new WaitForSeconds(0.1f);
	}
	[Command]
	public void Cmd_SwitchWeapons(bool _primary){
		primarySelected = _primary;
		if(isServerOnly)SwitchWeapons(primarySelected, primarySelected);
		
	}
	public void SwitchWeapons(bool oldValue, bool newValue){
		StartCoroutine (Co_SwitchWeapons (primarySelected));
	}
	
	public virtual IEnumerator Co_SwitchWeapons(bool _primary){
		anim.SetBool ("Switch Weapons", true);
		switchWeapons = true;
		yield return new WaitForSeconds (0.8f);
		if (!_primary) {
			primaryWeapon.SetActive (false);
			secondaryWeapon.SetActive (true);
			fireScript = secondaryWeapon.GetComponent<Fire> ();
		} else {
			secondaryWeapon.SetActive (false);
			primaryWeapon.SetActive (true);
			fireScript = primaryWeapon.GetComponent<Fire> ();
		}	
		
		fireScript.playerID = playerID;
		//We want to move the right hand target back and forth depending how long the gun is
		player_IK.rhOffset = fireScript.rhOffset;
		player_IK.gunPosition = rightHandPosition;
		player_IK.gripPosition = fireScript.lhTarget;
		if (fireScript.lhHint)
		{
			player_IK.lhHint = fireScript.lhHint;
		}
		yield return new WaitForSeconds (0.66f);
		switchWeapons = false;
		anim.SetBool ("Switch Weapons", false);
		fireScript.playerID = playerID;

		anim.SetFloat ("Drift", fireScript.bulk);
		anim.SetFloat ("Left Hand Grip", fireScript.leftGripSize);

		
	}
	
	public int GunNameToInt(string _name){
		for (int i = 0; i < Game_Controller.Instance.weaponsCatalog.guns.Length; i++){
			if(Game_Controller.Instance.weaponsCatalog.guns[i].name == _name){
				return i;
			}
		}
		return -1;
	}
	public int ScopeNameToInt(string _name){
		for (int i = 0; i < Game_Controller.Instance.weaponsCatalog.scopes.Length; i++){
			if(Game_Controller.Instance.weaponsCatalog.scopes[i].name == _name){
				return i;
			}
		}
		return -1;
	}
	
	///<summary>
	///Returns the team of this player
	///</summary>
	public int GetTeam(){
		return Game_Controller.Instance.playerStats[playerID].team;
	}
	///<summary>
	///Returns the name of this bot
	///</summary>
	public string GetName(){
		return Game_Controller.Instance.playerStats[playerID].name;
	}
	
	#endregion
}
