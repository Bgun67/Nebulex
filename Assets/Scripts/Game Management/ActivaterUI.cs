using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;


public class ActivaterUI : MonoBehaviour
{
    List<Activater> activaters;
    [SerializeField] GameObject infoPanelPrefab;
    Dictionary<Activater, ActivaterInfoPanel> infoPanels;

    // Start is called before the first frame update
    public void Open(Activater[] _activaters)
    {
        infoPanels = new Dictionary<Activater, ActivaterInfoPanel>();
        activaters = new List<Activater>(_activaters);
        foreach(Activater activater in activaters){
            ActivaterInfoPanel infoPanel = Instantiate(infoPanelPrefab, transform).GetComponent<ActivaterInfoPanel>();
            infoPanel.SetHighlight(false);
            infoPanels.Add(activater, infoPanel);
        }
    }

    public void SetHighlight(Activater activater, bool highlighted){
        if(!activater||!infoPanels.ContainsKey(activater)){
            return;
        }
        infoPanels[activater].SetHighlight(highlighted);
    }

    public void Select(Activater activater){

    }

    // Update is called once per frame
    public void UpdateInfo(Activater activater)
    {
        Vector3 screenSpacePosition = Camera.main.WorldToScreenPoint(activater.Position);
        if(!new Rect(0,0,Screen.width,Screen.height).Contains(screenSpacePosition)){
            infoPanels[activater].gameObject.SetActive(false);

        }
        infoPanels[activater].transform.position = screenSpacePosition;
        infoPanels[activater].UpdateData(activater.nextAvailableTime-Time.time, activater.text, 10f);
        
    }

    public void Close(){
        foreach(ActivaterInfoPanel panel in infoPanels.Values){
            Destroy(panel.gameObject);
        }
        infoPanels.Clear();
        activaters.Clear();
    }
}
