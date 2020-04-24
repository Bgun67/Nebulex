using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PilotAR : MonoBehaviour
{
	bool reverse;
	Animator anim;
	public LineRenderer predictedPath;
	// Start is called before the first frame update
	void Start()
    {
		anim = GetComponent<Animator>();
	}
	public void ChargeCannon(bool charge)
	{
		anim.SetBool("Charge", charge);
	}
	public void FireCannon(bool fire)
	{
		anim.SetBool("Fire", fire);
	}

	// Update is called once per frame
	public void ShowGauges(float throttle, float steering)
	{

		anim.SetFloat("LThrottle", (steering + throttle) * 0.5f);
		anim.SetFloat("CThrottle", throttle);
		anim.SetFloat("RThrottle", (throttle - steering) * 0.5f);
	}
	public void DrawPredictedPath(Rigidbody rb){
        float startTime = 0f;
        float endTime = 10f;
        int timeSteps = 100;
        float stepWidth = (startTime - endTime) / timeSteps;

        float velocity = rb.velocity.magnitude;
        Vector3 rotVelocity = rb.angularVelocity;
        Vector3 previousPosition = predictedPath.transform.position;
        Vector3 currentForward = -rb.velocity;

        predictedPath.positionCount = timeSteps;
        predictedPath.SetPosition(0, previousPosition);

        for (int i = 1; i < timeSteps; i++)
        {
			
            float sine = Mathf.Sin(i * stepWidth * rotVelocity.y);
            float cosine = Mathf.Cos(i * stepWidth * rotVelocity.y);

            //Rotate the forward vector
            currentForward = new Vector3(currentForward.x * cosine - currentForward.z * sine, 0, (currentForward.x * sine + currentForward.z * cosine));

            predictedPath.SetPosition(i, predictedPath.GetPosition(i - 1) + (i * stepWidth * velocity * currentForward));
        }
    }
}
