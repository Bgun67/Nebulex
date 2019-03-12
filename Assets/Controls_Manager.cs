using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Controls_Manager : MonoBehaviour {

	public GameObject contentPanel;
	public GameObject controlsButtonHandle;
	public List<GameObject> allHandles = new List<GameObject>();


	void OnEnable()
	{
		ShowMappings();
	}
	public void ShowMappings()
	{
		foreach (GameObject obj in allHandles)
		{
			Destroy(obj);
		}
		string[,] mappings = MInput.buttonMappings;
		for (int i = 0; i < mappings.GetLength(0); i++)
		{
			if (mappings[i, 0] != "")
			{
				GameObject buttonObj = Instantiate(controlsButtonHandle, contentPanel.transform);
				RectTransform rt = buttonObj.GetComponent<RectTransform>();
				rt.anchoredPosition = new Vector2(0f, -rt.rect.height * i);
				//find labels and button text
				Text[] texts = buttonObj.GetComponentsInChildren<Text>();
				//set the text of the label to the script name
				texts[0].text = mappings[i, 0];
				//set the label of the button to the controller name
				texts[1].text = mappings[i, 1];
				string _scriptButtonName = mappings[i, 0];
				Button button = buttonObj.GetComponentInChildren<Button>();
				button.onClick.AddListener(() => { SwapControl(_scriptButtonName); });
				allHandles.Add(buttonObj);

			}
		}
	}
	// Update is called once per frame
	public void SwapControl (string _scriptButtonName) {
		StartCoroutine(MInput.SwitchMappedButton(_scriptButtonName));
	}
}
