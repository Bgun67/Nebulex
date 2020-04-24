using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MetworkView))]
public class Animation_Sync_Module : MonoBehaviour
{
    public enum SyncType
    {
        Constant,
        Consistent,
        ValueChanged
    }
    public enum ValueType
    {
        Boolean,
        Integer,
        Float
    }
    [System.Serializable]
    public class SyncedProperty
    {
        public string propertyName;
        public SyncType syncType;
        [Tooltip("Applies Only when synctype = Consistent")]
        public float syncRate;
        public ValueType valueType;
        [Tooltip("Applies only when synctype =  valuechanged")]
        public float lastValue;
        public float lastSyncFrame;
    }
    MetworkView netView;
    [Tooltip("Set if required")]
    public Animator anim;
    public SyncedProperty[] propertiesToSync;

    private void Start()
    {
        netView = GetComponent<MetworkView>();
        if (anim == null)
        {
            anim = GetComponent<Animator>();
        }
    }
    // Update is called once per frame
    void Update()
    {
        for(int i = 0; i<propertiesToSync.Length; i++)
        {
            SyncedProperty property = propertiesToSync[i];
            float _value = 0f;
            switch (property.valueType)
            {
                case ValueType.Boolean:
                    _value = anim.GetBool(property.propertyName)?1:0;
                    break;
                case ValueType.Integer:
                    _value = (float)anim.GetInteger(property.propertyName);
                    break;
                case ValueType.Float:
                    _value = anim.GetFloat(property.propertyName);
                    break;
            }
            switch (property.syncType)
            {
                case SyncType.Constant:
                    netView.RPC("SetProperty", MRPCMode.Others, new object[] { property.propertyName, _value, (int)property.valueType });
                    break;
                case SyncType.Consistent:
                    if (Time.frameCount > property.lastSyncFrame + property.syncRate)
                    {
                        netView.RPC("SetProperty", MRPCMode.Others, new object[] { property.propertyName,_value, (int)property.valueType });
                        property.lastSyncFrame = Time.frameCount;
                    }
                    break;
                case SyncType.ValueChanged:
                    if (_value != property.lastValue)
                    {
                        print("Syncing" + this.gameObject.name+"Value"+_value+"Last Value"+property.lastValue);

                        property.lastValue = _value;
                        netView.RPC("SetProperty", MRPCMode.OthersBuffered, new object[] { property.propertyName, _value, (int)property.valueType });
                        
                    }
                    break;
                default:
                    break;
            }
        }
    }
    [MRPC]
    public void SetProperty(string _name, float _value, int _valueType)
    {
        print("Changing" + this.gameObject.name);
        ValueType type = (ValueType)_valueType;
        if (type == ValueType.Boolean)
        {
            anim.SetBool(_name,_value==1);
        }
        else if (type == ValueType.Integer)
        {
            anim.SetInteger(_name, (int)_value);
        }
        else if (type == ValueType.Float)
        {
            anim.SetFloat(_name, (float)_value);
        }

    }
}
