using UnityEngine;
using System.Collections;

public class Slot : MonoBehaviour {

	public int x, y, index;

	public Tile tile;
	public Target target;

	public void SetTile(Tile _tile) {
		this.tile = _tile;
	}

	public void UnsetTile() {
		this.tile = null;
	}

	public void SetTarget(Target _target) {
		this.target = _target;
	}

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnMouseUp() {
		BoardManager.Instance ().target.ShowView (this.target != null);
		Tile cheese = Instantiate (BoardManager.Instance().cheese);
		cheese.gameObject.transform.position = this.gameObject.transform.position;

		this.StartCoroutine (this.RestartCoroutine());
	}

	IEnumerator RestartCoroutine() {
		yield return new WaitForSeconds (2.0f);
		Application.LoadLevel("Main");
	}
}
