﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using UnityEngine.SceneManagement;

public class CameraPermissionChecker : MonoBehaviour {
	[SerializeField] private GameObject backGround_Panel;

	[SerializeField] SupCanvasController ccontroller;
	

	private bool permissionAsked = false;




	void Start () {

		permissionAsked = SaveManager.Instance.session_state.isPermissionRequested;
		var wasIntroShown = SaveManager.Instance.session_state.wasIntroShown;
		ccontroller.setIntroVisible(!wasIntroShown);

		if (permissionAsked && wasIntroShown){
			verifyPermission();	
		} 
	}

	public void verifyPermission(){
		iOSCameraPermission.VerifyPermission(gameObject.name, "SampleCallback");
	}

	private void loadScene(){
		SceneManager.LoadScene("ArealMainScene",LoadSceneMode.Single);
	}
	    private void SampleCallback(string permissionWasGranted)
    {       
        if (permissionWasGranted == "true" )
        {
			loadScene();
        }
        else
        {
			ccontroller.showWarningText();
        }

		SaveManager.Instance.SavePermissionRequest();
    }
}


