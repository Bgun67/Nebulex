
using UnityEngine;
using System.Collections;

public static class Extension_Methods {
	///_should override doesn't actually do anything
	public static string ToString(this Quaternion _obj, bool _shouldOverride){
		
		return  "(" + _obj.x.ToString("F6") + "," + _obj.y.ToString("F6") + "," + _obj.z.ToString("F6") + "," + _obj.w.ToString("F6") + ")";
	}
	public static string ToString(this object _obj, bool _isQuaternion){
		if(_isQuaternion){
			Quaternion _quat = (Quaternion)_obj;
			return  "(" + _quat.x.ToString("F6") + "," + _quat.y.ToString("F6") + "," + _quat.z.ToString("F6") + "," + _quat.w.ToString("F6") + ")";
		}
		return _obj.ToString();
	}
	
}

