using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;



public class GameUI : MonoBehaviour {


	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	public Animator settingsPanelAnim;
	public Animator chatPanelAnim;


	public Toggle cubeToggle;
	public Toggle sphereToggle;
	public Toggle capsuleToggle;
	
	public Toggle blueToggle;
	public Toggle greenToggle;
	public Toggle redToggle;
	public Toggle yellowToggle;
	public Button forwardButton;
	public Button sendMessageButton;
	public Text chatContentText;
	public InputField chatMessageInputField;



	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------

	private GameManager gameManager;

	private const int CUBE_AVATAR = 0;
	private const int SPHERE_AVATAR = 1;
	private const int CAPSULE_AVATAR = 2;
	
	private const int BLUE_COLOR = 0;
	private const int GREEN_COLOR = 1;
	private const int RED_COLOR = 2;
	private const int YELLOW_COLOR = 3;

	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------

	void Start() {

		gameManager = GameObject.Find("Game").GetComponent<GameManager>();





	}

	//----------------------------------------------------------
	// Public interface methods for UI
	//----------------------------------------------------------

	public void SetAvatarSelection(int numModel) {
		// Update settings panel with the selected model
		switch (numModel) {
		case CUBE_AVATAR:
			cubeToggle.isOn = true;
			break;
		case SPHERE_AVATAR:
			sphereToggle.isOn = true;
			break;
		case CAPSULE_AVATAR:
			capsuleToggle.isOn = true;
			break;
		}
	}

	public void SetColorSelection(int numColor) {
		// Update settings panel with the selected color
		switch (numColor) {
		case BLUE_COLOR:
			blueToggle.isOn = true;
			break;
		case GREEN_COLOR:
			greenToggle.isOn = true;
			break;
		case RED_COLOR:
			redToggle.isOn = true;
			break;
		case YELLOW_COLOR:
			yellowToggle.isOn = true;
			break;
		}
	}

	public void OnSettingsButtonClick() {
		settingsPanelAnim.SetBool("panelOpen", !settingsPanelAnim.GetBool("panelOpen"));
	}

	public void OnChatButtonClick() {
		chatPanelAnim.SetBool("panelOpen", !chatPanelAnim.GetBool("panelOpen"));

		//Debug.Log ("Panel Open: " + chatPanelAnim.GetBool("panelOpen"));
		//gameManager.SetPlayerControllerStatus (!chatPanelAnim.GetBool("panelOpen"));
	}

	public void OnDisconnectButtonClick() {
		gameManager.Disconnect();
	}

	public void OnLeaveGameButtonClick() {
		gameManager.LeaveGame ();
	}

	public void OnSendChatMessageClick() {
		gameManager.SendPublicMessage (chatMessageInputField.text);
	}

	public void OnAvatarToggleChange() {
		if (gameManager != null) {
			if (cubeToggle.isOn)
				gameManager.ChangePlayerModel (CUBE_AVATAR);
			else if (sphereToggle.isOn)
				gameManager.ChangePlayerModel (SPHERE_AVATAR);
			else if (capsuleToggle.isOn)
				gameManager.ChangePlayerModel (CAPSULE_AVATAR);
		}
	}
	
	public void OnColorToggleChange() {
		if (gameManager != null) {
			if (blueToggle.isOn)
				gameManager.ChangePlayerMaterial (BLUE_COLOR);
			else if (greenToggle.isOn)
				gameManager.ChangePlayerMaterial (GREEN_COLOR);
			else if (redToggle.isOn)
				gameManager.ChangePlayerMaterial (RED_COLOR);
			else if (yellowToggle.isOn)
				gameManager.ChangePlayerMaterial (YELLOW_COLOR);
		}
	}


}