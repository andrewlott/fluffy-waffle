using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Colorable : MonoBehaviour {
	[SerializeField]
	public SpriteRenderer spriteRenderer;
	[SerializeField]
	public List<Color> colors;

	private Color selectedColor = Color.white;
	private bool hasSetColor = false;

	void Start () {
		if (this.spriteRenderer != null) {
			this.spriteRenderer.color = this.GetSelectedColor();
		}
	}

	public Color GetSelectedColor() {
		if (!this.hasSetColor) {
			if (this.colors != null && this.colors.Count > 0) {
				this.colors.Shuffle();
				this.selectedColor = this.colors[0];
			}

			this.hasSetColor = true;
		}

		return this.selectedColor;
	}
}
