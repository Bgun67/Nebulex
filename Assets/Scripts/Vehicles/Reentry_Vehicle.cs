using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Reentry_Vehicle : Ship_Controller
{

    public List<Cubemap> skyboxTextures;
	
    void CorrectPitch(){
        return;
    }
	//TODO: Move to updated code to main script
    protected override void Fly(){
		rb.AddRelativeForce (0f,moveY *deltaThrustForce* 2f,
			moveZ *deltaThrustForce);
		
		if(rb.useGravity){
			
			rb.AddRelativeForce(rb.mass * 9.81f * Mathf.Clamp(Vector3.Project(rb.velocity, transform.forward).sqrMagnitude * (1+Mathf.Sin(Vector3.SignedAngle(rb.velocity, transform.forward, transform.right))) * 0.002f,0, 2.0f) * 50f * Time.deltaTime * Vector3.up);
			rb.AddForce(Mathf.Sin(Vector3.Angle(rb.velocity, transform.forward))*-rb.velocity);
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
			rb.AddRelativeTorque(MInput.GetMouseDelta("Mouse Y") * -0.2f * deltaThrustForce * torqueFactor,
				MInput.GetMouseDelta("Mouse X") * 0.2f  * deltaThrustForce * torqueFactor,
				Input.GetAxis("Move X") * deltaThrustForce * -torqueFactor);
		}
		else
		{
			rb.AddRelativeTorque(MInput.GetAxis("Rotate X") * deltaThrustForce * torqueFactor,
				MInput.GetAxis("Rotate Y")  * deltaThrustForce * torqueFactor,
				Input.GetAxis("Move X") * deltaThrustForce * -torqueFactor);
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
				yield return new WaitForSeconds(1f);
			}
		}
    }
}
