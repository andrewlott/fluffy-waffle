using UnityEngine;
using System.Collections;

public class Target : MonoBehaviour {

	public GameObject yes;
	public GameObject no;

	// Use this for initialization
	void Start () {
		this.yes.SetActive (false);
		this.no.SetActive (false);
	}

	public void ShowView(bool win) {
		if (win) {
			this.yes.SetActive (true);
		} else {
			this.no.SetActive (true);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
