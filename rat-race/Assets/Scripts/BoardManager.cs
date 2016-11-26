using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour {
	[SerializeField]
	Slot[] slotPrefabs;

	private int boardRadius = 3;
	private float unit = 0.32f;
	private List<Slot> slots;

	// Use this for initialization
	void Start () {
		this.MakeBoard ();
	}

	private void MakeBoard() {
		this.slots = new List<Slot> ();
		this.slots.Clear ();
		int total = 0;
		int totalStart = 0;
		float offset = unit / 2.0f;			
		for (int i = -boardRadius; i < boardRadius; i++) {
			total = totalStart;
			for (int j = -boardRadius; j < boardRadius; j++) {
				Slot slot = Instantiate(slotPrefabs[total % slotPrefabs.Length]);
				slot.gameObject.transform.position = new Vector3 (offset + j * unit, offset + i * unit, 0);
				this.slots.Add (slot);
				total++;
			}

			totalStart++;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
