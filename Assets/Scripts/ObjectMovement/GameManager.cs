using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Gamedonia.Rt;
using Gamedonia.Rt.Events;
using Gamedonia.Rt.Types;
using Gamedonia.Rt.Operations;
using Gamedonia.Rt.Entities;
using System;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	//----------------------------------------------------------
	// Public properties
	//----------------------------------------------------------

	public GameObject[] playerModels;
	public Material[] playerMaterials;

	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------
	
	private GamedoniaRT gdRt;

	private GameUI gameUI;


	private GameObject localPlayer;
	private PlayerController localPlayerController;
	private Dictionary<long, GameObject> gdRemotePlayers = new Dictionary<long, GameObject>();

	private bool isSpectator = false;

	public Button forwardButton;


	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------

	void Start() {

		gameUI = GameObject.Find("UI").GetComponent<GameUI>();

		gdRt = GamedoniaRT.SharedInstance ();

		reset ();

		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_CONNECTION_LOST, OnConnectionLost);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_USER_JOINED_ROOM, OnUserJoinedRoom);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_SET_USER_VARIABLES_SUCCESS, OnSetUserVariablesSuccess);
		gdRt.eventsDispatcher.AddEventListener (GamedoniaEvents.ON_USER_LEFT_ROOM, OnUserLeftRoom);
		gdRt.AddEventListener (GamedoniaEvents.ON_LEAVE_ROOM_SUCCESS, OnLeaveRoomSuccess);
		gdRt.AddEventListener (GamedoniaEvents.ON_LEAVE_ROOM_ERROR, OnLeaveRoomError);
		gdRt.AddEventListener (GamedoniaEvents.ON_SEND_PUBLIC_MESSAGE_SUCCESS, OnPublicMessageSuccess);
		gdRt.AddEventListener (GamedoniaEvents.ON_PUBLIC_MESSAGES_RECEIVED, OnPublicMessagesReceived);


		isSpectator = gdRt.roomsManager.GetLastJoinedRoom ().IsSpectator (gdRt.me);

		if (!isSpectator) {
			// Random avatar and color
			int numModel = UnityEngine.Random.Range (0, playerModels.Length);
			int numMaterial = UnityEngine.Random.Range (0, playerMaterials.Length);
			SpawnLocalPlayer (numModel, numMaterial);

			GameUI ui = GameObject.Find ("UI").GetComponent ("GameUI") as GameUI;
			ui.SetAvatarSelection (numModel);
			ui.SetColorSelection (numMaterial);
		}


		//Cargamos en la ventana del chat los mensajes que tengamos almacenados en el Buffer hasta el momento
		Room room = gdRt.roomsManager.GetLastJoinedRoom ();

		foreach (Map message in room.GetPublicMessages()) {
								
			gameUI.chatContentText.text += "<b><color=darkblue>" + message.GetString("u") +"</color></b>: "+  message.GetString("m") + "\n";

		}


		//Serializamos a fichero un mensaje
		List<Gamedonia.Rt.Entities.UserVariable> userVariables = new List<Gamedonia.Rt.Entities.UserVariable>();
		userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("x", 1.1));
		userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("y", 1.653));
		userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("z", 1.2332));
		userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("rot", 0.68));
		SetUserVariablesOperation suvop = new SetUserVariablesOperation(userVariables,gdRt.me);	
		suvop.WriteFile ();

	}
	
	void FixedUpdate() {

		if (gdRt != null) {

			if (localPlayer != null && localPlayerController != null && localPlayerController.MovementDirty) {
				IList<Gamedonia.Rt.Entities.UserVariable> userVariables = new List<Gamedonia.Rt.Entities.UserVariable>();
				userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("x", (double)localPlayer.transform.position.x));
				userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("y", (double)localPlayer.transform.position.y));
				userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("z", (double)localPlayer.transform.position.z));
				userVariables.Add(new Gamedonia.Rt.Entities.UserVariable("rot", (double)localPlayer.transform.rotation.eulerAngles.y));
				SetUserVariablesOperation suvop = new SetUserVariablesOperation(userVariables,GamedoniaRT.SharedInstance().me);				
				gdRt.SendOp(suvop);			
				localPlayerController.MovementDirty = false;
			}

		}

	}
	
	void OnApplicationQuit() {
		// Before leaving, lets notify the others about this client dropping out
		RemoveLocalPlayer();
		LogoutOperation lop = new LogoutOperation ();
		gdRt.SendOp (lop);
	}

	//----------------------------------------------------------
	// Gamedonia event listeners
	//----------------------------------------------------------

	public void OnConnectionLost(Gamedonia.Rt.Events.Event evt) {

		gdRt.eventsDispatcher.RemoveAllEventListeners ();
		Application.LoadLevel("ConnectionGD");

	}

	public void OnUserLeftRoom(Gamedonia.Rt.Events.Event evt) {

		Gamedonia.Rt.Entities.User user = evt.data.GetUser ("user");
		RemoveRemotePlayer (user);

	}

	void OnUserJoinedRoom(Gamedonia.Rt.Events.Event evt) {


		if (localPlayer != null) {
			
			IList<Gamedonia.Rt.Entities.UserVariable> userVariables = new List<Gamedonia.Rt.Entities.UserVariable> ();
			userVariables.Add (new Gamedonia.Rt.Entities.UserVariable ("x", (double)localPlayer.transform.position.x));
			userVariables.Add (new Gamedonia.Rt.Entities.UserVariable ("y", (double)localPlayer.transform.position.y));
			userVariables.Add (new Gamedonia.Rt.Entities.UserVariable ("z", (double)localPlayer.transform.position.z));
			userVariables.Add (new Gamedonia.Rt.Entities.UserVariable ("rot", (double)localPlayer.transform.rotation.eulerAngles.y));
			userVariables.Add (new Gamedonia.Rt.Entities.UserVariable ("model", (int)gdRt.me.GetVariable ("model").GetLong ()));
			userVariables.Add (new Gamedonia.Rt.Entities.UserVariable ("mat", (int)gdRt.me.GetVariable ("mat").GetLong ()));
			SetUserVariablesOperation suvop = new SetUserVariablesOperation (userVariables, gdRt.me);
			gdRt.SendOp (suvop);
		}

	}

	public void OnLeaveRoomSuccess(Gamedonia.Rt.Events.Event evt) {

		Application.LoadLevel ("ChooseGameGD");

	}

	public void OnLeaveRoomError(Gamedonia.Rt.Events.Event evt) {

		string errorMsg = evt.data.GetString ("e");
		Debug.Log ("Error leaving the room, "  + errorMsg);
	}


	void OnSetUserVariablesSuccess(Gamedonia.Rt.Events.Event evt) {


		//Debug.Log ("On Set User Variable");

		Gamedonia.Rt.Entities.User user = evt.data.GetUser ("user");
		Gamedonia.Rt.Types.Array changedVars = (Gamedonia.Rt.Types.Array)evt.data.GetArray("changedVarNames");

		if (user.userId == gdRt.me.userId)
			return;

		if (!gdRemotePlayers.ContainsKey (user.userId)) {

			// New client just started transmitting - lets create remote player
			Vector3 pos = new Vector3(0, 1, 0);
			if (user.ContainsVariable("x") && user.ContainsVariable("y") && user.ContainsVariable("z")) {
				pos.x = (float)user.GetVariable("x").GetDouble();
				pos.y = (float)user.GetVariable("y").GetDouble();
				pos.z = (float)user.GetVariable("z").GetDouble();
			}
			float rotAngle = 0;
			if (user.ContainsVariable("rot")) {
				rotAngle = (float)user.GetVariable("rot").GetDouble();
			}
			int numModel = 0;
			if (user.ContainsVariable("model")) {
				numModel = (int)user.GetVariable("model").GetLong();
			}
			int numMaterial = 0;
			if (user.ContainsVariable("mat")) {
				numMaterial = (int)user.GetVariable("mat").GetLong();
			}
			SpawnRemotePlayer(user.userId, numModel, numMaterial, pos, Quaternion.Euler(0, rotAngle, 0));
		}

		// Check if the remote user changed his position or rotation
		if (changedVars.Contains("x") && changedVars.Contains("y") && changedVars.Contains("z") && changedVars.Contains("rot")) {
			// Move the character to a new position...
			gdRemotePlayers[user.userId].GetComponent<SimpleRemoteInterpolation>().SetTransform(
				new Vector3((float)user.GetVariable("x").GetDouble(), (float)user.GetVariable("y").GetDouble(), (float)user.GetVariable("z").GetDouble()),
				Quaternion.Euler(0, (float)user.GetVariable("rot").GetDouble(), 0),
				true);

		}
		
		// Remote client selected new model?
		if (changedVars.Contains("model")) {
			SpawnRemotePlayer(user.userId, (int)user.GetVariable("model").GetLong(), (int)user.GetVariable("mat").GetLong(), gdRemotePlayers[user.userId].transform.position, gdRemotePlayers[user.userId].transform.rotation);
		}
		
		// Remote client selected new material?
		if (changedVars.Contains("mat")) {
			gdRemotePlayers[user.userId].GetComponentInChildren<Renderer>().material = playerMaterials[ (int)user.GetVariable("mat").GetLong() ];
		}


	}

	void OnPublicMessagesReceived (Gamedonia.Rt.Events.Event evt) {

		Room room = evt.data.GetRoom ("room");

		if (gdRt.roomsManager.GetLastJoinedRoom ().roomId == room.roomId) {

			Gamedonia.Rt.Types.Array messages = evt.data.GetArray ("messages");


			for (int i =0; i< messages.Count; i++) {

				Map message = messages.GetMap(i);

				gameUI.chatContentText.text += "<b><color=darkblue>" + message.GetString("u") +"</color></b>: " +  message.GetString("m") + "\n";

			}

		}


	}
	

	void OnPublicMessageSuccess (Gamedonia.Rt.Events.Event evt) {


		Debug.Log ("Public message send successfully");

		gameUI.chatContentText.text += "<b><color=purple>me:</color></b> " +  gameUI.chatMessageInputField.text + "\n";

	}


	//----------------------------------------------------------
	// Public interface methods for UI
	//----------------------------------------------------------

	public void Disconnect() {

		LogoutOperation lop = new LogoutOperation ();
		gdRt.SendOp (lop);
		gdRt.Disconnect ();

	}

	public void LeaveGame() {

		LeaveRoomOperation lrop = new LeaveRoomOperation (gdRt.roomsManager.GetLastJoinedRoom());
		gdRt.SendOp (lrop);

		if (localPlayer != null) Destroy (localPlayer);

	}
	
	public void ChangePlayerMaterial(int numMaterial) {
		localPlayer.GetComponentInChildren<Renderer>().material = playerMaterials[numMaterial];

		IList<Gamedonia.Rt.Entities.UserVariable> userVariables = new List<Gamedonia.Rt.Entities.UserVariable> ();
		userVariables.Add (new Gamedonia.Rt.Entities.UserVariable("mat",numMaterial));
		gdRt.SendOp (new SetUserVariablesOperation(userVariables,gdRt.me));
	}
	
	public void ChangePlayerModel(int numModel) {
		SpawnLocalPlayer(numModel, gdRt.me.GetVariable("mat").GetInt());
	}
	
	//----------------------------------------------------------
	// Private player helper methods
	//----------------------------------------------------------
	
	private void SpawnLocalPlayer(int numModel, int numMaterial) {
		Vector3 pos;
		Quaternion rot;
		

		if (localPlayer != null) {
			pos = localPlayer.transform.position;
			rot = localPlayer.transform.rotation;
			Camera.main.transform.parent = null;
			Destroy(localPlayer);
		} else {
			pos = new Vector3(0, 1, 0);
			rot = Quaternion.identity;
		}
		
		//Span the local player
		localPlayer = GameObject.Instantiate(playerModels[numModel]) as GameObject;
		localPlayer.transform.position = pos;
		localPlayer.transform.rotation = rot;
		
		// Setup the material
		localPlayer.GetComponentInChildren<Renderer>().material = playerMaterials[numMaterial];

		//Setup camera
		localPlayer.AddComponent<PlayerController>();
		localPlayerController = localPlayer.GetComponent<PlayerController>();
		localPlayer.GetComponentInChildren<TextMesh>().text = gdRt.me.name;
		Camera.main.transform.parent = localPlayer.transform;
		
		//Send our position to the server
		IList<Gamedonia.Rt.Entities.UserVariable> vars = new List<Gamedonia.Rt.Entities.UserVariable>();
		vars.Add(new Gamedonia.Rt.Entities.UserVariable("model",numModel));
		vars.Add(new Gamedonia.Rt.Entities.UserVariable("mat",numMaterial));
		
		SetUserVariablesOperation suvop = new SetUserVariablesOperation(vars,GamedoniaRT.SharedInstance().me);

		gdRt.SendOp(suvop);
	}

	
	private void SpawnRemotePlayer(long userId, int numModel, int numMaterial, Vector3 pos, Quaternion rot) {

		if (gdRemotePlayers.ContainsKey(userId) && gdRemotePlayers[userId] != null) {
			Destroy(gdRemotePlayers[userId]);
			gdRemotePlayers.Remove(userId);
		}
		

		GameObject remotePlayer = GameObject.Instantiate(playerModels[numModel]) as GameObject;
		remotePlayer.AddComponent<SimpleRemoteInterpolation>();
		remotePlayer.GetComponent<SimpleRemoteInterpolation>().SetTransform(pos, rot, false);
		

		remotePlayer.GetComponentInChildren<TextMesh> ().text = gdRt.usersManager.GetUserById (userId).name;
		remotePlayer.GetComponentInChildren<Renderer>().material = playerMaterials[numMaterial];
		

		gdRemotePlayers.Add(userId, remotePlayer);
	}

	
	private void RemoveLocalPlayer() {

		Destroy (localPlayer);

	}

	private void RemoveRemotePlayer(Gamedonia.Rt.Entities.User user) {
		if (user == gdRt.me) return;
		
		if (gdRemotePlayers.ContainsKey(user.userId)) {
			Destroy(gdRemotePlayers[user.userId]);
			gdRemotePlayers.Remove(user.userId);
		}
	}


	public void OnForwardDown() {
		
		localPlayerController.OnForwardDown ();
		
	}
	
	public void OnForwardUp() {
		
		localPlayerController.OnForwardUp ();
		
	}

	public void OnBackwardDown() {
		
		localPlayerController.OnBackwardDown ();
		
	}
	
	public void OnBackwardUp() {
		
		localPlayerController.OnBackwardUp ();
		
	}

	public void OnTurnRightDown() {
		
		localPlayerController.OnTurnRightDown ();
		
	}
	
	public void OnTurnRightUp() {
		
		localPlayerController.OnTurnRightUp ();
		
	}

	public void OnTurnLeftDown() {
		
		localPlayerController.OnTurnLeftDown ();
		
	}
	
	public void OnTurnLeftUp() {
		
		localPlayerController.OnTurnLeftUp ();
		
	}

	public void SendPublicMessage(string text) {


		SendPublicMessageOperation spop = new SendPublicMessageOperation (gdRt.roomsManager.GetLastJoinedRoom ().roomId, text);
		gdRt.SendOp (spop);


	}

	public void SetPlayerControllerStatus(bool state) {

		localPlayerController.enabled = state;
	}


	private void reset() {

		GamedoniaRT.SharedInstance ().eventsDispatcher.RemoveAllEventListeners ();		
		enableLoginUI(true);
	}
	
	private void enableLoginUI(bool enable) {

	}



}