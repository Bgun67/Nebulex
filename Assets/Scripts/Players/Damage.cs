using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Mirror;



public class Damage : NetworkBehaviour{
	public float originalHealth;
	[SyncVar (hook = "UpdateUIHook")]
	[HideInInspector] public float currentHealth;
	public UnityEvent dieFunction;
	public bool isVehicle = true;
	//public Text UI_healthText;
	//public RectTransform UI_healthBar;
	//public RectTransform UI_healthBox;

	public Transform initialPosition;
	[Tooltip ("Make Longer than the carcass destroy time")]
	public float resetTime;
	int[] assistArray;
	public Game_Controller gameController;

	public bool regen;
	 float regenWait;
	[Tooltip("Time after taking damage when the health starts to regenerate")]
	public float regenTime;
	[Tooltip("Regenerated Health per second")]
	public float regenRate = 10f;
	public bool healthShown;
	public bool indicateLowHealth = false;
	public GameObject lowHealthEffect;
	[SyncVar]
	public bool isDead;
	[Tooltip("Use this to send the damage to a different damage script specified below")]
	public bool forwarder;
	[Tooltip("Leave empty if above not checked")]
	public Damage forwardedDamage;
	[Tooltip("Ignore if forwarder not checked")]
	public float forwardedScale = 1f;


	public GameObject impactPrefab;

	[Tooltip("Set this to -1 to disable impact damage")]
	public float impactDamageFactor = -1f;
	public float damageThreshold = 0f;

	//public Metwork_Object netObj;
	void Awake(){
		assistArray = new int[Game_Controller.Instance.maxPlayers];
	}

	// Use this for initialization
	void Start () {
		if (forwarder) {
			if (forwardedDamage == null) {
				forwardedDamage = transform.root.GetComponent<Damage> ();
			}
			return;
		}
		currentHealth = originalHealth;
		Reactivate ();
		gameController = Game_Controller.Instance;
		if(healthShown){
			UpdateUI();
		}

	}

	void Update(){
		RegenHealth();
	}

	public void Reset(){
		this.gameObject.SetActive (false);
		ShowLowHealthEffect(false);
		Invoke (nameof(Reactivate), resetTime);	

	}
	public void Reactivate(){
		if (initialPosition != null) {
			transform.position = initialPosition.position;
			transform.rotation = initialPosition.rotation;
		} else {
			initialPosition = this.transform;
		}
		currentHealth = originalHealth;
		this.gameObject.SetActive (true);
		isDead = false;


	}
		

	public void TakeDamage(int damageAmount, int fromID, Vector3 _hitDirection, bool overrideTeam = false)
	{
		
		if(isLocalPlayer && this.tag == "Player"){
			UI_Manager.Instance.UpdateHitDirection(_hitDirection, this.transform);
			GlobalSound.HurtSound();
		}

		//Damage should only be done on the server and then synced to the clients
		if(!isServer){
			return;
		}
		//TODO: Implement a second class for children that doesn't have a networkID
		/*if (forwarder)
		{
			forwardedDamage.TakeDamage((int)(damageAmount * forwardedScale), fromID, _hitDirection, overrideTeam);
			return;
		}*/
		//Dead men take no bullets
		if (isDead)
		{
			return;
		}

		//I'm removing the offset used for bots, from now on they will just used a regular
		//1-32ish playerNumber;
		//if(fromID > 64){
		//	fromID = Game_Controller.GetBotFromPlayerID(fromID).botID - 64;
		//}

		int thisID = -1;
		if(this.GetComponent<Player>()!= null){
			thisID = this.GetComponent<Player>().playerID;
		}

		if(this.tag == "Player"){
			//Ignore friendly fire
			if ((gameController.playerStats[fromID].team == gameController.playerStats[thisID].team) && !overrideTeam)
			{
				return;
			}
			
			
		}
		if(damageAmount >= damageThreshold){
			currentHealth -= damageAmount;
		}

		if (regen)
		{
			regenWait = Time.time + regenTime;
		}
		if (currentHealth < originalHealth * 0.3f)
		{
			ShowLowHealthEffect(true);
		}
		
		if (currentHealth <= 0f)
		{
			currentHealth = 0;
			
			if (this.tag == "Player")
			{	
				int thisTeam = gameController.playerStats[thisID].team;
				int fromTeam = gameController.playerStats[fromID].team;
				if (fromTeam != thisTeam)
				{
					gameController.AddKill(fromID);
				}
				gameController.AddDeath(thisID);
				

				Chat.LogToChat((fromTeam == 0 ? "<color=green>": "<color=red>") + gameController.playerStats[fromID].name + "</color> killed "+ (thisTeam == 0 ? "<color=green>": "<color=red>") + gameController.playerStats[thisID].name +"</color>\n");

			
				for (int i = 1; i < assistArray.Length; i++)
				{
					if (i == fromID)
					{
						continue;
					}
					int assistScore = assistArray[i];
					assistScore = (int)Mathf.Clamp((assistScore / originalHealth) * 100, 0, 100);
					if (assistScore >= 85)
					{
						gameController.AddKill(i);

					}
					else if (assistScore > 33)
					{
						gameController.AddAssist(i, assistScore);

					}

				}
				for (int i = 0; i < assistArray.Length; i++)
				{
					assistArray[i] = 0;
				}
			}
			if (dieFunction.GetPersistentEventCount() > 0)
			{
				isDead = true;
				dieFunction.Invoke();

			}
			else
			{
				Destroy(this.gameObject);
			}
			int j = 0;
			//TODO: Implement child gameobject damage
			/*foreach (Damage damageScript in this.GetComponentsInChildren<Damage>())
			{
				if (j > 9)
				{
					break;
				}
				damageScript.TakeDamage(damageScript.originalHealth + 1, fromID, _hitDirection);
				j++;
			}*/




		}
		else
		{
			if (fromID != 0)
			{
				if(this.tag == "Player"){
					GlobalSound.SendRemoteSound(GlobalSound.JukeBox.HitEnemy, fromID);
				}
				assistArray[fromID] += damageAmount;
			}
		}
		//if (healthShown)
		//{
		//	UpdateUI();
		//}

	}
	public void ShowLowHealthEffect(bool _show)
	{
		if (indicateLowHealth)
		{
			if (lowHealthEffect != null)
			{
				/*TODO
				if (netObj != null)
				{
					if (Metwork.peerType != MetworkPeerType.Disconnected)
					{
						netObj.netView.RPC("RPC_ShowLowHealthEffect", MRPCMode.All, new object[] { _show });
					}
					else
					{
						RPC_ShowLowHealthEffect(_show);
					}
				}*/

			}
		}
	}
	[MRPC]
	public void RPC_ShowLowHealthEffect(bool _show)
	{
		lowHealthEffect.SetActive(_show);
	}
	public void RegenHealth(){
		if (currentHealth < originalHealth) {
			if (Time.time >= regenWait) {
				currentHealth+=Time.deltaTime*regenRate;
				//if (healthShown)
				//{
				//	UpdateUI();
				//}
			}
		} else {
			for (int i = 0; i < assistArray.Length; i++) {
				assistArray [i] = 0;
			}
		}
		if (currentHealth >= originalHealth * 0.3f)
		{
			ShowLowHealthEffect(false);
		}
	}

	public void OnCollisionEnter(Collision collision){
		if (forwarder) {
			return;
		}
		if (impactPrefab != null && collision.transform.root.tag == "Bullet") {
			GameObject impact = (GameObject)Instantiate (impactPrefab, collision.contacts[0].point, Quaternion.identity);
			impact.transform.forward = -collision.contacts [0].normal;
			impact.transform.SetParent (this.transform);
			Destroy (impact, 20f);
			return;
		}

		if (!isServer||collision.transform.root.tag == "Bullet") {
			return;
		}

		if (impactDamageFactor > 0f) {
			
			
			float damage = Mathf.Abs(Vector3.Dot(collision.relativeVelocity, collision.contacts[0].normal));
			damage *= impactDamageFactor;
			if (damage < 10f)
			{
				return;
			}
			damage *= damage;
			damage *= originalHealth/1000f;

			if (damage > 0.01f*originalHealth) {
				TakeDamage ((int)damage, 0, collision.contacts[0].point);
			}

			
		}
	}
	public void UpdateUIHook(float oldValue, float newValue){
		UpdateUI();
	}
	public void UpdateUI(){
		if(!isLocalPlayer){
		return;}

		if (healthShown == true) {
			if (isVehicle)
			{
				UI_Manager.Instance.vehicleHealthBox.gameObject.SetActive(true);
				UI_Manager.Instance.vehicleHealthText.text = "+" + (int)currentHealth;
				UI_Manager.Instance.vehicleHealthBar.offsetMin = new Vector2(128f - (float)currentHealth / (float)originalHealth * 128f, -16f);
				UI_Manager.Instance.vehicleHealthBar.offsetMax = new Vector2(256f - (float)currentHealth / (float)originalHealth * 128f, 0f);
			}
			else
			{
				UI_Manager.Instance.healthBox.gameObject.SetActive(true);
				UI_Manager.Instance.healthText.text = "+" + (int)currentHealth;
				UI_Manager.Instance.healthBar.offsetMin = new Vector2(128f - (float)currentHealth / (float)originalHealth * 128f, -16f);
				UI_Manager.Instance.healthBar.offsetMax = new Vector2(256f - (float)currentHealth / (float)originalHealth * 128f, 0f);
			}
			//None of the below is ever called I think
		} else {
			//UI_healthText.text = "";
			if (isVehicle)
			{
				if (UI_Manager.Instance.vehicleHealthBox != null)
				{
					UI_Manager.Instance.vehicleHealthBox.gameObject.SetActive(false);
				}
			}
			else
			{
				if (UI_Manager.Instance.healthBox != null)
				{
					UI_Manager.Instance.healthBox.gameObject.SetActive(false);
				}
			}

		}
	}
}
