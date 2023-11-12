using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AnimReceiver : MonoBehaviour
{
    Dictionary<string, UnityAction> subscribers = new Dictionary<string, UnityAction>();
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    public void Subscribe(string name, UnityAction action)
    {
        if(name.ToUpper()!=name){
            Debug.LogWarning("Name of event " + name + "uses the wrong format, please use all capitals and underscores");
        }

        if (subscribers.ContainsKey(name))
        {
            subscribers[name] = action;
        }
        else
        {
            subscribers.Add(name, action);
        }
    }

    public void PostEvent(string name)
    {
        if(name.ToUpper()!=name){
            Debug.LogWarning("Name of event " + name + "uses the wrong format, please use all capitals and underscores");
        }
        if (subscribers.ContainsKey(name))
        {
            subscribers[name].Invoke();
        }
    }
}
