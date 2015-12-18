using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Gamedonia.Rt;
using Gamedonia.Rt.Entities;
using Gamedonia.Rt.Operations;
using Gamedonia.Rt.Events;


public class ChooseGameUI : MonoBehaviour {


	public static string selectedRoom;
	public static bool spectator;

	EventTrigger eventTrigger = null;

	public EventSystem eventSystem;
	public Toggle spectatorToggle;
	public Button createNewGame;
	public Text errorText;

	private GamedoniaRT gdRt;

	void PrintRoomsList ()
	{
		errorText.text = "";
		GameObject grid = GameObject.Find ("GridWithElements");
		ICollection<Room> rooms = gdRt.roomsManager.GetRoomsList ();
		foreach (Room room in rooms) {
			GameObject roomName = Instantiate (Resources.Load ("RoomName")) as GameObject;
			roomName.transform.SetParent (grid.transform);
			Text roomTextLabel = roomName.GetComponent<Text> ();
			roomTextLabel.text = room.name + "(" + room.playersCount + ") (" + room.spectatorsCount + ")";
			if (room.IsPasswordProtected()) roomTextLabel.text += "*";
			eventTrigger = roomName.GetComponent<EventTrigger> ();
			AddEventTrigger (OnSelectRoomClick, EventTriggerType.PointerClick);
		}
	}

	// Use this for initialization
	void Start () {
	
		reset ();

		gdRt = GamedoniaRT.SharedInstance ();
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_JOIN_ROOM_SUCCESS, OnJoinRoomSuccess);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_JOIN_ROOM_ERROR, OnJoinRoomError);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_ROOM_ADDED_TO_GROUP, OnRoomAddedToGroup);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_USERS_IN_ROOMS_CHANGED, OnRoomAddedToGroup); // Same callback, we just refresh
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_ROOM_WAS_CLOSED, OnRoomAddedToGroup);

		PrintRoomsList ();

	}
	
	// Update is called once per frame
	void Update () {
	
	}



	public void OnJoinRoomSuccess(Gamedonia.Rt.Events.Event evt) {

		Debug.Log ("Join Room Success!!!!");
		Application.LoadLevel ("GameGD");

	}

	public void OnJoinRoomError(Gamedonia.Rt.Events.Event evt) {

		string errorMsg = evt.data.GetString("error");
		errorText.text = "Unable to join room, " + errorMsg;

	}

	public void OnRoomAddedToGroup(Gamedonia.Rt.Events.Event evt) {

		OnRefreshRoomsClick ();

	}

	public void OnCreateNewGameClick() {

		Debug.Log ("Create a new game");
		Application.LoadLevel ("CreateGameGD");
	}

	public void OnSelectRoomClick(BaseEventData data) {

		GamedoniaRT gd = GamedoniaRT.SharedInstance ();

		//Debug.Log ("Room Name Clicked: " + eventSystem.currentSelectedGameObject.name);
		Text roomLabel = eventSystem.currentSelectedGameObject.GetComponent<Text> ();
		string roomName = roomLabel.text.Substring (0, roomLabel.text.IndexOf ("("));

		ChooseGameUI.selectedRoom = roomName;
		ChooseGameUI.spectator = spectatorToggle.isOn;

		Room room = gd.roomsManager.GetRoomByName (roomName);

		if (room.IsPasswordProtected ()) {

			Application.LoadLevel("EnterRoomPasswordGD");


		} else {

			JoinRoomOperation jrop = new JoinRoomOperation (roomName);	
			
			if (spectatorToggle.isOn) {
				jrop.JoinAsSpectator ();
			}
			
			gd.SendOp (jrop);

		}

	}

	public void OnRefreshRoomsClick() {


		GameObject grid = GameObject.Find ("GridWithElements");

		var children = new List<GameObject>();
		foreach (Transform child in grid.transform) children.Add(child.gameObject);
		children.ForEach(child => Destroy(child));

		PrintRoomsList ();
	}

	public void OnMatchMakingClick() {

		Application.LoadLevel("MatchMakingGD");

	}

	#region TriggerEventsSetup
	
	private void AddEventTrigger(UnityAction action, EventTriggerType triggerType)
	{
		// Create a nee TriggerEvent and add a listener
		EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
		trigger.AddListener((eventData) => action()); // you can capture and pass the event data to the listener
		
		// Create and initialise EventTrigger.Entry using the created TriggerEvent
		EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = triggerType };
		
		// Add the EventTrigger.Entry to delegates list on the EventTrigger
		eventTrigger.triggers.Add(entry);
	}
	
	private void AddEventTrigger(UnityAction<BaseEventData> action, EventTriggerType triggerType)
	{
		// Create a nee TriggerEvent and add a listener
		EventTrigger.TriggerEvent trigger = new EventTrigger.TriggerEvent();
		trigger.AddListener((eventData) => action(eventData)); // you can capture and pass the event data to the listener
		
		// Create and initialise EventTrigger.Entry using the created TriggerEvent
		EventTrigger.Entry entry = new EventTrigger.Entry() { callback = trigger, eventID = triggerType };
		
		// Add the EventTrigger.Entry to delegates list on the EventTrigger
		eventTrigger.triggers.Add(entry);
	}
	
	#endregion

	private void reset() {

		GamedoniaRT.SharedInstance ().eventsDispatcher.RemoveAllEventListeners ();		
		enableLoginUI(true);
	}
	
	private void enableLoginUI(bool enable) {
		createNewGame.interactable = enable;
		errorText.text = "";
	}
}
