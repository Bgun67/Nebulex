using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reentry_Vehicle : Ship_Controller
{

    public List<Cubemap> skyboxTextures;
	public Vector3 cg;
	
	//void Start(){
		//InvokeRepeating("InvokeSky", 30f, 30f);
	//}
	void InvokeSky(){
		DeOrbit(Vector3.one);
	}
    void CorrectPitch(){
        return;
    }
	//TODO: Move to updated code to main script
    protected override void Fly(){
		rb.centerOfMass = cg;
		rb.AddRelativeForce (0f,moveY *deltaThrustForce* 2f,
			moveZ *deltaThrustForce);
		
		if(rb.useGravity){
			//Lift
			rb.AddForce(rb.mass * 9.81f * Mathf.Clamp(Vector3.Project(rb.velocity, transform.forward).sqrMagnitude * (1+Mathf.Sin(Vector3.SignedAngle(rb.velocity, transform.forward, transform.right))) * 0.002f,0, 1000.0f) * 10f * Time.deltaTime * Vector3.Cross(rb.velocity, transform.right).normalized);
			//Drag due to off angle
			//rb.AddForce(Mathf.Sin(Vector3.Angle(rb.velocity, transform.forward))*rb.velocity.magnitude*-rb.velocity);
			//Restoring force due to sideslip
			rb.AddRelativeTorque(new Vector3(
				-Vector3.SignedAngle(rb.velocity, transform.forward, transform.right)*0.1f,
				-Vector3.SignedAngle(rb.velocity, transform.forward, transform.up),
				//Pitch/roll coupling
				-Vector3.SignedAngle(rb.velocity, transform.forward, transform.up)*0.1f
			)*0.5f*rb.velocity.sqrMagnitude);
		}
		else
		{
			rb.AddRelativeForce(0f, 0f, deltaThrustForce*0.4f);
		}
		//rb.useGravity = false;

		engineSound.pitch = Mathf.Lerp(previousEnginePitch,Mathf.Clamp(Mathf.Abs(moveZ+moveX+moveY),0,0.1f) + (Time.frameCount % 5f)*0.003f  + 0.85f, 0.3f);
		previousEnginePitch = engineSound.pitch;
		if (MInput.useMouse)
		{
			rb.angularVelocity = transform.TransformDirection(MInput.GetMouseDelta("Mouse Y") * -0.2f * moveFactors.angularSpeed,
				MInput.GetMouseDelta("Mouse X") * 0.2f  * moveFactors.angularSpeed,
				Input.GetAxis("Move X") * moveFactors.angularSpeed);
		}
		else
		{
			rb.angularVelocity = transform.TransformDirection(MInput.GetAxis("Rotate X") * moveFactors.angularSpeed,
				MInput.GetAxis("Rotate Y")  * moveFactors.angularSpeed,
				Input.GetAxis("Move X") * moveFactors.angularSpeed);
		}
	}


    public void RPC_LandMode(bool on)
	{
		if (on)
		{
			
			rb.angularDrag = 1f;
			rb.drag = 0.2f;
		}
		else
		{
			
			rb.angularDrag = 0.5f;
			rb.drag = 0.1f;
		}
	}

	public void DeOrbit(Vector3 finalPosition){
		StartCoroutine(ChangeSkybox(true));
		rb.MovePosition(finalPosition);
	}

	IEnumerator ChangeSkybox(bool isDeOrbiting)
    {
		if(isDeOrbiting){
			int i = 0;
			while(i < skyboxTextures.Count){ 
				RenderSettings.skybox.SetTexture("_Tex", skyboxTextures[i]);
				i++;
				yield return new WaitForSeconds(0.5f);
			}
		}
    }
}
