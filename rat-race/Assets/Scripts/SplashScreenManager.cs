using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SplashScreenManager : MonoBehaviour {

	void Start () {
	}

	void Update () {
	}

	public void LoadGame() {
		Application.LoadLevel("Main");
	}
}
