using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {
	
	public float forwardSpeed = 10;
	public float backwardSpeed = 8;
	public float rotationSpeed = 40;

	private bool forward = false;
	private bool backward = false;
	private bool left = false;
	private bool right = false;

	// Dirty flag for checking if movement was made or not
	public bool MovementDirty {get; set;}

	void Start() {
		MovementDirty = false;
	}
	
	void Update () {


		// Forward/backward makes player model move
		float translation = Input.GetAxis("Vertical");

		if (forward)
			translation = 1.0f;

		if (backward)
			translation = -1.0f;
	
		//Debug.Log (translation);


		if (translation != 0) {
			this.transform.Translate(0, 0, translation * Time.deltaTime * forwardSpeed);
			MovementDirty = true;
		}
	
		// Left/right makes player model rotate around own axis
		float rotation = Input.GetAxis("Horizontal");

		if (left)
			rotation = -1.0f;
	
		if (right)
			rotation = 1.0f;

		if (rotation != 0) {
			this.transform.Rotate(Vector3.up, rotation * Time.deltaTime * rotationSpeed);
			MovementDirty = true;
		}


	
	}

	public void OnForwardDown() {
		
		forward = true;
		
	}
	
	public void OnForwardUp() {
		
		forward = false;
		
	}

	public void OnBackwardDown() {
		
		backward = true;
		
	}
	
	public void OnBackwardUp() {
		
		backward = false;
		
	}

	public void OnTurnLeftDown() {
		
		left = true;
		
	}
	
	public void OnTurnLeftUp (){
		
		left = false;
		
	}

	public void OnTurnRightDown() {
		
		right = true;
		
	}
	
	public void OnTurnRightUp (){
		
		right = false;
		
	}




}
