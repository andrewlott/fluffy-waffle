using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {

	public Slot slot;

	public static float animationDuration = 0.125f;
	public Sprite sprite;
	public Sprite backgroundSprite;

	public Colorable colorable;
	private Vector3 startPosition;
	private float startTime;

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
		if (!BoardManager.Instance ().hasCheeseBeenPlaced) {
			return;
		}
		if (this.slot == null) {
			return;
		}
		if (this.slot.gameObject.transform.position != this.gameObject.transform.position) {
			if (this.startTime == 0) {
				this.startPosition = this.gameObject.transform.position;
				this.startTime = Time.time;
			}
			this.gameObject.transform.position = Vector3.Lerp (this.startPosition, this.slot.gameObject.transform.position, (Time.time - this.startTime) / animationDuration);
		} else if (this.startTime != 0) {
			this.startTime = 0;
			this.startPosition = Vector3.zero;
			Actor actor = this.gameObject.GetComponent<Actor>();
			if (actor.myActorId == Actor.mouseId) {
				Animator animator = this.gameObject.GetComponent<Animator>();
				animator.SetTrigger("MouseIdle"); // can alleviate extra frame when moving a lot by delegating all instructions to actors (pass them index to determine delay and instruction)
			}
		}
	}
}
