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

    public void UpdateData(float remainingTime, string text, float currentDistance){
		string remainingTimeText = remainingTime>0? "<color=red>"+remainingTime.ToString("0.00")+"s</color>": "<color=green>Ready</color>";
		textPanel.text = this.name+"\n"+text.Replace("useKey", GetUseKey())+"\n"+currentDistance.ToString("0.0")+"m\n"+remainingTimeText;
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
