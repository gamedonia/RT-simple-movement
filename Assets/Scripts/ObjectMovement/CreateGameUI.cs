using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Gamedonia.Rt;
using Gamedonia.Rt.Operations;
using Gamedonia.Rt.Events;
using Gamedonia.Rt.Entities;

public class CreateGameUI : MonoBehaviour {

	private GamedoniaRT gdRt;
	public InputField gameNameInputField;
	public InputField gamePasswordInputField;
	public Toggle neverToogle;
	public Toggle emptyToogle;
	public Toggle emptyWithTimeoutToogle;
	public Toggle emptyCreatorDisconnectedToogle;
	public Toggle hiddenToogle;


	public Text errorText;

	// Use this for initialization
	void Start () {

		gdRt = GamedoniaRT.SharedInstance ();

		reset ();

		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_CREATE_ROOM_SUCCESS,OnCreateRoomSuccess);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_CREATE_ROOM_ERROR,OnCreateRoomError);

		errorText.text = "";
	}


	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnCreateRoomSuccess(Gamedonia.Rt.Events.Event evt) {

		Application.LoadLevel ("ChooseGameGD");

	}

	public void OnCreateRoomError(Gamedonia.Rt.Events.Event evt) {

		string errorMsg = evt.data.GetString ("e");
		errorText.text = errorMsg;

	}

	public void OnCreateClick() {

		errorText.text = "";

		if (!gameNameInputField.Equals ("")) {
			Gamedonia.Rt.Entities.RoomSettings rsettings = new Gamedonia.Rt.Entities.RoomSettings ();
			rsettings.name = gameNameInputField.text;
			rsettings.isGame = true;
			rsettings.maxUsers = 8;
			rsettings.maxSpectators = 16;
			rsettings.isHidden = hiddenToogle.isOn;

			if (!"".Equals(gamePasswordInputField.text)) rsettings.password = gamePasswordInputField.text;

			if (neverToogle.isOn) {
				rsettings.removePolicy = Room.REMOVE_POLICY_NEVER;
			}else if (emptyToogle.isOn) {
				rsettings.removePolicy = Room.REMOVE_POLICY_EMPTY;
			}else if (emptyWithTimeoutToogle.isOn) {
				rsettings.removePolicy = Room.REMOVE_POLICY_EMPTY_WITH_TIMEOUT;
			}else if (emptyCreatorDisconnectedToogle.isOn) {
				rsettings.removePolicy = Room.REMOVE_POLICY_EMPTY_AND_CREATOR_DISCONNECTED;
			}





			
			CreateRoomOperation crop = new CreateRoomOperation (rsettings);
			gdRt.SendOp (crop);

		} else {
			errorText.text = "Game name cannot be empty";
		}

	}

	public void OnBackClick() {

		Application.LoadLevel ("ChooseGameGD");
	}

	private void reset() {
		GamedoniaRT.SharedInstance ().eventsDispatcher.RemoveAllEventListeners ();		
		enableLoginUI(true);
	}
	
	private void enableLoginUI(bool enable) {
		gameNameInputField.interactable = enable;
		errorText.text = "";
	}
}
