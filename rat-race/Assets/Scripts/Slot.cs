using UnityEngine;
using System.Collections;

public class Slot : MonoBehaviour {

	public int x, y, index;

	public Tile tile;

	public void SetTile(Tile _tile) {
		this.tile = _tile;
	}

	public void UnsetTile() {
		this.tile = null;
	}

	void OnMouseUp() {
		if (BoardManager.Instance ().hasCheeseBeenPlaced) {
			return;
		}

		BoardManager.Instance ().CheesePlaced (this);
	}
}
