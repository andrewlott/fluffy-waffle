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
	private Vector3 targetPosition;
	private float startTime;

	public void SetSlot(Slot _slot) {
		this.slot = _slot;
	}

	public void UnsetSlot() {
		this.slot = null;
	}

	public void MoveInDirection(Direction direction, int amount = 1) {
		int vert = 0;
		int horiz = 0;
		switch (direction) {
		case Direction.Up:
			vert = amount;
			break;
		case Direction.Down:
			vert = -amount;
			break;
		case Direction.Left:
			horiz = -amount;
			break;
		case Direction.Right:
			horiz = amount;
			break;
		}

		this.SetTarget(this.targetPosition + new Vector3(horiz * BoardManager.unit, vert * BoardManager.unit, 0));
	}

	public void SetTarget(Vector3 target) {
		this.targetPosition = target;
	}

	// Update is called once per frame
	void Update () {
		if (!BoardManager.Instance ().hasCheeseBeenPlaced) {
			this.SetTarget(this.gameObject.transform.localPosition);
			return;
		}

		if (this.targetPosition != this.gameObject.transform.localPosition) {
			if (this.startTime == 0) {
				this.startPosition = this.gameObject.transform.localPosition;
				this.startTime = Time.time;
			}
			this.gameObject.transform.localPosition = Vector3.Lerp(this.startPosition, this.targetPosition, (Time.time - this.startTime) / animationDuration);
		} else if (this.startTime != 0) {
			this.startTime = 0;
			this.startPosition = Vector3.zero;
			Actor actor = this.gameObject.GetComponent<Actor>();
			if (actor.myActorId == Actor.mouseId) {
				Animator animator = this.gameObject.GetComponent<Animator>();
				animator.SetTrigger("MouseIdle"); // can alleviate extra frame when moving a lot by delegating all instructions to actors (pass them index to determine delay and instruction)
			}
		} else if (this.startTime == 0) {
			this.targetPosition = this.gameObject.transform.localPosition;
		}

//		if (this.slot == null) {
//			return;
//		}
//		if (this.slot.gameObject.transform.localPosition != this.gameObject.transform.localPosition) {
//			if (this.startTime == 0) {
//				this.startPosition = this.gameObject.transform.localPosition;
//				this.startTime = Time.time;
//			}
//			this.gameObject.transform.localPosition = Vector3.Lerp (this.startPosition, this.slot.gameObject.transform.localPosition, (Time.time - this.startTime) / animationDuration);
//		} else if (this.startTime != 0) {
//			this.startTime = 0;
//			this.startPosition = Vector3.zero;
//			Actor actor = this.gameObject.GetComponent<Actor>();
//			if (actor.myActorId == Actor.mouseId) {
//				Animator animator = this.gameObject.GetComponent<Animator>();
//				animator.SetTrigger("MouseIdle"); // can alleviate extra frame when moving a lot by delegating all instructions to actors (pass them index to determine delay and instruction)
//			}
//		}
	}
}
