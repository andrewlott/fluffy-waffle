using UnityEngine;
using System.Collections;

public class BoardManager : MonoBehaviour {
	[SerializeField]
	GameObject[] slots;

	private int boardRadius = 3;
	private float unit = 0.32f;

	// Use this for initialization
	void Start () {
		this.MakeBoard ();
	}

	private void MakeBoard() {
		int total = 0;
		int totalStart = 0;
		for (int i = -boardRadius; i <= boardRadius; i++) {
			if (i == 0) {
				continue;
			}

			total = totalStart;
			for (int j = -boardRadius; j <= boardRadius; j++) {
				if (j == 0) {
					continue;
				}
				GameObject slot = Instantiate(slots[total++ % slots.Length]);
				slot.transform.position = new Vector3 (j * unit, i * unit, 0);
			}

			totalStart++;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
