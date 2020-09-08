using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class UDBox : MonoBehaviour
{
    [System.Serializable]
    public class StringEvent : UnityEvent <string>{

    }
    public string[] options;
    public int optionIndex = 0; 
    public Text textBox;

    [SerializeField]
    public StringEvent method;

    // Start is called before the first frame update
    void Start()
    {
        textBox.text = options[optionIndex];
    }

    public void Increase(){
        optionIndex++;
        if(optionIndex >= options.Length){
            optionIndex = 0;
        }
        
        textBox.text = options[optionIndex];
        InvokeMethod();
    }
    public void Decrease(){
        optionIndex--;
        if(optionIndex < 0){
            optionIndex = options.Length-1;
        }
        
        textBox.text = options[optionIndex];
        InvokeMethod();
    }

    void InvokeMethod(){
        // this is to turn off the value and call set in the editor
        method.SetPersistentListenerState(0, UnityEventCallState.Off);
        method.RemoveAllListeners();

        // get the method assigned in the editor and call it
        System.Reflection.MethodInfo methodInfo = UnityEventBase.GetValidMethodInfo(method.GetPersistentTarget(0), method.GetPersistentMethodName(0), new System.Type[] { typeof(string) } );
        methodInfo.Invoke(method.GetPersistentTarget(0), new object[]{options[optionIndex]});
    }


}
