using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class StartGameUI : MonoBehaviour
{
    public UIDocument m_UIDocument;

    private Label m_LblTimeRemaining;
    private Button m_BtnDeploy;

    void Awake()
    {
       
    }

    void OnEnable(){
        var m_Root = m_UIDocument.rootVisualElement;
        m_LblTimeRemaining = m_UIDocument.rootVisualElement.Q<Label>("lbl_TimeRemaining");

        m_BtnDeploy = m_UIDocument.rootVisualElement.Q<Button>("btn_Deploy");
        m_BtnDeploy.clickable.clicked += HideGui;
        m_BtnDeploy.SetEnabled(false);
    }
    
    public void UpdateTime(double time){
        print("Update Time"+time);
        TimeSpan timeSpan = TimeSpan.FromSeconds(time);
        m_LblTimeRemaining.text = timeSpan.ToString("mm':'ss");

    }
    public void MatchReady(){
        print("Match Ready");
    Debug.Log(m_BtnDeploy);
        m_BtnDeploy.SetEnabled(true);
    }

    void HideGui(){
        this.gameObject.SetActive(false);
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
    }
}
