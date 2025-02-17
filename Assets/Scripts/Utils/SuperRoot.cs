﻿using UnityEngine;
using System.Collections;

public class SuperRoot : MonoBehaviour {

	public GameObject mPopup;
		
	protected void Start () {
		mPopup = null;
		UtilMgr.SetRoot(transform);
		//transform.FindChild ("Camera").transform.localPosition = new Vector3(0f, UtilMgr.GetScaledPositionY(), -2000f);    //not use !!
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}
	
	protected void Awake(){		
		if(GetComponent<AudioSource>() == null){
			gameObject.AddComponent<AudioSource>();
		}
		Debug.Log("frameRate is "+Application.targetFrameRate);		
		Debug.Log("vSyncCount is "+QualitySettings.vSyncCount);		
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = 20;
	}
	
	protected void Update () {
		if (Input.GetKeyDown (KeyCode.Escape)) {
//			if(Application.loadedLevelName.Equals("SceneMain")){
//				if(!transform.FindChild("TF_Betting").gameObject.activeSelf){
//					OnBackPressed ();
//				}
//			}else{
				OnBackPressed ();
//			}
		}
		
	}
	
	void OnApplicationFocus(bool focus){
//		UtilMgr.OnFocus = focus;
//		Debug.Log("Application focus : "+focus);
	}
	
	void OnApplicationPause(bool pause){
		UtilMgr.OnPause = pause;
//		Debug.Log("Application pause : "+pause);
		if(pause){
			UtilMgr.PauseTime = System.DateTime.Now;
		} else{
			System.TimeSpan timeDiff = System.DateTime.Now - UtilMgr.PauseTime;
//			Debug.Log("hours : "+timeDiff.Hours+", minutes : "+timeDiff.Minutes+", secs : "+timeDiff.Seconds);

			if(timeDiff.Minutes > 29){
				AutoFade.LoadLevel("Login"); return;
			}

			if(UtilMgr.GetLastBackState() == UtilMgr.STATE.LiveBingo
			   && UserMgr.eventJoined != null){
				NetMgr.JoinGame();
				transform.FindChild("LiveBingo").GetComponent<LiveBingo>().ReloadAll();
			}
		}
//			UtilMgr.DismissLoading();
	}
	
	public void OnBackPressed()
	{
		if(mPopup != null){
			mPopup.SetActive(false);
			mPopup = null;
		} else if (DialogueMgr.IsShown) {
			DialogueMgr.Instance.BtnCancelClicked();
		} else if(IsAnimating){
			return;
		} else {
			UtilMgr.OnBackPressed ();
		}
	}

	bool isAnimating;

	public bool IsAnimating {
		get {
			return isAnimating;
		}
		set {
			isAnimating = value;
		}
	}
}
