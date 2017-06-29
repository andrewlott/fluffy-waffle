using UnityEngine;
using System.Collections;

public class Instruction : MonoBehaviour {

	[SerializeField]
	SpriteRenderer actorRenderer;
	[SerializeField]
	GameObject[] directionContainers;

	public Direction direction;
	public Tile actor;

	// Use this for initialization
	void Start () {
	
	}

	public void SetupWithActorAndDirection (Tile _actor, Direction _direction) {
		this.direction = _direction;
		this.actor = _actor;

		this.actorRenderer.sprite = _actor.sprite;

		for (int i = 0; i < this.directionContainers.Length; i++) {
			this.directionContainers [i].SetActive (false);
		}
		switch (this.direction) {
		case Direction.Up:
			this.directionContainers [0].SetActive (true);
			break;
		case Direction.Down:
			this.directionContainers [1].SetActive (true);
			break;
		case Direction.Left:
			this.directionContainers [2].SetActive (true);
			break;
		case Direction.Right:
			this.directionContainers [3].SetActive (true);
			break;
		default:
			break;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
