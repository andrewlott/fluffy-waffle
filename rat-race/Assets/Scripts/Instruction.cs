using UnityEngine;
using System.Collections;

public class InstructionSet {
	public Direction direction;
	public int actorId;
}

public class Instruction : MonoBehaviour {
	[SerializeField]
	SpriteRenderer actorRenderer;
	[SerializeField]
	SpriteRenderer actorBackgroundRenderer;
	[SerializeField]
	GameObject[] directionContainers;

	public InstructionSet instructionSet;
	// Use this for initialization
	void Start () {
	
	}

	public void SetupWithActorAndDirection (int _actorId, Direction _direction) {
		InstructionSet _instructionSet = new InstructionSet {
			direction = _direction, 
			actorId = _actorId
		};

		this.SetupWithInstructionSet (_instructionSet);
	}

	public void SetupWithInstructionSet (InstructionSet _instructionSet) {
		this.instructionSet = _instructionSet;
		this.actorRenderer.sprite = BoardManager.Instance().SpriteForActorId (this.instructionSet.actorId);
		this.actorBackgroundRenderer.sprite = BoardManager.Instance().BackgroundSpriteForActorId(this.instructionSet.actorId);
		Colorable colorable = BoardManager.Instance().ColorableForActorId(this.instructionSet.actorId);
		if (colorable != null && this.actorBackgroundRenderer != null && this.actorBackgroundRenderer.sprite != null) {
			this.actorBackgroundRenderer.color = colorable.GetSelectedColor();
		}

		for (int i = 0; i < this.directionContainers.Length; i++) {
			this.directionContainers [i].SetActive (false);
		}
		switch (this.instructionSet.direction) {
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
