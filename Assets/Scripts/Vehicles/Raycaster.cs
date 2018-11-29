using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using UnityEngine;

public class Raycaster : MonoBehaviour {

	public bool cast = false;
	public int maxRouteLength = 16;
	float maxVariation;
	int totalCasts = 0;
	public int maxCasts = 8000;


	[HideInInspector]
	public Queue <Vector3> route = new Queue<Vector3>(30);
	public Vector3 origin = Vector3.zero;
	public Vector3 target = new Vector3(30f,15f,15f);
	public float resolution = 10f;
	int routeLength = int.MaxValue;
	//List <Vector3> allPositions = new List<Vector3>();
	Dictionary<Vector3, int> allPositions = new Dictionary<Vector3, int> ();
	//List<int> allLengths = new List<int> ();

	Vector3 forwardVector;
	Vector3 backVector;
	Vector3 upVector;
	Vector3 downVector; 
	Vector3 leftVector; 
	Vector3 rightVector;

	bool totalCastOut = false;

	public LayerMask mask;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (cast) {
			cast = false;

			Cast ();
		}
	}

	public void Cast(){
		forwardVector = new Vector3 (0, 0f, resolution);
		backVector = new Vector3 (0, 0, -resolution);
		upVector = new Vector3 (0, resolution, 0);
		downVector = new Vector3 (0, -resolution, 0);
		leftVector = new Vector3 (-resolution, 0, 0);
		rightVector = new Vector3 (resolution, 0f, 0);

		route.Clear ();
		route = new Queue<Vector3> ();
		allPositions.Clear();
		//allLengths.Clear ();
		routeLength = int.MaxValue;
		maxVariation = maxRouteLength * resolution / 3f;
		totalCasts = 0;
		totalCastOut = false;

		Raycast (origin, new Queue<Vector3>());
		if (route.Count > 0) {
			print (route.ToArray () [route.Count - 1]);
			print ("Length: " + route.Count);
			Visualize (route.ToArray ());

		} else {
			print ("No route found");
			if (totalCastOut) {
				print ("Ran out of total casts");
			}

			if (allPositions.Count <= 64001) {
				Vector3[] queue = new Vector3[allPositions.Count];
				allPositions.Keys.CopyTo (queue,0);
				Visualize (queue);
			} else {
				print (allPositions.Count);
			}

		}
	}

	public void Visualize(Vector3[] queue){
		LineRenderer line;
		if (this.GetComponent<LineRenderer> () == null) {
			line = this.gameObject.AddComponent<LineRenderer> ();
		} else {
			line = this.GetComponent<LineRenderer> ();
		}
		line.hideFlags = HideFlags.HideInInspector;

		line.positionCount = queue.Length;
		line.SetPositions (queue);

	}

	public void Raycast(Vector3 position, Queue<Vector3> stack){
		//print ("cast");


		int stackCount = stack.Count + 1;
		if (stackCount >= routeLength) {
			return;
		}
		//if (totalCasts > maxCasts) {
		//	totalCastOut = true;
		//	return;
		//}
		if (stackCount > maxRouteLength) {
			return;
		}
		if ((position.x-origin.x) > maxVariation || (position.x-origin.x) < -maxVariation || (position.y-origin.y) > maxVariation || (position.y-origin.y) < -maxVariation || (position.z-origin.z) > maxVariation || (position.z-origin.z) < -maxVariation) {
			return;
		}
		Queue<Vector3> currStack = new Queue<Vector3>(stack);
		currStack.Enqueue (position);



		if (stackCount <= routeLength && (target - position).sqrMagnitude < resolution * resolution) {
			route = currStack;
			routeLength = route.Count;
			print ("Found route: " + route.Count);
			return;
		}

		//totalCasts++;

		//print (position);
		Vector3 newVector;

		newVector = position + forwardVector;

		int tmpRouteLength;

		if (allPositions.TryGetValue(newVector, out tmpRouteLength)) {
			totalCasts--;
			if (tmpRouteLength <= stackCount + 1) {
				//print("Contains");
				//return;
			} else {
				allPositions [newVector] = stackCount + 1;
				if (!Physics.Linecast (position, newVector,mask.value,QueryTriggerInteraction.Ignore)) {
					Raycast (newVector, currStack);
				}
			}
		}else {
			allPositions.Add (newVector,stackCount + 1);
			if(!Physics.Linecast(position, newVector,mask.value,QueryTriggerInteraction.Ignore)){
				Raycast(newVector, currStack);
			}
			//allLengths.Add (stackCount);
		}

		newVector = position + backVector;

		if (allPositions.TryGetValue(newVector, out tmpRouteLength)) {
			totalCasts--;
			if (tmpRouteLength <= stackCount + 1) {
				//print("Contains");
				//return;
			} else {
				allPositions [newVector] = stackCount + 1;
				if (!Physics.Linecast (position, newVector,mask.value,QueryTriggerInteraction.Ignore)) {
					Raycast (newVector, currStack);
				}
			}
		}else {
			allPositions.Add (newVector,stackCount + 1);
			if(!Physics.Linecast(position, newVector,mask.value,QueryTriggerInteraction.Ignore)){
				Raycast(newVector, currStack);
			}
			//allLengths.Add (stackCount);
		}

		newVector = position + upVector;

		if (allPositions.TryGetValue(newVector, out tmpRouteLength)) {
			totalCasts--;
			if (tmpRouteLength <= stackCount + 1) {
				//print("Contains");
				//return;
			} else {
				allPositions [newVector] = stackCount + 1;
				if (!Physics.Linecast (position, newVector,mask.value,QueryTriggerInteraction.Ignore)) {
					Raycast (newVector, currStack);
				}
			}
		}else {
			allPositions.Add (newVector,stackCount + 1);
			if(!Physics.Linecast(position, newVector,mask.value,QueryTriggerInteraction.Ignore)){
				Raycast(newVector, currStack);
			}
			//allLengths.Add (stackCount);
		}

		newVector = position + downVector;

		if (allPositions.TryGetValue(newVector, out tmpRouteLength)) {
			totalCasts--;
			if (tmpRouteLength <= stackCount + 1) {
				//print("Contains");
				//return;
			} else {
				allPositions [newVector] = stackCount + 1;
				if (!Physics.Linecast (position, newVector,mask.value,QueryTriggerInteraction.Ignore)) {
					Raycast (newVector, currStack);
				}
			}
		}else {
			allPositions.Add (newVector,stackCount + 1);
			if(!Physics.Linecast(position, newVector,mask.value,QueryTriggerInteraction.Ignore)){
				Raycast(newVector, currStack);
			}
			//allLengths.Add (stackCount);
		}

		newVector = position + leftVector;

		if (allPositions.TryGetValue(newVector, out tmpRouteLength)) {
			totalCasts--;
			if (tmpRouteLength <= stackCount + 1) {
				//print("Contains");
				//return;
			} else {
				allPositions [newVector] = stackCount + 1;
				if (!Physics.Linecast (position, newVector,mask.value,QueryTriggerInteraction.Ignore)) {
					Raycast (newVector, currStack);
				}
			}
		}else {
			allPositions.Add (newVector,stackCount + 1);
			if(!Physics.Linecast(position, newVector,mask.value,QueryTriggerInteraction.Ignore)){
				Raycast(newVector, currStack);
			}
			//allLengths.Add (stackCount);
		}

		newVector = position + rightVector;

		if (allPositions.TryGetValue(newVector, out tmpRouteLength)) {
			totalCasts--;
			if (tmpRouteLength <= stackCount + 1) {
				//print("Contains");
				//return;
			} else {
				allPositions [newVector] = stackCount + 1;
				if (!Physics.Linecast (position, newVector,mask.value,QueryTriggerInteraction.Ignore)) {
					Raycast (newVector, currStack);
				}
			}
		}else {
			allPositions.Add (newVector,stackCount + 1);
			if(!Physics.Linecast(position, newVector,mask.value,QueryTriggerInteraction.Ignore)){
				Raycast(newVector, currStack);
			}
			//allLengths.Add (stackCount);
		}
			


			

		
		//print ("Done");
	}
}
