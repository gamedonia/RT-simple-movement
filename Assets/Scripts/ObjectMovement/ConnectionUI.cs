using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using Gamedonia.Rt;
using Gamedonia.Rt.Events;
using Gamedonia.Rt.Operations;
using System;
using System.IO;
using LitJson_Gamedonia;


public class ConnectionUI : MonoBehaviour {

	public string Host = "192.168.1.14";
	
	//----------------------------------------------------------
	// UI elements
	//----------------------------------------------------------

	public InputField nameInput;
	public InputField ipAddressInput;
	public InputField apiKeyInput;
	public InputField secretInput;
	public Button loginButton;
	public Text errorText;

	//----------------------------------------------------------
	// Private properties
	//----------------------------------------------------------


	//----------------------------------------------------------
	// Unity calback methods
	//----------------------------------------------------------

	void Awake() {

	}

	void Start() {
		#if UNITY_WEBPLAYER
		if (!Security.PrefetchSocketPolicy(Host, TcpPort, 500)) {
			Debug.LogError("Security Exception. Policy file loading failed!");
		}
		#endif



		/*
		// Initialize UI
		errorText.text = "";

		string path = Application.dataPath;
		if (Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.OSXPlayer) {
			path += "/../../";
		}
		else if (Application.platform == RuntimePlatform.WindowsPlayer) {
			path += "/../";
		}
		
		
		Debug.Log (path);


		string config = File.ReadAllText (path + "config.json");
		if (!"".Equals (config)) {
			Dictionary<string,string> configMap = JsonMapper.ToObject<Dictionary<string,string>> (config);
			apiKeyInput.text = configMap["apiKey"];
			secretInput.text = configMap["secret"];
		}


		//Debug.Log (Application.absoluteURL);
		//Debug.Log (Application.persistentDataPath);
		//string readText = File.ReadAllText(path);
		*/
		//Load settings from file
		ipAddressInput.text = GamedoniaRT.SharedInstance ().server;
		apiKeyInput.text = GamedoniaRT.SharedInstance ().apiKey;
		secretInput.text = GamedoniaRT.SharedInstance ().secret;


		reset ();
		GamedoniaRT.SharedInstance ().eventsDispatcher.AddEventListener (GamedoniaEvents.ON_CONNECTION_SUCCESS, OnConnectionSuccess);
		GamedoniaRT.SharedInstance ().eventsDispatcher.AddEventListener (GamedoniaEvents.ON_CONNECTION_ERROR, OnConnectionError);
		GamedoniaRT.SharedInstance ().eventsDispatcher.AddEventListener (GamedoniaEvents.ON_LOGIN_SUCCESS, OnLoginSuccess);
		GamedoniaRT.SharedInstance ().eventsDispatcher.AddEventListener (GamedoniaEvents.ON_CREATE_ROOM_SUCCESS, OnCreateRoomSuccess);
		GamedoniaRT.SharedInstance ().eventsDispatcher.AddEventListener (GamedoniaEvents.ON_JOIN_ROOM_SUCCESS, OnJoinRoomSuccess);




	}
	
	void Update() {

	}

	//----------------------------------------------------------
	// Public interface methods for UI
	//----------------------------------------------------------
	
	public void OnLoginButtonClick() {
		enableLoginUI(false);

		if (!"".Equals(apiKeyInput.text) && !"".Equals(secretInput.text)) GamedoniaRT.SharedInstance ().Init (apiKeyInput.text,secretInput.text);

		GamedoniaRT.SharedInstance ().Connect (ipAddressInput.text,9996);

	}

	//----------------------------------------------------------
	// Private helper methods
	//----------------------------------------------------------
	
	private void enableLoginUI(bool enable) {
		nameInput.interactable = enable;
		loginButton.interactable = enable;
		errorText.text = "";
	}
	
	private void reset() {
		GamedoniaRT.SharedInstance ().eventsDispatcher.RemoveAllEventListeners ();		
		enableLoginUI(true);
	}

	void OnConnectionSuccess(Gamedonia.Rt.Events.Event evt) {

		Debug.Log ("Connected!!");

		LoginOperation lop = new LoginOperation();
		System.Random rnd = new System.Random();
		int val = rnd.Next (1,9999);
		
		lop.silent = Convert.ToString (val);
		
		lop.name = nameInput.text;
		GamedoniaRT.SharedInstance().SendOp(lop);



	}

	void OnConnectionError(Gamedonia.Rt.Events.Event evt) {

		reset();	
		errorText.text = evt.data.GetString("error");		
		
	}


	void OnLoginSuccess(Gamedonia.Rt.Events.Event evt) {
		
		Debug.Log ("Logged in!!");

		//Creamos la room
		long gameRoomId = GamedoniaRT.SharedInstance().roomsManager.GetRoomIdByName("DefaultRoom");

		if (gameRoomId != -1) {
			//Room already exists
			//Join Room
			Application.LoadLevel ("ChooseGameGD");

			//JoinRoomOperation jrop = new JoinRoomOperation("GameRoom");			
			//GamedoniaRT.SharedInstance().SendOp(jrop);

		} else {
			//We need to create the room

			Gamedonia.Rt.Entities.RoomSettings rsettings = new Gamedonia.Rt.Entities.RoomSettings();
			rsettings.name = "DefaultRoom";
			rsettings.isGame = true;
			rsettings.maxUsers = 8;
			rsettings.maxSpectators = 16;

			CreateRoomOperation crop = new CreateRoomOperation(rsettings);
			
			GamedoniaRT.SharedInstance().SendOp(crop);

		}


	}
	

	void OnCreateRoomSuccess(Gamedonia.Rt.Events.Event evt) {
		
		Debug.Log ("Room Created!!");
		Application.LoadLevel ("ChooseGameGD");

	}
	

	void OnJoinRoomSuccess(Gamedonia.Rt.Events.Event evt) {

		Debug.Log ("Room Joined");
		//reset();
		
		// Go to main game scene
		Application.LoadLevel("GameGD");

	}
	
}