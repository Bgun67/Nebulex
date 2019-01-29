using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System.Reflection;
using System;
using UnityEngine.SceneManagement;


[ExecuteInEditMode]
public class MetworkView : MonoBehaviour {


	const int INT = 0;
	const int STRING = 1;
	const int FLOAT = 2;
	const int VECTOR3 = 3;
	const int BOOL = 4;
	const int QUATERNION = 5;


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

		print("Quaternion: " + ((Quaternion)(ConvertFromString(new Quaternion(0.35f,0.54f,0.12f,0.3f).ToString(true),"5"))).ToString(true));
		print(float.Parse("0.35"));

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
		//0: DestinationNumber%
		//1:SourceNumber%
		//2: MetViewID%
		//3: Function name%
		//4: MRPCMode number%
		//5: number of arguments%
		//6: argType 1 number%
		//7: arg1 value%
		//8: argType 2 number%
		//9: arg2 value %
		//&&% (The finishing character)

		//Here we determine the destination of the packet
		//In the case of AllBuffered, All, Others and OthersBuffered, the playernumber of the player the message originates from will be sent


		//If the packet is going to a specific player, the MRPCMode will be set and the destination player will be used
		int _playerNumber = _player.connectionID;

		StringBuilder _data = new StringBuilder ("");
		_data.Append (_playerNumber.ToString());
		_data.Append ("%");
		_data.Append (Metwork.player.connectionID.ToString());
		_data.Append ("%");
		_data.Append (viewID);
		_data.Append ("%");
		_data.Append (_function);
		_data.Append ("%");
		_data.Append ((int)MRPCMode.Player);
		_data.Append ("%");
		_data.Append (_args.Length);
		_data.Append ("%");
		for(int i = 0; i< _args.Length; i++){
			int _typeNum = GetTypeNum(_args[i]);
			_data.Append (_typeNum.ToString());
			_data.Append ("%");
			if(_typeNum == QUATERNION){
				_data.Append(_args[i].ToString(true));
			}else{
				_data.Append (_args[i].ToString());
			}
			_data.Append ("%");
		}
		_data.Append ("&&");
		_data.Append ("%");

		Metwork.SendData (_data.ToString (), MRPCMode.Player);
	}

	/// <summary>
	/// Sends an RPC call
	/// </summary>
	/// <param name="_function">The function name</param>
	/// <param name="_mode">How the call should be delivered</param>
	/// <param name="_args">Arguments</param>
	public void RPC(string _function, MRPCMode _mode, params object[] _args ){
		//The RPC data pack follows this format currently:
		//0: DestinationNumber%
		//1:SourceNumber%
		//2: MetViewID%
		//3: Function name%
		//4: MRPCMode number%
		//5: number of arguments%
		//6: argType 1 number%
		//7: arg1 value%
		//8: argType 2 number%
		//9: arg2 value %
		//&&% (The finishing character)

		//Here we determine the destination of the packet
		//In the case of AllBuffered, All, Others and OthersBuffered, the playernumber of the player the message originates from will be sent
		int _playerNumber = Metwork.player.connectionID;

		//If the packet is going to a specific player, the MRPCMode will be set and the destination player will be used
		//However, this will be covered in the other function

		StringBuilder _data = new StringBuilder ("");
		_data.Append (_playerNumber.ToString());
		_data.Append ("%");
		_data.Append (Metwork.player.connectionID.ToString());
		_data.Append ("%");
		_data.Append (viewID);
		_data.Append ("%");
		_data.Append (_function);
		_data.Append ("%");
		_data.Append ((int)_mode);
		_data.Append ("%");
		_data.Append (_args.Length);
		_data.Append ("%");
		for(int i = 0; i< _args.Length; i++){
			int _typeNum = GetTypeNum(_args[i]);
			_data.Append (_typeNum.ToString());
			_data.Append ("%");
			if(_typeNum == QUATERNION){
				_data.Append(_args[i].ToString(true));
			}
			else{
			_data.Append (_args[i].ToString());
			}
			_data.Append ("%");
		}
		_data.Append ("&&");
		_data.Append ("%");

		Metwork.SendData (_data.ToString (), _mode);

		//The send can't send itself messages, so we'll do this manually
		if(Metwork.isServer){
			switch (_mode) {
			case MRPCMode.All:
			case MRPCMode.AllBuffered:


					//Invoke the RPC
				this.RecieveRPC (_data.ToString().Split(new char[]{'%'},StringSplitOptions.RemoveEmptyEntries));
					
					
					break;
			}
		}
	}

	/// <summary>
	/// Internal use only
	/// </summary>
	public void RecieveRPC(string[] _data){
		//The RPC data pack follows this format currently:
		//0: DestinationNumber%
		//1:SourceNumber%
		//2: MetViewID%
		//3: Function name%
		//4: MRPCMode number%
		//5: number of arguments%
		//6: argType 1 number%
		//7: arg1 value%
		//8: argType 2 number%
		//9: arg2 value %
		//&&% (The finishing character)

		//Split the payload into segments
		//string[] _data = _message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries);

		//Debug.Log (_message);

		bool _hasInvoked = false;
		//How the fuck I invoke this method:
		for(int i = 0; i< methods.Count; i++) {
			if (methods[i].Name != _data [3]) {
				continue;
			}
			int _numArgs = int.Parse (_data [5]);
			object[] _arguments = new object[_numArgs];
			for (int j = 0; j < _numArgs; j++) {
				_arguments [j] = ConvertFromString (_data [7 + j * 2], _data [6 + j * 2]);
			}

			methods[i].Invoke (scripts[i], _arguments);
			_hasInvoked = true;
		}

		if (!_hasInvoked) {
			#if(UNITY_EDITOR || UNITY_EDITOR_64)
			Debug.Log ("Function " + _data [3] + " does not exist");
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
		
		Debug.LogError ("Cannot use RPC with type: " + _object.GetType().ToString ());
		return -1;

	}

	

	object ConvertFromString(string _string, string _typeNum){
		//print (_typeNum);
		int _type = int.Parse (_typeNum);

		switch (_type) {
			case BOOL:
				return bool.Parse (_string);
			case STRING:
				return _string;
			case INT:
				return int.Parse (_string);
			case FLOAT:
				return float.Parse (_string);
			case VECTOR3:
				//Trim the brackets off the start and the end
				string[] _components = _string.Split (new char[]{ ' ',',','(',')' }, StringSplitOptions.RemoveEmptyEntries);
				return new Vector3 (float.Parse (_components [0]), float.Parse (_components [1]), float.Parse (_components [2]));
			case QUATERNION:
				_components = _string.Split (new char[]{' ',',','(',')' }, StringSplitOptions.RemoveEmptyEntries);
				return new Quaternion (float.Parse (_components [0]), float.Parse (_components [1]), float.Parse (_components [2]),float.Parse (_components [3]));
		}

		return null;

	}


	/// <summary>
	/// Unpacks the message and returns the MetworkView viewID.
	/// </summary>
	/// <returns>View ID the message is for</returns>
	public static int UnpackViewID(string _message){
		//The RPC data pack follows this format currently:
		//0: DestinationNumber%
		//1:SourceNumber%
		//2: MetViewID%
		//3: Function name%
		//4: MRPCMode number%
		//5: number of arguments%
		//6: argType 1 number%
		//7: arg1 value%
		//8: argType 2 number%
		//9: arg2 value %
		//&&% (The finishing character)

		//print (Split (_message, 2)[0].ToString());
		//return (int)Split (_message, 2)[0];

		//print (_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries) [2]);
		//Return the first payload segment
		return int.Parse(_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries)[2]);
	}
	/// <summary>
	/// Unpacks the message and returns the MRPCMode.
	/// </summary>
	/// <returns>MRPCMode the message is sent as</returns>
	public static MRPCMode UnpackMRPCMode(string _message){
		//The RPC data pack follows this format currently:
		//0: DestinationNumber%
		//1:SourceNumber%
		//2: MetViewID%
		//3: Function name%
		//4: MRPCMode number%
		//5: number of arguments%
		//6: argType 1 number%
		//7: arg1 value%
		//8: argType 2 number%
		//9: arg2 value %
		//&&% (The finishing character)

		//print (_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries) [4]);
		//Return the first payload segment
		return (MRPCMode)int.Parse(_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries)[4]);
	}
	/// <summary>
	/// Unpacks the message and returns the destination number
	/// </summary>
	/// <returns>The destination number of the message</returns>
	public static int UnpackDestinationNumber(string _message){
		//The RPC data pack follows this format currently:
		//0: DestinationNumber%
		//1:SourceNumber%
		//2: MetViewID%
		//3: Function name%
		//4: MRPCMode number%
		//5: number of arguments%
		//6: argType 1 number%
		//7: arg1 value%
		//8: argType 2 number%
		//9: arg2 value %
		//&&% (The finishing character)

		//print (_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries) [0]);
		//Return the first payload segment
		return int.Parse(_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries)[0]);
	}
	/// <summary>
	/// Unpacks the message and returns the destination number
	/// </summary>
	/// <returns>The destination number of the message</returns>
	public static int UnpackSourceNumber(string _message){
		//The RPC data pack follows this format currently:
		//0: DestinationNumber%
		//1:SourceNumber%
		//2: MetViewID%
		//3: Function name%
		//4: MRPCMode number%
		//5: number of arguments%
		//6: argType 1 number%
		//7: arg1 value%
		//8: argType 2 number%
		//9: arg2 value %
		//&&% (The finishing character)

		//print (_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries) [1]);
		//Return the first payload segment
		return int.Parse(_message.Split (new char[]{ '%' }, System.StringSplitOptions.RemoveEmptyEntries)[1]);
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
	
		
}
