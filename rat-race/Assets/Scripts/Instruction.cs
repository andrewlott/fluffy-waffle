using UnityEngine;
using System.Collections;

public class Instruction : MonoBehaviour {

	[SerializeField]
	SpriteRenderer actorRenderer;
	[SerializeField]
	GameObject[] directionContainers;

	// Use this for initialization
	void Start () {
	
	}

	public void SetupWithActorAndDirection (GameObject actor, Direction direction) {
		this.actorRenderer.sprite = actor.GetComponentInChildren<SpriteRenderer> ().sprite;

		for (int i = 0; i < directionContainers.Length; i++) {
			directionContainers [i].SetActive (false);
		}
		switch (direction) {
		case Direction.Up:
			directionContainers [0].SetActive (true);
			break;
		case Direction.Down:
			directionContainers [1].SetActive (true);
			break;
		case Direction.Left:
			directionContainers [2].SetActive (true);
			break;
		case Direction.Right:
			directionContainers [3].SetActive (true);
			break;
		default:
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
