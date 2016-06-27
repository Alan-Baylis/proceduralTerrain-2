using UnityEngine;
using System.Collections;

public class FlyCamera : MonoBehaviour {

	float mainSpeed  = 10.0f; 
	float shiftAdd = 50.0f;
	float maxShift = 1000.0f; 
	float camSens = 0.25f;
	private Vector3 lastMouse = new Vector3(255, 255, 255); 
	private float totalRun = 1.0f;
	public bool canMove = true;

	private void Update () {

		if (canMove) {
			lastMouse = Input.mousePosition - lastMouse ;
			lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0 );
			lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x , transform.eulerAngles.y + lastMouse.y, 0);
			transform.eulerAngles = lastMouse;
			lastMouse =  Input.mousePosition;
			//Mouse  camera angle done.  

			//Keyboard commands
			Vector3 p = GetBaseInput();
			if (Input.GetKey (KeyCode.LeftShift)){
				totalRun += Time.deltaTime;
				p  = p * totalRun * shiftAdd;
				p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
				p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
				p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
			}
			else{
				totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
				p = p * mainSpeed;
			}

			p = p * Time.deltaTime;
			if (Input.GetKey(KeyCode.Space)){ //If player wants to move on X and Z axis only
				float temp = transform.position.y;
				Vector3 temp2 = transform.position;
				temp2.y = temp;
				transform.Translate(p);
				transform.position = temp2;
			}
			else{
				transform.Translate( p);
			}
		}

	}

	private Vector3 GetBaseInput() { //returns the basic values, if it's 0 than it's not active.
		Vector3 p_Velocity = new Vector3();
		if (Input.GetKey (KeyCode.W)){
			p_Velocity += new Vector3(0, 0 , 1);
		}
		if (Input.GetKey (KeyCode.S)){
			p_Velocity += new Vector3(0, 0 , -1);
		}
		if (Input.GetKey (KeyCode.A)){
			p_Velocity += new Vector3(-1, 0 , 0);
		}
		if (Input.GetKey (KeyCode.D)){
			p_Velocity += new Vector3(1, 0 , 0);
		}
		return p_Velocity;
	}
}
