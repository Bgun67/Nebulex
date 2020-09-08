using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Reflection;
using System;
using System.IO;
using UnityEngine.SceneManagement;


[ExecuteInEditMode]
public class MetworkView : MonoBehaviour {


	const int INT = 0;
	const int STRING = 1;
	const int FLOAT = 2;
	const int VECTOR3 = 3;
	const int BOOL = 4;
	const int QUATERNION = 5;
	const int BYTES = 6;


	public int viewID = -1;

	List<MethodInfo> methods = new List<MethodInfo>();
	List<MonoBehaviour> scripts = new List<MonoBehaviour>();

	public void Start(){
		methods.Clear ();
		scripts.Clear ();
		MonoBehaviour[] _scripts = this.GetComponents<MonoBehaviour>();

		MonoBehaviour _behaviour;
		for (int j = 0; j < _scripts.Length; j++) {
			_behaviour = _scripts [j];

			MethodInfo[] objectFields = _behaviour.GetType ().GetMethods (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

			for (int i = 0; i < objectFields.Length; i++) {
				MRPC attribute = Attribute.GetCustomAttribute(objectFields[i],typeof(MRPC)) as MRPC;
				if(attribute != null){
					methods.Add (objectFields [i]);
					scripts.Add (_behaviour);
				}
			}
		}
	}

		



	public void TestServer(){
		Metwork.InitializeServer ("NOOOT");
	}
	public void TestConnect(){
		Metwork.Connect ("NOOOT");
	}


	void Reset(){

	}


	void OnEnable(){
		methods.Clear ();
		scripts.Clear ();
		MonoBehaviour[] _scripts = this.GetComponents<MonoBehaviour>();

		if (_scripts != null) {

			foreach (MonoBehaviour _behaviour in _scripts) {
				MethodInfo[] objectFields = _behaviour.GetType ().GetMethods (BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

				for (int i = 0; i < objectFields.Length; i++) {
					MRPC attribute = Attribute.GetCustomAttribute (objectFields [i], typeof(MRPC)) as MRPC;
					if (attribute != null) {
						methods.Add (objectFields [i]);
						scripts.Add (_behaviour);
					}
				}
			}
		}
	}
	/// <summary>
	/// Sends an RPC call
	/// </summary>
	/// <param name="_function">The function name</param>
	/// <param name="_player">The player to which this should be delivered should be delivered</param>
	/// <param name="_args">Arguments</param>
	public void RPC(string _function, MetworkPlayer _player, params object[] _args ){
		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length IN BYTES (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)

		//Here we determine the destination of the packet
		//In the case of AllBuffered, All, Others and OthersBuffered, the playernumber of the player the message originates from will be sent
		int _playerNumber = _player.connectionID;

		//If the packet is going to a specific player, the MRPCMode will be set and the destination player will be used
		//However, this will be covered in the other function
		MemoryStream _data = new MemoryStream();
		_data.Append(Metwork.BitGetBytes(_playerNumber));
		_data.Append(Metwork.BitGetBytes(Metwork.player.connectionID));
		_data.Append(Metwork.BitGetBytes(viewID));
		_data.Append(Metwork.BitGetBytes((int)MRPCMode.Player));
		_data.Append(Metwork.BitGetBytes(Encoding.UTF8.GetBytes(_function).Length));
		_data.Append(Encoding.UTF8.GetBytes(_function));
		_data.Append(Metwork.BitGetBytes(_args.Length));

		for(int i = 0; i< _args.Length; i++){
			int _typeNum = GetTypeNum(_args[i]);
			_data.Append (Metwork.BitGetBytes(_typeNum));
			byte[] _objData;
			if(_typeNum == VECTOR3){
				_objData = _args[i].GetBytes(false);
			}
			else if(_typeNum == QUATERNION){
				_objData = _args[i].GetBytes(true);
			}
			else if(_typeNum == BYTES){
				_objData = (byte[])_args[i];
			}
			else{
				//TODO: Fix;
				_objData = GetBytes(_args[i], _typeNum);
			}
			_data.Append(Metwork.BitGetBytes(_objData.Length));
			_data.Append(_objData);
		}

		Metwork.SendData (_data.ToArray (), MRPCMode.Player);
	}

	/// <summary>
	/// Sends an RPC call
	/// </summary>
	/// <param name="_function">The function name</param>
	/// <param name="_mode">How the call should be delivered</param>
	/// <param name="_args">Arguments</param>
	public void RPC(string _function, MRPCMode _mode, params object[] _args ){
		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)

		//Here we determine the destination of the packet
		//In the case of AllBuffered, All, Others and OthersBuffered, the playernumber of the player the message originates from will be sent
		int _playerNumber = Metwork.player.connectionID;
		//TODO: Fix Endianness

		//If the packet is going to a specific player, the MRPCMode will be set and the destination player will be used
		//However, this will be covered in the other function
		MemoryStream _data = new MemoryStream();
		_data.Append(Metwork.BitGetBytes(_playerNumber));
		_data.Append(Metwork.BitGetBytes(Metwork.player.connectionID));
		_data.Append(Metwork.BitGetBytes(viewID));
		_data.Append(Metwork.BitGetBytes((int)_mode));
		_data.Append(Metwork.BitGetBytes(Encoding.UTF8.GetBytes(_function).Length));
		_data.Append(Encoding.UTF8.GetBytes(_function));
		_data.Append(Metwork.BitGetBytes(_args.Length));

		for(int i = 0; i< _args.Length; i++){
			int _typeNum = GetTypeNum(_args[i]);
			_data.Append (Metwork.BitGetBytes(_typeNum));
			byte[] _objData;
			if(_typeNum == VECTOR3){
				_objData = _args[i].GetBytes(false);
			}
			else if(_typeNum == QUATERNION){
				_objData = _args[i].GetBytes(true);
			}
			else if(_typeNum == BYTES){				
				_objData = (byte[])_args[i];
			}
			else{
				//TODO: Fix;
				_objData = GetBytes(_args[i], _typeNum);
			}
			_data.Append(Metwork.BitGetBytes(_objData.Length));
			_data.Append(_objData);
		}
		Metwork.SendData (_data.ToArray (), _mode);

		//The send can't send itself messages, so we'll do this manually
		if(Metwork.isServer){
			switch (_mode) {
			case MRPCMode.All:
			case MRPCMode.AllBuffered:


					//Invoke the RPC
				this.RecieveRPC (_data.ToArray());
					
					
					break;
			}
		}
	}

	/// <summary>
	/// Internal use only
	/// </summary>
	public void RecieveRPC(byte[] _data){
		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)

		//Debug.Log (_message);
		int _functionNameLength = Metwork.TryToInt32(_data.Sub(16,4),0);
		string _functionName = Encoding.UTF8.GetString(_data, 20, _functionNameLength);
		
		

		bool _hasInvoked = false;
		//How the fuck I invoke this method:
		for(int i = 0; i< methods.Count; i++) {
			if (methods[i].Name != _functionName) {
				continue;
			}
			int _numArgs = Metwork.TryToInt32(_data.Sub(20 + _functionNameLength,4),0);
			int bytePosition = 20 + _functionNameLength + 4;
			object[] _arguments = new object[_numArgs];

			for (int j = 0; j < _numArgs; j++) {
				int _argLength = Metwork.TryToInt32(_data.Sub(bytePosition+4, 4),0);
				//That should probably be 8
				_arguments [j] = ConvertFromString (_data.Sub(bytePosition+8, _argLength),_data.Sub(bytePosition, 4));
				bytePosition += 8 + _argLength;
			}

			methods[i].Invoke (scripts[i], _arguments);
			_hasInvoked = true;
		}

		if (!_hasInvoked) {
			#if(UNITY_EDITOR || UNITY_EDITOR_64)
			Debug.Log ("Function " + _functionName + " does not exist");
			#endif
		}

	}

	int GetTypeNum<T>(T _object){
		
		if(_object.GetType() == typeof(int))
			return INT;
		if(_object.GetType() == typeof(float))
			return FLOAT;
		if(_object.GetType() == typeof(string))
			return STRING;
		if(_object.GetType() == typeof(bool))
			return BOOL;
		if(_object.GetType() == typeof(Vector3))
			return VECTOR3;
		if (_object.GetType () == typeof(Quaternion))
			return QUATERNION;
		if(_object.GetType() == typeof(byte[]))
			return BYTES;
		
		Debug.LogError ("Cannot use RPC with type: " + _object.GetType().ToString ());
		return -1;

	}

	

	object ConvertFromString(byte[] _string, byte[] _typeNum){
		//print (_typeNum);
		int _type = Metwork.TryToInt32(_typeNum,0);

		switch (_type) {
			case BOOL:
				return BitConverter.ToBoolean(_string,0);
			case STRING:
				return Encoding.UTF8.GetString(_string,0, _string.Length);
			case INT:
				return Metwork.TryToInt32(_string,0);
			case FLOAT:
				return Metwork.TryToSingle(_string,0);
			case VECTOR3:
				//Trim the brackets off the start and the en
				return new Vector3 (Metwork.TryToSingle(_string.Sub(0,4),0), Metwork.TryToSingle(_string.Sub(4,4),0),Metwork.TryToSingle(_string.Sub(8,4),0));
			case QUATERNION:
				return new Quaternion (Metwork.TryToSingle(_string.Sub(0,4),0), Metwork.TryToSingle(_string.Sub(4,4),0),Metwork.TryToSingle(_string.Sub(8,4),0),Metwork.TryToSingle(_string.Sub(12,4),0) );
			case BYTES:
				return (byte[])_string;
		}

		return null;

	}


	/// <summary>
	/// Unpacks the message and returns the MetworkView viewID.
	/// </summary>
	/// <returns>View ID the message is for</returns>
	public static int UnpackViewID(byte[] _buffer){
		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)

		
		//Return the first payload segment
		return Metwork.TryToInt32(_buffer.Sub(8,4),0);
	}
	/// <summary>
	/// Unpacks the message and returns the MRPCMode.
	/// </summary>
	/// <returns>MRPCMode the message is sent as</returns>
	public static MRPCMode UnpackMRPCMode(byte[] _buffer){
		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)

		//Return the first payload segment
		return (MRPCMode)Metwork.TryToInt32(_buffer.Sub(12,4),0);
	}
	/// <summary>
	/// Unpacks the message and returns the destination number
	/// </summary>
	/// <returns>The destination number of the message</returns>
	public static int UnpackDestinationNumber(byte[] _buffer){
		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)


		//Return the first payload segment
		return Metwork.TryToInt32(_buffer.Sub(0,4),0);
	}
	/// <summary>
	/// Unpacks the message and returns the destination number
	/// </summary>
	/// <returns>The destination number of the message</returns>
	public static int UnpackSourceNumber(byte[] _buffer){
		//The RPC data pack follows this format currently:
		//byte range: name (dataType)
		//0-3: Destination Number (int)
		//4-7: Source Number (int)
		//8-11: Metview ID(int)
		//12-15: MRPCMode (int)
		//16-19: Function name length (int)
		//20-20+n: Function name (utf8 string)
		//something - something + 4: Number of arguments (int)
		//After that it's
		//4 bytes: argType number (int)
		//4 bytes: arg Length in bytes (int)
		//n bytes: argument (bytes)


		//Return the first payload segment
		return Metwork.TryToInt32(_buffer.Sub(4,4),0);
	}

	public static byte[] Split(byte[] bytes, int segment){
		int segmentNum = 0;

		int startIndex = 0;
		int oldStartIndex = 0;
		int endIndex = 0;

		for(int i = 0; i<bytes.Length; i=2){
			if (bytes [i] == 0x25) {
				//print ("%");
				endIndex = i - 1;
				oldStartIndex = startIndex;
				startIndex = i + 1;
				if (segmentNum == segment) {
					//print ("segment start index: " + oldStartIndex + " end index: " + endIndex);
					//return the bytes in an array
					byte[] retBytes = new byte[endIndex - oldStartIndex + 1];
					for (int j = 0; j < retBytes.Length; j++) {
						retBytes [j] = bytes [j + oldStartIndex];
					}
					return retBytes;
				}

				segmentNum++;
			}
		}
		return new byte[0];


	}

	public static byte[] GetBytes(object _data, int _typeNum){
		switch (_typeNum){
			case INT:
				int  _int = (int) _data;
				return Metwork.BitGetBytes(_int);
			case STRING:
				string  _string = (string) _data;
				return Encoding.UTF8.GetBytes(_string);
			case FLOAT:
				float  _float = (float) _data;
				return Metwork.BitGetBytes(_float);
			case BOOL:
				bool  _bool = (bool) _data;
				return BitConverter.GetBytes(_bool);
			case BYTES:
				return (byte[])_data;
			default:
				Debug.Log("Could not Get bytes for data type: " + _typeNum.ToString());
				return (byte[])_data;
		}
	}
	
	
		
}
