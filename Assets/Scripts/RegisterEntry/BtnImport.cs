﻿using UnityEngine;
using System.Collections;

public class BtnImport : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void OnClick(){
		transform.root.FindChild("Lineup").GetComponent<MyLineup>().Init();
	}
}
