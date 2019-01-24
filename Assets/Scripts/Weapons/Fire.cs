using System.Collections;
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
	public float fireDelay;
	public Transform shotSpawn;
	public float reloadTime;
	[Tooltip("Rounds Per Minute")]
	public float fireRate;
	public int magSize;
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
	public AudioSource shootSound;
	[HideInInspector]
	public AudioClip sound;
	public WaitForSeconds reloadWait;
	public Animator weaponAnim;
	public bool isRecoil = false;
	public UnityEvent recoil;
	[Tooltip("0-1 bitches!")]
	public float recoilAmount = 0.5f;
	[Tooltip("0-1 How much the weapon drifts")]
	[Range(0,1)]
	public float bulk = 0.5f;

	[Tooltip("1 = smg style, 2 = pistol")]
	public int recoilNumber = 1;
	public int aimAnimNumber = 1;
	public Transform scopePosition;
	public int fired = 0;
	[Tooltip("DO fired bullets start with the parents velocity?")]
	public bool ignoreParentVelocity;
	public Rigidbody rootRB;
	public MetworkView netView;
	public ParticleSystem muzzleFlash;
	public int playerID;
	public bool reloading;
	public GameObject magGO;
	[SerializeField]
	public Stack<GameObject> poolList = new Stack<GameObject> ();
	public Stack<GameObject> destroyedStack = new Stack<GameObject> ();
	WaitForEndOfFrame wait = new WaitForEndOfFrame();


	// Use this for initialization
	void Start () {
		reloadWait = new WaitForSeconds (reloadTime);
		shootSound = this.GetComponent<AudioSource> ();
		sound = shootSound.clip;
		weaponAnim = this.GetComponent<Animator> ();
		fireRate = 1 / (fireRate / 60f);

		if (!ignoreParentVelocity) {
			rootRB = transform.root.GetComponent<Rigidbody> ();
		}

		if (netView == null) {
			netView = this.GetComponent<MetworkView> ();
		}
		if (netView == null) {
			netView = this.GetComponentInParent<MetworkView> ();
		}

		Invoke("CreateObjectPool", Random.Range(0,0.1f));
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
	public void FireWeapon(){
		if (!reloading) {
			if (fireType == FireTypes.SemiAuto) {
			
				if (fired > 0) {
					return;
				} 
			} else if (fireType == FireTypes.Burst) {

				if (fired > 2) {
					return;
				} 
			}
			if (Time.time >= fireDelay) {
				if (magAmmo > 0) {
					fireDelay = Time.time + fireRate;
					GameObject bullet = GetBullet ();
					bullet.transform.position = shotSpawn.position;
					bullet.transform.rotation = shotSpawn.rotation;
					fired++;
					if (weaponAnim != null) {
						try {
							weaponAnim.SetTrigger ("Fire");
						} catch {

						}
					}
					if (ignoreParentVelocity) {
						bullet.GetComponent<Rigidbody> ().velocity = shotSpawn.transform.forward * bulletVelocity;
					} else {
						bullet.GetComponent<Rigidbody> ().velocity = rootRB.velocity + shotSpawn.transform.forward * bulletVelocity;
					}
					bullet.GetComponent<Bullet_Controller> ().damagePower = damagePower;

					if (Metwork.peerType != MetworkPeerType.Disconnected) {
						netView.RPC ("RPC_FireWeapon", MRPCMode.Others, new object[] {
							bullet.transform.position,
							bullet.transform.rotation,
							bullet.GetComponent<Rigidbody> ().velocity,
							bullet.GetComponent<Bullet_Controller> ().damagePower,
							playerID
						});
					}
					if (shootSound != null) {
						shootSound.PlayOneShot (sound, 1f);
					}
					if (muzzleFlash != null) {
						muzzleFlash.Play ();
					}
					bullet.GetComponent<Bullet_Controller> ().fromID = playerID;
					if (isRecoil) {
						if (recoil.GetPersistentEventCount () > 0) {
							recoil.Invoke ();
						} else {
							transform.root.SendMessage ("Recoil");
						}
					}

					magAmmo--;
					if (magAmmo < 1) {
						StartCoroutine (Reload ());

					}

				} else {
					StartCoroutine (Reload ());

				}
			}
		}
	}

	[MRPC]
	void RPC_FireWeapon(Vector3 _position, Quaternion _rotation, Vector3 _velocity, int _damagePower, int _ID){
		if (weaponAnim != null) {
			
			//weaponAnim.SetTrigger ("Fire");
		}
		if (shootSound != null) {
			shootSound.PlayOneShot (sound, 1f);
			//shootSound.Play ();

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

	}



	public IEnumerator Reload(){
		if (reloading == false) {
			reloading = true;
			if (magAmmo < magSize) {
				if (totalAmmo > 0) {
					if (Metwork.peerType != MetworkPeerType.Disconnected) {
						transform.root.GetComponent<MetworkView>().RPC ("RPC_Reload", MRPCMode.All, new object[] { });
					} else {
						transform.root.SendMessage ("RPC_Reload");
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
	void OnEnable(){
		if (reloading) {
			reloading = false;
			StartCoroutine (Reload());
		}
		transform.root.SendMessage ("UpdateUI");

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
