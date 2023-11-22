using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Fire : MonoBehaviour
{
    [Header("Gun Info")]
    public WeaponProperties weaponProperties;
    float fireDelay;
    public Transform shotSpawn;
    //This is rarely used and many need to be removed
    public Transform rhTarget;
    public Transform lhTarget;
    public Transform lhHint;
    [HideInInspector] public int totalAmmo;
    [HideInInspector] public int magAmmo;
    [HideInInspector] public float damageFactor =1;
    [Header("Sound")] [HideInInspector]
    public AudioSource source;

    [HideInInspector] public Scope scope;

    [Space()]
    public WaitForSeconds reloadWait;
    public Animator weaponAnim;

    public Transform scopePosition;
    int fired = 0;
    Rigidbody rootRB;
    public ParticleSystem muzzleFlash;
    public ParticleSystem bulletCasings;
    [HideInInspector] public int playerID;
   [HideInInspector]   public bool reloading;
    public delegate void ReloadEvent();
    public ReloadEvent OnReloadEvent;
    public GameObject magGO;

    public Stack<GameObject> poolList = new Stack<GameObject>();
    public Stack<GameObject> destroyedStack = new Stack<GameObject>();


    // Use this for initialization
    void Awake()
    {
        Setup();
    }
    void Setup()
    {
        reloadWait = new WaitForSeconds(weaponProperties.ReloadTime);
        source = this.GetComponentInChildren<AudioSource>();

        weaponAnim = this.GetComponent<Animator>();

        if (!weaponProperties.IgnoreParentVelocity)
        {
            rootRB = transform.root.GetComponent<Rigidbody>();
        }


        magAmmo = weaponProperties.MagSize;
        totalAmmo = weaponProperties.MaxAmmo;

    }

    public void Activate(GameObject player)
    {
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
    void Update()
    {
        if (!Input.GetButton("Fire1"))
        {
            fired = 0;
        }


    }
    //I've ignored sending the inherited velocity, I think the server will be up to date enought
    //on it so the difference will be small.
    public bool FireWeapon(Vector3 shotSpawnPosition, Vector3 shotSpawnForward)
    {

        if (!reloading)
        {

            if (weaponProperties.FireType == FireTypes.SemiAuto)
            {

                if (fired > 0)
                {
                    return false;
                }
            }
            else if (weaponProperties.FireType == FireTypes.Burst)
            {

                if (fired > 2)
                {
                    return false;
                }
            }
            if (Time.time >= fireDelay)
            {

                if (magAmmo > 0)
                {
                    fireDelay = Time.time + 1/(weaponProperties.FireRate/60);

                    if (Physics.Raycast(shotSpawnPosition, shotSpawnForward, out RaycastHit hit, 200f))
                    {
                        Damage damage = hit.transform.GetComponentInParent<Damage>();
                        if (damage)
                        {
                             damage.TakeDamage(weaponProperties.DamagePower*damageFactor, playerID, -shotSpawnForward);
                        }
                    }

                    fired++;
                    if (weaponAnim != null)
                    {
                        try
                        {
                            weaponAnim.SetTrigger("Fire");
                        }
                        catch
                        {

                        }
                    }



                    if (source) source.PlayOneShot(weaponProperties.ShootSound, 1f);

                    if (muzzleFlash) muzzleFlash.Play();

                    if (bulletCasings) bulletCasings.Play();

                    magAmmo--;
                    if (magAmmo < 1)
                    {
                        //	StartCoroutine (Reload ());

                    }
                    return true;


                }
                if (magAmmo <= 0)
                {
                    if (weaponProperties.TriggerClick != null)
                    {
                        source.PlayOneShot(weaponProperties.TriggerClick, 0.3f);
                    }
                    StartCoroutine(Reload());

                }
            }
        }
        return false;

    }

    /*[Command]
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
	/*
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



    public IEnumerator Reload()
    {
        if (reloading == false)
        {
            reloading = true;
            if (magAmmo < weaponProperties.MagSize)
            {
                if (totalAmmo > 0)
                {

                    //Invoke a delegate when the gun reloads
                    if (OnReloadEvent != null)
                    {
                        OnReloadEvent();
                    }
                    yield return new WaitForSeconds(weaponProperties.ReloadTime);

                    if (totalAmmo > (weaponProperties.MagSize - magAmmo))
                    {
                        totalAmmo -= (weaponProperties.MagSize - magAmmo);
                        magAmmo = weaponProperties.MagSize;
                    }
                    else
                    {
                        magAmmo += totalAmmo;
                        totalAmmo = 0;
                    }
                }
            }
            reloading = false;

        }

        //transform.root.SendMessage("UpdateUI");

    }
    public void RestockAmmo()
    {
        magAmmo = weaponProperties.MagSize;
        totalAmmo = weaponProperties.MaxAmmo;
    }
    void OnEnable()
    {
        source = this.GetComponent<AudioSource>();

        if (reloading)
        {
            reloading = false;
            StartCoroutine(Reload());
        }
        if (weaponProperties.CockSound != null && transform.parent != null)
        {
            source.PlayOneShot(weaponProperties.CockSound, 0.3f);
        }

        //transform.root.SendMessage("UpdateUI");

    }

}
