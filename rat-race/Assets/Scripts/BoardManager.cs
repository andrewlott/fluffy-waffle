using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction {
	Up = 0,
	Down = 1,
	Left = 2,
	Right = 3,
}

public class BoardManager : MonoBehaviour {
	[SerializeField]
	int boardRadius = 2;
	[SerializeField]
	Slot[] slotPrefabs;
	[SerializeField]
	Instruction instructionPrefab;
	[SerializeField]
	Tile [] blocks;
	[SerializeField]
	Tile mouse;
	[SerializeField]
	public Tile cheese;
	[SerializeField]
	public Target target;
	[SerializeField]
	GameObject instructionArea;

	private float unit = 0.32f;
	private List<Slot> slots;

	private static BoardManager _instance;
	public static BoardManager Instance() {
		return _instance;
	}

	// Use this for initialization
	void Start () {
		_instance = this;
		// hackyy

		this.MakeBoard ();
		this.MakeTiles ();
		this.MakeInstructions ();
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
				slot.index = this.slots.Count;
				slot.x = j + boardRadius;
				slot.y = i + boardRadius;
				this.slots.Add (slot);
				total++;
			}

			totalStart++;
		}
	}

	public Slot RandomUnoccupiedSlot() {
		Slot slot;
		do {
			int randomIndex = Random.Range (0, this.slots.Count);
			slot = this.SlotForIndex (randomIndex);
		} while (slot.tile != null);
		return slot;
	}

	public Slot SlotForCoordinate(int x, int y) {
		return SlotForIndex (x + (boardRadius * 2 * y));
	}

	public Slot SlotForIndex(int index) {
		return this.slots [index];
	}

	private void MakeTiles() {
		this.JoinSlotAndTile (this.RandomUnoccupiedSlot (), this.mouse, false);
		this.mouse.gameObject.transform.position = this.mouse.slot.gameObject.transform.position;
	}

	private void JoinSlotAndTile(Slot slot, Tile tile, bool move) {
		slot.SetTile (tile);
		tile.SetSlot (slot);
		if (move) {
			// animate it over
		}
	}

	private void JoinSlotAndTarget(Slot slot, Target target) {
		slot.SetTarget (target);
		target.gameObject.transform.position = slot.gameObject.transform.position;
	}

	private void UnjoinSlotAndTile(Slot slot, Tile tile) {
		slot.UnsetTile ();
		tile.UnsetSlot ();
	}

	private void MakeInstructions() {
		int x = this.mouse.slot.x;
		int y = this.mouse.slot.y;

		List<Direction> directions = new List<Direction> ();
		int randomAmount = Random.Range (boardRadius, 2 * boardRadius);
		while (directions.Count < randomAmount) {
			Direction randomDirection = (Direction)Random.Range (0, 4);
			switch (randomDirection) {
			case Direction.Up:
				if (y == boardRadius * 2 - 1) {
					continue;
				}
				y++;
				break;
			case Direction.Down:
				if (y == 0) {
					continue;
				}
				y--;
				break;
			case Direction.Left:
				if (x == 0) {
					continue;
				}
				x--;
				break;
			case Direction.Right:
				if (x == boardRadius * 2 - 1) {
					continue;
				}
				x++;
				break;
			default:
				break;
			}

			Instruction instruction = Instantiate (this.instructionPrefab);
			instruction.SetupWithActorAndDirection (this.mouse.gameObject, randomDirection);
			instruction.gameObject.transform.SetParent (this.instructionArea.gameObject.transform, false);

			directions.Add (randomDirection); // prob dont need
		}
		this.JoinSlotAndTarget (this.SlotForCoordinate (x, y), this.target);
	}

	private int indexDeltaForDirection(Direction direction) {
		switch (direction) {
			case Direction.Up:
				return -boardRadius;
			case Direction.Down:
				return boardRadius;
			case Direction.Left:
				return -1;
			case Direction.Right:
				return 1;
			default:
				return 0;
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
