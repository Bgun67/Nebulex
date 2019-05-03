using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MetID_Storage
{

	public int id;
	public string rootName;
	public string parentName;
	public string objectName;
	public string scriptName;
	public string sceneName;
	public MetID_Storage(int _newID, string _newRoot, string _newParentName, string _newObjectName, string _newScriptName, string _newSceneName)
	{
		id = _newID;
		rootName = _newRoot;
		parentName = _newParentName;
		objectName = _newObjectName;
		scriptName = _newScriptName;
		sceneName = _newSceneName;
	}
	public MetID_Storage()
	{
		
	}

}
[System.Serializable]
public class MetworkViewConflict
{

	public int currentID;
	public int documentedID;
	public MetworkView view;
	public string path;
	public bool fix;
	public MetworkViewConflict(int _newcurrentID, int _newDocumentedID, string _newPath, MetworkView _newView)
	{
		currentID = _newcurrentID;
		documentedID = _newDocumentedID;
		path = _newPath;
		view = _newView;
	}
	public MetworkViewConflict()
	{
		
	}

}

