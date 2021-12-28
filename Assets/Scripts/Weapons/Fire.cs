﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Fire : MonoBehaviour {
	public enum FireTypes
	{
		SemiAuto,
		FullAuto,
		Burst,
	}
	#region GunInfo
	[Header("Gun Info")]
	float fireDelay;
	public Transform shotSpawn;
	//This is rarely used and many need to be removed
	public Transform rhTarget;
	[Tooltip("An offset to be applied to the position of the right hand to accomodate longer/shorter guns")]
	public Vector3 rhOffset;
	public Transform lhTarget;
	public Transform lhHint;
	public float reloadTime;
	[Tooltip("Rounds Per Minute")]
	public float fireRate;
	public int magSize;
	[HideInInspector]
	public int magAmmo;
	public int maxAmmo;
	public int totalAmmo;
	public GameObject bulletPrefab;
	public int damagePower;
	public float bulletVelocity;
	public FireTypes fireType;
	public int skillLevel;
	#endregion
	public List<string> unavailableScopes = new List<string> ();
	[Space()]
	[Header("Sound")]
	public AudioSource shootSound;
	public AudioClip triggerClick;
	public AudioClip cockSound;
	[HideInInspector]
	public AudioClip sound;

	[Space()]
	public WaitForSeconds reloadWait;
	public Animator weaponAnim;
	[Tooltip("0-1 bitches!")]
	public float recoilAmount = 0.5f;
	[Tooltip("0-1 How much the weapon drifts")]
	[Range(0,1)]
	public float bulk = 0.5f;

	public int aimAnimNumber = 1;
	public Transform scopePosition;
	int fired = 0;
	[Tooltip("DO fired bullets start with the parents velocity?")]
	public bool ignoreParentVelocity;
	Rigidbody rootRB;
	public MetworkView netView;
	public ParticleSystem muzzleFlash;
	public int playerID;
	public bool reloading;
	public delegate void ReloadEvent();
	public ReloadEvent OnReloadEvent;
	public GameObject magGO;

	public Stack<GameObject> poolList = new Stack<GameObject> ();
	public Stack<GameObject> destroyedStack = new Stack<GameObject> ();


	// Use this for initialization
	void Awake()
	{
		Invoke("Setup", Random.Range(0.1f, 0.2f));
	}
	void Setup()
	{
		reloadWait = new WaitForSeconds(reloadTime);
		shootSound = this.GetComponent<AudioSource>();
		sound = shootSound.clip;
		weaponAnim = this.GetComponent<Animator>();
		fireRate = 1f / (fireRate / 60f);

		if (!ignoreParentVelocity)
		{
			rootRB = transform.root.GetComponent<Rigidbody>();
		}

		if (netView == null)
		{
			netView = this.GetComponent<MetworkView>();
		}
		if (netView == null)
		{
			netView = this.GetComponentInParent<MetworkView>();
		}
		magAmmo = magSize;
		totalAmmo = maxAmmo;

		Invoke(nameof(CreateObjectPool), Random.Range(0, 0.01f));
		//
	}

	public void Activate(GameObject player){
		/*Player_Controller controller = player.GetComponent<Player_Controller> ();
		string loadoutSettingsString = "";
		string[] loadoutSettings = Util.LushWatermelon(System.IO.File.ReadAllLines (Application.streamingAssetsPath+"/Loadout Settings.txt"));
		if (controller.primarySelected) {
			loadoutSettings [0] = this.name.Replace("(Clone)(Clone)", "");
		} else {

			loadoutSettings [1] = this.name.Replace("(Clone)(Clone)", "");

		}
		foreach(string line in loadoutSettings){
			loadoutSettingsString+=line+'/';
		}
		if(Metwork.peerType  != MetworkPeerType.Disconnected){
			
			controller.netView.RPC("RPC_LoadWeaponData",MRPCMode.AllBuffered, new object[]{loadoutSettingsString});
		}
		else{
			controller.RPC_LoadWeaponData(loadoutSettingsString);
		}
		print ("Destroying Gun");
		Destroy (this.gameObject);*/
	}
	
	// Update is called once per frame
	void Update(){
		if (!Input.GetButton ("Fire1")) {
			fired = 0;
		}
		

	}
	//I've ignored sending the inherited velocity, I think the server will be up to date enought
	//on it so the difference will be small.
	public bool FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward){
		
		if (!reloading) {
			
			if (fireType == FireTypes.SemiAuto) {
			
				if (fired > 0) {
					return false;
				} 
			} else if (fireType == FireTypes.Burst) {

				if (fired > 2) {
					return false;
				} 
			}
			if (Time.time >= fireDelay) {
				
				if (magAmmo > 0) {
					fireDelay = Time.time + fireRate;
					GameObject bullet = GetBullet ();
					
					bullet.transform.position = shotSpawnPosition;
					bullet.transform.forward = shotSpawnForward;
					fired++;
					if (weaponAnim != null) {
						try {
							weaponAnim.SetTrigger ("Fire");
						} catch {

						}
					}
		
					if (ignoreParentVelocity) {
						bullet.GetComponent<Rigidbody> ().velocity = (shotSpawnForward) * bulletVelocity;
					} else {
						bullet.GetComponent<Rigidbody> ().velocity = rootRB.GetPointVelocity(transform.position) + (shotSpawnForward) * bulletVelocity;
					}
					bullet.GetComponent<Bullet_Controller> ().damagePower = damagePower;

					if (shootSound != null) {
						shootSound.PlayOneShot (sound, 1f);
					}
					if (muzzleFlash != null) {
						muzzleFlash.Play ();
					}
					bullet.GetComponent<Bullet_Controller> ().fromID = playerID;

					magAmmo--;
					if (magAmmo < 1) {
					//	StartCoroutine (Reload ());

					}
					return true;


				} 
				if (magAmmo <= 0) {
					if (triggerClick != null)
					{
						shootSound.PlayOneShot(triggerClick, 0.3f);
					}
					StartCoroutine (Reload ());

				}
			}
		}
		return false;

	}
	/*No longer needed
	[Command]
	void Cmd_FireWeapon(Vector3 _position, Quaternion _rotation, Vector3 _velocity, int _damagePower, int _ID){
		//Fire the gun on the clients
		Rpc_FireWeapon(_position, _rotation,_velocity, _damagePower, _ID);
		
		GameObject bullet = GetBullet ();
		bullet.transform.position = _position;
		bullet.transform.rotation = _rotation;

		fired++;
		bullet.GetComponent<Rigidbody> ().velocity = _velocity;
		bullet.GetComponent<Bullet_Controller> ().damagePower = _damagePower;
		bullet.GetComponent<Bullet_Controller> ().fromID = _ID;
		

	}
	//CHECK: It is possible that this runs on all the clients (so it is repeated on the original client)
	[ClientRpc]
	void Rpc_FireWeapon(Vector3 _position, Quaternion _rotation, Vector3 _velocity, int _damagePower, int _ID){
		if (weaponAnim != null) {
			
			//weaponAnim.SetTrigger ("Fire");
		}
		if (shootSound != null) {
			shootSound.PlayOneShot (sound, 1f);
		}
		if (muzzleFlash != null) {
			muzzleFlash.Play ();
		}
		GameObject bullet = GetBullet ();
		bullet.transform.position = _position;
		bullet.transform.rotation = _rotation;

		fired++;
		bullet.GetComponent<Rigidbody> ().velocity = _velocity;
		bullet.GetComponent<Bullet_Controller> ().damagePower = _damagePower;
		bullet.GetComponent<Bullet_Controller> ().fromID = _ID;
		

	}*/



	public IEnumerator Reload(){
		if (reloading == false) {
			reloading = true;
			if (magAmmo < magSize) {
				if (totalAmmo > 0) {
					
					//Invoke a delegate when the gun reloads
					if(OnReloadEvent != null){
						OnReloadEvent();
					}
					yield return new WaitForSeconds (reloadTime);

					if (totalAmmo > (magSize - magAmmo)) {
						totalAmmo -= (magSize - magAmmo);
						magAmmo = magSize;
					} else {
						magAmmo += totalAmmo;
						totalAmmo = 0;
					}
				}
			}
			reloading = false;

		}

		transform.root.SendMessage ("UpdateUI");

	}
	public void RestockAmmo(){
		magAmmo = magSize;
		totalAmmo = maxAmmo;
	}
	void OnEnable(){
		shootSound = this.GetComponent<AudioSource>();

		if (reloading) {
			reloading = false;
			StartCoroutine (Reload());
		}
		if (cockSound != null&&transform.parent!=null)
		{
			shootSound.PlayOneShot(cockSound, 0.3f);
		}
		
		transform.root.SendMessage ("UpdateUI");

	}

	void UpdateUI(){
		return;
	}

	void CreateObjectPool(){
		for (int i = 0; i < magSize; i++) {
			GameObject _bullet = GameObject.Instantiate (bulletPrefab, new Vector3 (0f, 1000f, 0f), Quaternion.identity);
			_bullet.SetActive (false);
			_bullet.hideFlags = HideFlags.HideInInspector;
			_bullet.hideFlags = HideFlags.HideInHierarchy;

			poolList.Push(_bullet);

			//yield return wait;
		}
	}
	GameObject GetBullet(){
		if (poolList.Count == 0) {
			if (destroyedStack.Count > 0)
			{
				ReturnBullets();
			}
			else
			{
				return null;
			}

		}
		GameObject bullet = poolList.Pop ();
		destroyedStack.Push (bullet);
		bullet.SetActive (true);
		return(bullet);

	}


	void ReturnBullets(){
		
		foreach (GameObject _bullet in destroyedStack) {
			poolList.Push (_bullet);
		}
	}

}
