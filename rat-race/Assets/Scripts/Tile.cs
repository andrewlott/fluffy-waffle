using UnityEngine;
using System.Collections;

public class Tile : MonoBehaviour {

	public Slot slot;

	public void SetSlot(Slot _slot) {
		this.slot = _slot;
	}

	public void UnsetSlot() {
		this.slot = null;
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
