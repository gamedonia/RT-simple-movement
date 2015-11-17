using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using Gamedonia.Rt;
using Gamedonia.Rt.Entities;
using Gamedonia.Rt.Operations;
using Gamedonia.Rt.Events;


public class MatchMakingUI : MonoBehaviour {


	public static string selectedRoom;
	public static bool spectator;

	public InputField queryRoomsInput;
	public InputField roomsLimit;
	public Button searchRoomsBtn;
	public Text errorText;

	private GamedoniaRT gdRt;

	void Start() {

		reset ();
		gdRt = GamedoniaRT.SharedInstance ();

	}

	public void OnSearchRoomsClick() {

		Debug.Log ("Searching rooms");

	}

	private void reset() {
		
		GamedoniaRT.SharedInstance ().eventsDispatcher.RemoveAllEventListeners ();		
		enableLoginUI(true);
	}
	
	private void enableLoginUI(bool enable) {
		searchRoomsBtn.interactable = enable;
		errorText.text = "";
	}

}
