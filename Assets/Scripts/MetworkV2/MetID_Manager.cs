using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using UnityEngine.SceneManagement;


#if UNITY_EDITOR || UNITY_EDITOR_64
[ExecuteInEditMode]
public class MetID_Manager : MonoBehaviour
{
	public bool readIdsFromFile;
	public bool checkCurrentSceneIDs;
	public bool fixAllIDs;
	public MetID_Storage[] formattedData;
	public List<MetworkViewConflict> conflictingViews;
	public int currentID;
	// Start is called before the first frame update

	// Update is called once per frame
	void OnGUI()
    {
		if (readIdsFromFile)
		{
			readIdsFromFile = false;
			ReadMetIDS();
		}
		if (checkCurrentSceneIDs)
		{
			checkCurrentSceneIDs = false;
			CheckCurrentSceneIDs();
		}
		if (fixAllIDs)
		{
			fixAllIDs = false;
			foreach (MetworkViewConflict conflictingView in conflictingViews)
			{
				conflictingView.fix = true;
			}
		}

		for (int i = 0; i < conflictingViews.Count; i++)
		{
			MetworkViewConflict conflictingView = conflictingViews[i];

			if (conflictingView.fix)
			{
				conflictingView.view.viewID = conflictingView.documentedID;
				conflictingViews.Remove(conflictingView);
				i--;
			}
		}

	}
	void ReadMetIDS()
	{
		string[] _rawData = System.IO.File.ReadAllLines(@"Assets/Scripts/MetworkV2/MetID_Storage.txt");
		GenerateBackup(_rawData);
		FormatData(_rawData);

	}
	void GenerateBackup(string [] _data)
	{
		System.IO.File.WriteAllLines(@"Assets/Scripts/MetworkV2/MetID_Storage (1).txt", _data);
	}
	void FormatData(string[] _data)
	{
		int _maxIndex = _data.Length;
		formattedData = new MetID_Storage[_maxIndex];
		for (int i = 0; i < _maxIndex; i++)
		{
			if (i % 1000 == 0)
			{
				Thread.Sleep(100);
			}
			string[] _chunk = _data[i].Split('\t');
			int _id;
			if (!int.TryParse(_chunk[0], out _id)){
				continue;
			}
			MetID_Storage _tmpContainer = new MetID_Storage(_id,_chunk[1], _chunk[2], _chunk[3], _chunk[4], _chunk[5]);
			formattedData[i] = _tmpContainer;
		}
	}
	void CheckCurrentSceneIDs()
	{
		MetworkView[] objs = FindObjectsOfType<MetworkView>();
		conflictingViews.Clear();
		CheckIDs(objs, SceneManager.GetActiveScene().name);
	}
	void CheckIDs(MetworkView[] _objs, string sceneName = "")
	{
		
		int _maxIndex = formattedData.Length;
		int _rootObjCount = _objs.Length;
		for (int i = 0; i < _maxIndex; i++)
		{
			currentID = i;

			string _rootName = GetRootName(i);
			string _parentName = formattedData[i].parentName;
			string _objectName = formattedData[i].objectName;
			if (_objectName == "")
			{
				continue;
			}
			if (sceneName != "" && formattedData[i].sceneName != "All" && formattedData[i].sceneName != sceneName)
			{
				continue;
			}
			//when the object has been checked, we will break this loop
			bool objectFound = false;
			for (int j = 0; j < _rootObjCount; j++)
			{
				MetworkView _metView = _objs[j];
				if (_metView.transform.name == _objectName)
				{
					if (_metView.transform.parent== null||_metView.transform.parent.name == _parentName)
					{
						if (_metView.transform.root.name == _rootName)
						{
							CheckID(_metView, i);
							objectFound = true;
							break;
						}
					}
					
				}
			}
			if (objectFound == false)
			{
				Debug.LogWarning("Could not find " + formattedData[i].objectName + " ID: " + formattedData[i].id + " at index " + i);
			}
		}
	}
	string GetRootName(int i)
	{
		if (formattedData[i].rootName == "")
		{
			return  formattedData[i].parentName;
		}
		else
		{
			return formattedData[i].rootName;
		}
	}
	
	void CheckID(MetworkView _view, int _index)
	{
		if (_view.viewID != formattedData[_index].id)
		{
			string path = "";
			if (_view.transform.parent != null)
			{
				path = _view.transform.root.name + _view.transform.parent.name + _view.transform.name;
			}
			Debug.LogWarning("Met ID Conflict: Path: " +path+
			"\r\n Documented ID: " + formattedData[_index].id + " Actual ID: " + _view.viewID);
			conflictingViews.Add(new MetworkViewConflict(_view.viewID, formattedData[_index].id,path, _view));
		}

	}
}
#endif