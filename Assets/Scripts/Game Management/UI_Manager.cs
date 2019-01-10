using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI_Manager : MonoBehaviour {

	public static UI_Manager _instance;

	public RectTransform healthBox;
	public RectTransform healthBar;

	//Ammo fields: private because Braden doesn't need to put his grubby
	//little hands all over them
	[SerializeField]
	private Animator ammoAnim;
	[SerializeField]
	private Text magAmmoText;
	[SerializeField]
	private Text totalAmmoText;

	//Pie Menu
	[SerializeField]
	private Image primaryPanel;
	[SerializeField]
	private Image secondaryPanel;
	public GameObject[] pieQuadrants;


	/// <summary>
	/// Pie state keeps track of the status of the pie. Center is when no segment group is selected
	/// While up, down, left and right keep track of which group (ex group 11,12,1) is selected
	/// </summary>
	public enum PieState
	{
		Center,
		Up,
		Right,
		Left,
		Down
	}
	public PieState pieState = PieState.Center;
	private bool isPieShown = false;
	public int selectedSegment = -1;
	public static int pieChoice = -1;


	public delegate void OnPieEvent(int _selectedSegment);
	public static OnPieEvent onPieEvent;



	// Use this for initialization
	void Start () {
		UI_Manager._instance = this;	
	}

	void LateUpdate(){
		pieChoice = -1;
		if (Input.GetKeyDown (KeyCode.LeftControl)) {
			isPieShown = true;

		} 
		if (Input.GetKeyUp (KeyCode.LeftControl)) {
			isPieShown = false;
			pieChoice = selectedSegment;
			//if (selectedSegment != -1 && onPieEvent != null) {
			//	onPieEvent.Invoke (selectedSegment);
			//}
		}
		LaunchPie ();
	}

	void LaunchPie(){
		for (int i = 0; i<pieQuadrants.Length; i++){
		//	pieQuadrants [i].SetActive (false);
		}
		if (!isPieShown) {
			pieState = PieState.Center;
			selectedSegment = -1;

			//Lengthening for optimization
			if(pieQuadrants[0].activeSelf){
					pieQuadrants [0].SetActive (false);
			}
			if(pieQuadrants[1].activeSelf){
					pieQuadrants [1].SetActive (false);
			}
			if(pieQuadrants[2].activeSelf){
					pieQuadrants [2].SetActive (false);
			}
			if(pieQuadrants[3].activeSelf){
					pieQuadrants [3].SetActive (false);
			}
			
			return;
		}

		if (pieState == PieState.Center) {
			for (int i = 0; i < pieQuadrants.Length; i++) {
				if(!pieQuadrants[i].activeSelf){
					pieQuadrants [i].SetActive (true);
				}
			}
			if (Input.GetAxis ("Move Z") > 0.5f) {
				pieState = PieState.Up;
			} else if (Input.GetAxis ("Move Z") < -0.5f) {
				pieState = PieState.Down;
			} else if (Input.GetAxis ("Move X") > 0.5f) {
				pieState = PieState.Right;
			} else if (Input.GetAxis ("Move X") < -0.5f) {
				pieState = PieState.Left;
			}
		} else if (pieState == PieState.Up) {
			pieQuadrants [0].SetActive (true);
			
			//Lengthening for optimization
			if(pieQuadrants[1].activeSelf){
					pieQuadrants [1].SetActive (false);
			}
			if(pieQuadrants[2].activeSelf){
					pieQuadrants [2].SetActive (false);
			}
			if(pieQuadrants[3].activeSelf){
					pieQuadrants [3].SetActive (false);
			}

			if (Input.GetAxis ("Move Z") > 0.2f) {
				//Up
				selectedSegment = 12;
			} else if (Input.GetAxis ("Move Z") < -0.2f) {
				//Down
				selectedSegment = -1;
				pieState = PieState.Center;
			} else if (Input.GetAxis ("Move X") > 0.2f) {
				//Right
				selectedSegment = 1;
			} else if (Input.GetAxis ("Move X") < -0.2f) {
				//Left
				selectedSegment = 11;
			}
		}
		else if (pieState == PieState.Right) {
			pieQuadrants [1].SetActive (true);

			//Lengthening for optimization
			if(pieQuadrants[0].activeSelf){
					pieQuadrants [0].SetActive (false);
			}
			if(pieQuadrants[2].activeSelf){
					pieQuadrants [2].SetActive (false);
			}
			if(pieQuadrants[3].activeSelf){
					pieQuadrants [3].SetActive (false);
			}

			if (Input.GetAxis ("Move Z") > 0.2f) {
				//Up
				selectedSegment = 2;
			} else if (Input.GetAxis ("Move Z") < -0.2f) {
				//Down
				selectedSegment = 4;

			} else if (Input.GetAxis ("Move X") > 0.2f) {
				//Right
				selectedSegment = 3;
			} else if (Input.GetAxis ("Move X") < -0.2f) {
				//Left
				selectedSegment = -1;
				pieState = PieState.Center;
			}
		}
		else{
			//Lengthening for optimization
			if(pieQuadrants[0].activeSelf){
					pieQuadrants [0].SetActive (false);
			}
			if(pieQuadrants[1].activeSelf){
					pieQuadrants [1].SetActive (false);
			}
			if(pieQuadrants[2].activeSelf){
					pieQuadrants [2].SetActive (false);
			}
			if(pieQuadrants[3].activeSelf){
					pieQuadrants [3].SetActive (false);
			}
		}
	}

	public void UpdateAmmo(int _magAmmo, int _magSize, int _totalAmmo){
		ammoAnim.SetFloat ("Normalized Ammo", (float)_magAmmo/(float)_magSize);
		magAmmoText.text = _magAmmo.ToString();
		totalAmmoText.text = _totalAmmo.ToString();
	}

	public void ChangeWeapon(bool _isPrimary){
		if (_isPrimary) {
			primaryPanel.enabled = true;
			secondaryPanel.enabled = false;
		}
		else {
			primaryPanel.enabled = false;
			secondaryPanel.enabled = true;
		}
	}

}
