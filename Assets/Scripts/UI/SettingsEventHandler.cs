using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public class SettingsEventHandler : MonoBehaviour
{
    [SerializeField]
    private UIDocument m_UIDocument;

    //[SerializeField]
    //private UnityEvent m_OnResumeClicked;
    [SerializeField]
    private Pause_Menu m_PauseMenu;
    [SerializeField]
    private Game_Settings m_GameSettings;

    // Start is called before the first frame update
    public void Init()
    {
        VisualElement root = m_UIDocument.rootVisualElement;
        var btn_Resume = root.Q<Button>("btn_Resume");
        btn_Resume.clickable.clicked += m_PauseMenu.Resume;

        var btn_Options = root.Q<Button>("btn_Options");
        btn_Options.clickable.clicked += m_PauseMenu.Options;
        var btn_Loadout = root.Q<Button>("btn_Loadout");
        btn_Loadout.clickable.clicked += m_PauseMenu.GoToLoadoutScene;
        var btn_Recall = root.Q<Button>("btn_Recall");
        btn_Recall.clickable.clicked += m_PauseMenu.Recall;
        var btn_Desert = root.Q<Button>("btn_Desert");
        btn_Desert.clickable.clicked += m_PauseMenu.Quit;

        //Settings menu buttons
        var btn_Graphics = root.Q<Button>("btn_Graphics");
        btn_Graphics.clickable.clicked += () => {ToggleMenuVisibility(root, "menu_Graphics");};

        var btn_Gameplay = root.Q<Button>("btn_Gameplay");
        btn_Gameplay.clickable.clicked += () => {ToggleMenuVisibility(root, "menu_Gameplay");};

        var btn_Audio = root.Q<Button>("btn_Audio");
        btn_Audio.clickable.clicked += () => {ToggleMenuVisibility(root, "menu_Audio");};

        ToggleMenuVisibility(root, "");

        var sldr_MouseSensitivity = root.Q<Slider>("sldr_MouseSensitivity");
        sldr_MouseSensitivity.RegisterValueChangedCallback(v => {m_GameSettings.ChangeLookSensitivity(v.newValue);});

    }

    void ToggleMenuVisibility(VisualElement root, string element){
        foreach(string menuString in new[] {"menu_Graphics", "menu_Gameplay","menu_Audio"}){
            var menu_Graphics = root.Q<VisualElement>(menuString);
            if(menuString == element)
                menu_Graphics.style.display = DisplayStyle.Flex;
            else
                menu_Graphics.style.display = DisplayStyle.None;
        }
        
        
    }

}
