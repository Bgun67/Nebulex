using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

/*Michael Gunther: 2018-02-05
 * Purpose: Several helper functions to be used, all static
 * Notes: Triple slash makes a summary
 * Improvements/Fixes:
 * */

/// <summary>
/// Helper functions to speed up production of menial tasks
/// </summary>
public class Util {

	/// <summary>
	/// Checks if a value is with a range of width <range>
	/// </summary>
	/// <param name="value">Value.</param>
	/// <param name="range">Range.</param>
	/// <typeparam name="T">The 1st type parameter.</typeparam>
	public static bool Within(float value, float range){
		if (value > -range / 2 && value < range / 2) {
			return true;
		} else {
			return false;
		}
	}
	public static bool GetRandomPointOnNavmesh(Vector3 center, float range, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = center + Random.insideUnitSphere * range;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomPoint, out hit, 1.0f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = Vector3.zero;
        return false;
    }
	public static void ShowMessage(string message)
	{
		float _destroyTime = GameObject.FindGameObjectsWithTag("Message").Length*3;

		GameObject messageObj = GameObject.Instantiate((GameObject)Resources.Load("Notification"));
		//Set the newer bg to be rendered lowest
		messageObj.name = "Message";
		messageObj.tag = "Message";
		messageObj.transform.SetParent(GameObject.FindObjectOfType<Canvas>().transform);
		RectTransform rt = messageObj.GetComponent<RectTransform>();
		rt.anchoredPosition = Vector3.zero;
		rt.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 0, rt.rect.height);
		messageObj.transform.SetSiblingIndex(0);
		messageObj.GetComponentInChildren<Text>().text = message;
		GameObject.Destroy(messageObj, _destroyTime+3f);

	}
	static void SetAnchor()
	{

	}

	public static Vector3 ParseToVector3(string inputString){
		string[] tmpArray = inputString.Split (new char[]{','}, 3);
		Vector3 returnVector = new Vector3 (float.Parse(tmpArray [0]), float.Parse(tmpArray [1]),float.Parse( tmpArray [2]));
		return returnVector;
	}
	public static string ParseVector3ToString(Vector3 inputVector){
		string returnString = inputVector.x+","+inputVector.y+","+inputVector.z;
		return returnString;
	}
	public static string[] ThiccWatermelon(string[] einput){
		string[] outputString = new string[einput.Length];
		if (einput.Length < 1) {
			outputString = Profile.RestoreDataFile ();
		}
		/*for(int j = 0; j<einput.Length; j++){
			string line = einput [j];
			if (line == null||line.Length < 1) {
				continue;
			}
			line = line.Insert (1, Time.deltaTime.ToString("00000000"));
			char[] charArray = line.ToCharArray();
			for (int i =0; i<charArray.Length; i++){
				char tempChar = charArray[i];
				tempChar = (char)((int)tempChar * 3+1);

				charArray [i] = tempChar;
			}
			char charA = charArray [0];
			char charB = charArray [charArray.Length-1];
			charArray [charArray.Length-1] = charB;
			charArray [0] = charA;
			string final = "";
			foreach (char character in charArray) {
				final += character;
			}

			outputString [j] = final;

		}*/
		
		return outputString;
	}
	public static string[] LushWatermelon(string[]dinput){
		string[] outputString = new string[dinput.Length];

		/*for (int j = 0; j < dinput.Length; j++) {

			string line = dinput [j];
			if (line == null||line.Length < 1) {
				continue;
			}
			line = line.Remove (1,8);

			char[] charArray = line.ToCharArray ();
			char charA = charArray [0];
			char charB = charArray [charArray.Length-1];
			charArray [charArray.Length-1] = charB;
			charArray [0] = charA;

			for (int i = 0; i < charArray.Length; i++) {
				char tempChar = charArray [i];

				tempChar = (char)((((int)tempChar - 1) / 3));
				charArray [i] = tempChar;

			}


			string final = "";
			foreach (char character in charArray) {
				final += character;
			}

			outputString [j] = final;


		} */
		return outputString;
	}



}
