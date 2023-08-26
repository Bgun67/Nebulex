using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ActivaterInfoPanel : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI textPanel;
    [SerializeField] GameObject panelGlow;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void SetHighlight(bool highlighted){
        panelGlow.SetActive(highlighted);
    }

    public void UpdateData(float nextAvailableTime, string text, float currentDistance){
		float remainingTime = nextAvailableTime-Time.time;
		string remainingTimeText = remainingTime>0? "<color=red>"+remainingTime+"</color>": "<color=green>Ready</color>";
		textPanel.text = this.name+"\n"+text.Replace("useKey", GetUseKey())+"\n"+currentDistance+"\n"+remainingTimeText;
	}

    string GetUseKey()
	{
		string useKey = "";
			if (MInput.useMouse)
			{
				useKey = "R";
			}
			else
			{
				useKey = "□";
			}
		return useKey;
	}
}
