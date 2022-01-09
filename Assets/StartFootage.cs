using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Media;
using UnityEngine.SceneManagement;
using UnityEngine.PostProcessing;

public class StartFootage : MonoBehaviour
{
	public Rigidbody moduleRb;
	public Text messageBox;
	string[] displayedText = new string [50];
	int currentLine;
	public string[] lifeSupportCommands;
	public GameObject explosion;
	public Camera mainCam;
	public Transform finalPosition;
	public GameObject ragdoll;
	public AudioSource music;
	public Light camLight;
	public GameObject plus;
	public AudioClip[] sfxClips;
	public WindowsVoice voice;
	
	static bool IsFirstTime()
	{
		
		//is first time
		if (Game_Settings.currGameplaySettings.firstTime)
		{
			Game_Settings.currGameplaySettings.firstTime = false;
			Game_Settings.SaveGameSettings();
			return true;
		}
		else
		{
			return false;
		}
	}
	// Start is called before the first frame update

	void Start()
    {
		Physics.autoSimulation = true;
		
		if (!IsFirstTime())
		{
			SceneManager.LoadScene("Start Scene");
		}
		else
		{
			StartCoroutine(RunSceneSequence());
		}
	}
	void Update()
	{
		if (Input.anyKeyDown)
		{
			SceneManager.LoadScene("Start Scene");
		}
	}


	// Update is called once per frame
	IEnumerator RunSceneSequence()
    {
		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(ModuleConnect());
		currentLine++;
		SetText("Connected");
		currentLine++;
		System.Media.SystemSounds.Question.Play();

		yield return DisplaySimpleCommands(lifeSupportCommands);

		yield return StartCoroutine(DepressurizeAirLock());
		currentLine++;

		SetText("Success");
		currentLine++;
		yield return new WaitForSeconds(0.3f);

		SetText("All Systems Healthy, Disconnecting Umbilical");
		currentLine++;

		yield return StartCoroutine(DisconnectUmbilical());
		currentLine++;
		
		SetText("Done");
		currentLine++;

		SetText("Disconnecting Module");
		currentLine++;
		currentLine++;

		SetText("May Apollo Be Your Guide");
		currentLine++;


		yield return new WaitForSeconds(1f);
		music.PlayOneShot(sfxClips[0]);
		DisconnectModule();

		yield return new WaitForSeconds(3f);
		DestroyModule();

		yield return new WaitForSeconds(2.5f);
		music.PlayOneShot(sfxClips[1]);
		music.PlayOneShot(sfxClips[2]);
		yield return StartCoroutine(ShakeCamera());



		yield return new WaitForSeconds(3f);
		plus.SetActive(false);
		yield return StartCoroutine(MoveCamera(finalPosition.position, finalPosition.rotation));


	}
	IEnumerator DisplaySimpleCommands(string[] commands)
	{
		foreach (string line in commands)
		{
			SetText(line);
			currentLine++;

			yield return new WaitForSeconds(0.02f);

		}
	}
	IEnumerator ModuleConnect()
	{
		StartCoroutine(FocusCamera());
		int i = 0;
		while (i < 12)
		{
			if (i % 3 == 0)
			{
				SetText("Connecting to Module .");
			}
			else
			{
				SetText(displayedText[currentLine] + " .");
			}
			yield return new WaitForSeconds(0.5f);
			i++;
		}
	}
	IEnumerator FocusCamera()
	{
		float i = 0;
		DepthOfFieldModel.Settings _settings = mainCam.GetComponent<PostProcessingBehaviour>().profile.depthOfField.settings;
		while (i <=2f)
		{

			_settings.focusDistance = i;
			mainCam.GetComponent<PostProcessingBehaviour>().profile.depthOfField.settings = _settings;
			yield return new WaitForSeconds(0.02f);
			i+=0.06f;

		}
		while (i >=0.2f)
		{

			_settings.focusDistance = i;
			mainCam.GetComponent<PostProcessingBehaviour>().profile.depthOfField.settings = _settings;
			yield return new WaitForSeconds(0.02f);
			i-=0.06f;

		}
		while (i <10f)
		{

			_settings.focusDistance = i;
			mainCam.GetComponent<PostProcessingBehaviour>().profile.depthOfField.settings = _settings;
			yield return new WaitForSeconds(0.02f);
			i+=0.06f;

		}
	}
	IEnumerator DepressurizeAirLock()
	{
		int i = 0;
		while (i <= 100)
		{
			
			SetText("Depressurizing Airlock"+i+"%");
			System.Media.SystemSounds.Question.Play();

			if (i == 99)
			{
				yield return new WaitForSeconds(1f);
			}
			yield return new WaitForSeconds(0.02f);
			i++;

		}
	}
	IEnumerator DisconnectUmbilical()
	{
		int i = 0;
		while (i <= 8)
		{

			SetText(displayedText[currentLine]+"|");

			yield return new WaitForSeconds(0.7f);
			i++;
		}
	}
	IEnumerator ShakeCamera()
	{
		int i = 0;
		while (i <= 10)
		{
			if (i % 2 == 0)
			{
				mainCam.transform.position += Vector3.up * 1f/(i+1);
			}
			else
			{
				mainCam.transform.position -= Vector3.up * 1f/(i+1);
			}
			yield return new WaitForEndOfFrame();
			i++;
		}
	}
	IEnumerator MoveCamera(Vector3 newPosition, Quaternion newRotation)
	{
		StartCoroutine(ShineLight());
		float i = 0;
		GrainModel.Settings _settings1 = mainCam.GetComponent<PostProcessingBehaviour>().profile.grain.settings;

			BloomModel.Settings _settings2 = mainCam.GetComponent<PostProcessingBehaviour>().profile.bloom.settings;

		while (Vector3.Distance(mainCam.transform.position,newPosition)>1f)
		{
			_settings1.intensity = Mathf.Clamp01(_settings1.intensity-i);
			mainCam.GetComponent<PostProcessingBehaviour>().profile.grain.settings = _settings1;

			_settings2.bloom.intensity += i;
			mainCam.GetComponent<PostProcessingBehaviour>().profile.bloom.settings = _settings2;


			mainCam.transform.position = Vector3.Lerp(mainCam.transform.position, newPosition, 0.1f*i);
			mainCam.transform.rotation = Quaternion.Lerp(mainCam.transform.rotation, newRotation, 0.1f*i);
			i += 0.01f;
			music.volume = 1f;//Mathf.Clamp01(i*10f+0.2f);
			yield return new WaitForEndOfFrame();
			
		}
	}
	IEnumerator ShineLight()
	{
		float i = 0.1f;
		while (i<100f)
		{

			camLight.intensity = i;
			i = i+i*0.05f;
			yield return new WaitForEndOfFrame();
			
		}
		while (i>0f)
		{

			music.volume = i/100f;
			i = i-3f;
			yield return new WaitForEndOfFrame();
			
		}

		SceneManager.LoadSceneAsync("Start Scene");

		
	}
	public void DisconnectModule()
	{
		moduleRb.AddRelativeForce(new Vector3(0f, -100f, 0f)*moduleRb.mass);
		moduleRb.AddRelativeTorque(new Vector3(-1f, -1.2f, 0f)*moduleRb.mass);
	}
	public void DestroyModule()
	{
		moduleRb.AddForce(new Vector3(0f, -100f, 0f)*moduleRb.mass);
		moduleRb.AddRelativeTorque(new Vector3(20f, 60f, 6f)*moduleRb.mass);
		moduleRb.gameObject.GetComponent<ConstantForce>().enabled = true;
		explosion.SetActive(true);
	}
	void SetText(string text)
	{
		displayedText[currentLine] = text;
		DisplayText();

	}
	void DisplayText()
	{
		messageBox.text = "";
		foreach (string line in displayedText)
		{
			messageBox.text += line + "\r\n";
		}
	}
}
