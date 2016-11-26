using UnityEngine;
using System.Collections;

public class Slot : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseUp() {
		Debug.LogFormat ("pressed! x:{0}, y:{1}", this.gameObject.transform.position.x, this.gameObject.transform.position.y);
	}
}
