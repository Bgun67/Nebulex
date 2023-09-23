using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class Pause_Menu : MonoBehaviour {
	public GameObject confirmQuitPanel;
	public GameObject confirmRecallPanel;
	//public GameObject settingsPanel;
	public GameObject eventSystem;

	[SerializeField]
    private UIDocument m_UIDocument;
	private VisualElement m_Root;

	// Use this for initialization
	void OnEnable(){
		BuildPage();
	}
	public void BuildPage()
    {
        m_Root = m_UIDocument.rootVisualElement;
        var btn_Resume = m_Root.Q<Button>("btn_Resume");
        btn_Resume.clickable.clicked += Resume;

        var btn_Options = m_Root.Q<Button>("btn_Options");
        btn_Options.clickable.clicked += Options;
        var btn_Loadout = m_Root.Q<Button>("btn_Loadout");
        btn_Loadout.clickable.clicked += GoToLoadoutScene;
        var btn_Recall = m_Root.Q<Button>("btn_Recall");
        btn_Recall.clickable.clicked += Recall;
        var btn_Desert = m_Root.Q<Button>("btn_Desert");
        btn_Desert.clickable.clicked += Quit;
    }
	
	public void Pause () {
		this.gameObject.SetActive (true);

		MInput.inputLock = MInput.InputLock.LockAll;
        UnityEngine.Cursor.visible = true;
	}
	public void Recall(){
		confirmRecallPanel.SetActive (true);
	}
	public void Options()
	{
		var menu_Settings = m_Root.Q<Box>("menu_Settings");
		menu_Settings.style.display = DisplayStyle.Flex;
	}
	public void Quit(){
		confirmQuitPanel.SetActive (true);
	}
	public void Deny(){
		confirmQuitPanel.SetActive (false);
		confirmRecallPanel.SetActive (false);

	}
	public void KillPlayer(){
		Player_Controller player = FindObjectOfType<Game_Controller>().localPlayer;

		MInput.inputLock = MInput.InputLock.None;
		if (player)
		{
			player.GetComponent<Damage>().TakeDamage(200, 0, player.transform.position + player.transform.forward, true);
		}
		confirmRecallPanel.SetActive(false);
		Resume();
	}
	public void GoToMainMenu(){
		//try{
			//Destroy(CustomNetworkManager.Instance.gameObject);
			//Cleanup Metwork
			if(CustomNetworkManager.Instance.isServerMachine){
            	CustomNetworkManager.Instance.StopServer();
        	}
       		CustomNetworkManager.Instance.StopClient();
			   //This part will automatically send me out to the match scene
			   //when the server is shut down
			
		//}
		//catch{
		//	Debug.LogWarning("Failed to find one or more network components when quitting");
		//}
		MInput.inputLock = MInput.InputLock.None;
		SceneManager.LoadScene ("Start Scene");
	}
	public void Resume(){
		var menu_Settings = m_Root.Q<Box>("menu_Settings");
		menu_Settings.style.display = DisplayStyle.None;

		this.gameObject.SetActive(false);
		MInput.inputLock = MInput.InputLock.None;
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
		UI_Manager.Instance.isPaused = false;

		//eventSystem.SetActive (false);
	}
	public void GoToLoadoutScene()
	{
		MInput.inputLock = MInput.InputLock.None;
		SceneManager.LoadScene("Loadout Scene", LoadSceneMode.Additive);
	}

}
