
using UnityEngine;
using System.Collections;
using System;
using System.IO;

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
	public static byte[] GetBytes(this object _obj, bool _isQuaternion){
		if(!_isQuaternion){
			Vector3 _vector = (Vector3) _obj;
			MemoryStream _result = new MemoryStream();
			_result.Append(Metwork.BitGetBytes(_vector.x));
			_result.Append(Metwork.BitGetBytes(_vector.y));
			_result.Append(Metwork.BitGetBytes(_vector.z));
			return _result.ToArray();
		}
		else{
			Quaternion _quaternion = (Quaternion)_obj;
			MemoryStream _result = new MemoryStream();
			_result.Append(Metwork.BitGetBytes(_quaternion.x));
			_result.Append(Metwork.BitGetBytes(_quaternion.y));
			_result.Append(Metwork.BitGetBytes(_quaternion.z));
			_result.Append(Metwork.BitGetBytes(_quaternion.w));
			return _result.ToArray();
		}
	}
	
}

