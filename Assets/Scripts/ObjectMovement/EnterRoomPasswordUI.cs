using UnityEngine;
using UnityEngine.UI;
using Gamedonia.Rt;
using Gamedonia.Rt.Operations;
using Gamedonia.Rt.Events;

using System.Collections;

public class EnterRoomPasswordUI : MonoBehaviour {


	public Text roomName;
	public InputField gamePasswordInputField;
	public Text errorText;

	private GamedoniaRT gdRt;


	// Use this for initialization
	void Start () {
		roomName.text = ChooseGameUI.selectedRoom;

		gdRt = GamedoniaRT.SharedInstance ();
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_JOIN_ROOM_SUCCESS, OnJoinRoomSuccess);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_JOIN_ROOM_ERROR, OnJoinRoomError);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnJoinClick() {

		JoinRoomOperation jrop = new JoinRoomOperation (ChooseGameUI.selectedRoom);	

		//jrop.password
		jrop.SetPassword(gamePasswordInputField.text);

		if (ChooseGameUI.spectator) {
			jrop.JoinAsSpectator ();
		}
		
		gdRt.SendOp (jrop);
	}

	public void OnJoinRoomSuccess(Gamedonia.Rt.Events.Event evt) {
		
		Debug.Log ("Join Room Success!!!!");
		Application.LoadLevel ("GameGD");
		
	}
	
	public void OnJoinRoomError(Gamedonia.Rt.Events.Event evt) {
		
		string errorMsg = evt.data.GetString("error");
		errorText.text = "Unable to join room, " + errorMsg;
		
	}
}
