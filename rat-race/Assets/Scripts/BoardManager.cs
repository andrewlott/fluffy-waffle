using UnityEngine;
using UnityEngine.UI;
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
	int boardRadius;
	[SerializeField]
	Slot[] slotPrefabs;
	[SerializeField]
	GameObject instructionPrefab;
	[SerializeField]
	Tile [] blocks;
	[SerializeField]
	GameObject mousePrefab;
	[SerializeField]
	public Tile cheesePrefab;
	[SerializeField]
	public Target target;
	[SerializeField]
	GameObject instructionArea;
	[SerializeField]
	Sprite noCheeseSprite;
	[SerializeField]
	List<GameObject> actorPrefabs;
	[SerializeField]
	int initialTimeInSeconds = 60;
	[SerializeField]
	int bonusTimeInSeconds = 5;
	[SerializeField]
	int initialLives = 3;
	[SerializeField]
	Sprite lifeLoseSprite;

	[SerializeField]
	Text levelText;
	[SerializeField]
	Text timerText;
	[SerializeField]
	List<Image> mouseLivesImages;

	public bool hasCheeseBeenPlaced = false;
	private int level = 1;
	private float startTime;
	private int bonusTime;
	private int lives;

	private float unit = 0.32f; // make this 28 to do cool tile background thing
	private List<Slot> slots = new List<Slot>();
	private List<Instruction> instructions = new List<Instruction>();
	private Tile mouse;
	private Tile cheese;
	private List<Actor> actors = new List<Actor>();

	private static BoardManager _instance;
	public static BoardManager Instance() {
		return _instance;
	}

	// Use this for initialization
	void Start () {
		_instance = this;
		// hackyy
		this.Initialize();
		this.Restart();
	}

	void Update() {
		this.UpdateTimerText();
	}

	private void Initialize() {
		this.startTime = Time.time;
		this.lives = this.initialLives;
	}

	private void Restart() {
		Debug.Log ("Restarting");
		this.UpdateLevelText();

		this.MakeBoard ();
		this.MakeTiles ();
		this.MakeInstructions ();
	}

	private int BoardRadiusForLevel() {
		return 1 + ((int)Mathf.Log (this.level));
	}

	private void MakeBoard() {
		Debug.LogFormat ("Level {0}", this.level);
		foreach (Slot s in this.slots) {
			GameObject.Destroy(s.gameObject);
		}
		this.slots.Clear ();

		this.boardRadius = this.BoardRadiusForLevel ();

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
		if (index < 0 || index >= this.slots.Count) {
			return null;
		}
		return this.slots [index];
	}

	private void MakeTiles() {
		foreach (Actor a in this.actors) {
			GameObject.Destroy (a.gameObject);
		}
		this.actors.Clear ();

		if (this.cheese != null) {
			GameObject.Destroy (this.cheese.gameObject);
		}
		this.hasCheeseBeenPlaced = false;
		if (this.mouse != null) {
			GameObject.Destroy (this.mouse.gameObject);
		}
		GameObject g = Instantiate (this.mousePrefab) as GameObject;
		this.mouse = g.GetComponent<Tile> ();

		this.JoinSlotAndTile (this.RandomUnoccupiedSlot (), this.mouse);
		this.mouse.gameObject.transform.position = this.mouse.slot.gameObject.transform.position;

		Actor mouseActor = this.mouse.GetComponent<Actor> ();
		this.actors.Add (mouseActor);
		List<Actor> blockActors = this.GenerateBlocks ();
		this.actors.AddRange (blockActors);
	}

	public Slot SlotInDirectionFromSlot(Slot slot, Direction direction) {
		if (!this.IsValidDirectionFromSlot(slot, direction)) {
			return null;
		}
		return this.SlotForIndex (slot.index + this.IndexDeltaForDirection (direction));
	}

	public bool IsValidDirectionFromSlot(Slot slot, Direction direction) {
		switch (direction) {
		case Direction.Down:
			return slot.index + this.IndexDeltaForDirection(direction) >= 0;
		case Direction.Left:
			{
				int originalRow = this.RowForSlotIndex(slot.index);
				return originalRow == this.RowForSlotIndex(slot.index + this.IndexDeltaForDirection(direction));
			}
		case Direction.Right:
			{
				int originalRow = this.RowForSlotIndex(slot.index);
				return originalRow == this.RowForSlotIndex(slot.index + this.IndexDeltaForDirection(direction));
			}
		case Direction.Up:
			return slot.index + this.IndexDeltaForDirection(direction) < this.slots.Count;
		}
		return false;
	}

	public int RowForSlotIndex(int index) {
		return index / (boardRadius * 2);
	}

	public int ColumnForSlotIndex(int index) {
		return index % (boardRadius * 2);
	}

	public Tile TileInDirectionFromTile(Tile tile, Direction direction) {
		Slot nextSlot = this.SlotInDirectionFromSlot (tile.slot, direction);
		if (nextSlot == null) {
			return null;
		}

		return nextSlot.tile;
	}

	private void JoinSlotAndTile(Slot slot, Tile tile) {
		slot.SetTile (tile);
		tile.SetSlot (slot);
	}
		
	private void UnjoinSlotAndTile(Slot slot, Tile tile) {
		slot.UnsetTile ();
		tile.UnsetSlot ();
	}
//
//	private List<Actor> ActorsWithId(int actorId) {
//		List<Actor> ret = new List<Actor> ();
//		foreach (Slot s in this.slots) {
//			if (s.tile != null) {
//				Actor actor = s.tile.gameObject.GetComponent<Actor> ();
//				if (actor != null) {
//					ret.Add (actor);
//				}
//			}
//		}
//
//		return ret;
//	}

	private Vector2 PositionInDirection(int i, int j, Direction direction) {
		switch (direction) {
			case Direction.Down:
				i -= 1;
				break;
			case Direction.Left:
				j -= 1;
				break;
			case Direction.Right:
				j += 1;
				break;
			case Direction.Up:
				i += 1;
				break;
		}

		return new Vector2 (i, j);
	}

	private int ValueInDirection(int [,] boardState, int i, int j, Direction direction) {
		switch (direction) {
			case Direction.Down:
				i -= 1;
				break;
			case Direction.Left:
				j -= 1;
				break;
			case Direction.Right:
				j += 1;
				break;
			case Direction.Up:
				i += 1;
				break;
		}

		if (i < 0 || i > boardState.GetLength (0) - 1 || j < 0 || j > boardState.GetLength (1) - 1) {
			return -1;
		}

		return boardState [i, j];
	}

	private void ApplyMove(int [,] boardState, int actorId, Direction direction, List<int> blacklist = null) {
		if (blacklist == null) {
			blacklist = new List<int> ();
		}
		blacklist.Add (actorId);
		List<int> influencedActorIds = new List<int> ();
		List<Vector2> positions = new List<Vector2> ();
		for (int i = boardState.GetLength (0) - 1; i >= 0; i--) {
			for (int j = 0; j < boardState.GetLength (1); j++) {
				int val = boardState [i, j];
				if (val != actorId) {
					continue;
				}

				int valInDirection = this.ValueInDirection (boardState, i, j, direction);
				if (valInDirection >= 0) {
					positions.Add(this.PositionInDirection(i, j, direction));
				}

				if (valInDirection > 0 && !influencedActorIds.Contains (valInDirection) && !blacklist.Contains(valInDirection)) {
					influencedActorIds.Add (valInDirection);
				}
				boardState [i, j] = 0;
			}
		}

		foreach (int influencedActorId in influencedActorIds) {
			this.ApplyMove (boardState, influencedActorId, direction, blacklist);
		}

		foreach (Vector2 position in positions) {
			boardState [(int)position.x, (int)position.y] = actorId;
		}
	}

	private bool IsLegalMove(int [,] boardState, int actorId, Direction direction, List<int> blacklist = null) {
		if (blacklist == null) {
			blacklist = new List<int> ();
		}
		blacklist.Add (actorId);
		bool ret = true;
		List<int> influencedActorIds = new List<int> ();
		for (int i = boardState.GetLength (0) - 1; i >= 0; i--) {
			for (int j = 0; j < boardState.GetLength (1); j++) {
				int val = boardState [i, j];
				if (val != actorId) {
					continue;
				}

				int valInDirection = this.ValueInDirection (boardState, i, j, direction);
				if (valInDirection < 0) {
					return false;
				}

				if (valInDirection > 0 && !influencedActorIds.Contains (valInDirection) && !blacklist.Contains(valInDirection)) {
					influencedActorIds.Add (valInDirection);
				}
			}
		}

		foreach (int influencedActorId in influencedActorIds) {
			ret = ret && this.IsLegalMove (boardState, influencedActorId, direction, blacklist);
		}

		return ret;
//		bool ret = true;
//		List<int> influencedActorIds = new List<int> ();
//		foreach (Actor actor in actors) {
//			Tile t = actor.gameObject.GetComponent<Tile> ();
//			Slot moveToSlot = this.SlotInDirectionFromSlot (t.slot, direction);
//			if (moveToSlot == null) {
//				return false;
//			}
//
//			Tile neighbor = this.TileInDirectionFromTile (t, direction);
//			if (neighbor != null) {
//				Actor neighborActor = neighbor.gameObject.GetComponent<Actor> ();
//				if (!influencedActorIds.Contains (neighborActor.myActorId)) {
//					influencedActorIds.Add (neighborActor.myActorId);
//				}
//			}
//		}
//
//		foreach (int influencedActorId in influencedActorIds) {
//			ret = ret && this.IsLegalMove (boardState, influencedActorId, direction);
//		}
//
//		return ret;
	}

	private List<int> ActorIds() {
		List<int> actorIds = new List<int> ();
		foreach (Actor actor in this.actors) {
			if (!actorIds.Contains (actor.myActorId)) {
				actorIds.Add (actor.myActorId);
			}
		}
		return actorIds;
	}

	private List<Actor> ActorsWithId(int actorId) {
		List<Actor> a = new List<Actor> ();
		foreach (Actor actor in this.actors) {
			if (actor.myActorId == actorId) {
				a.Add (actor);
			}
		}
		return a;
	}

	private List<InstructionSet> AllValidNextInstructions(int [,] boardState) {
		List<InstructionSet> allInstructions = new List<InstructionSet> ();
		Direction[] directions = new Direction[]{ Direction.Down, Direction.Left, Direction.Right, Direction.Up };
		// attempt each next instruction
		foreach (int actorId in this.ActorIds()) {
			foreach (Direction direction in directions) {
				if (this.IsLegalMove (boardState, actorId, direction)) {
					allInstructions.Add (new InstructionSet {
						actorId = actorId,
						direction = direction
					});
				}
			}
		}


		return allInstructions;
	}

	private InstructionSet RandomValidNextInstruction(int [,] boardstate) {
		List<InstructionSet> allValidNextInstructions = this.AllValidNextInstructions (boardstate);
		if (allValidNextInstructions == null || allValidNextInstructions.Count == 0) {
			return null;
		}

		allValidNextInstructions.Shuffle ();
		return allValidNextInstructions[0];
	}
		
	private void PrintBoardState(int [,] boardState) {
		string ss = "";
		for (int i = boardState.GetLength(0) - 1; i >= 0; i--) {
			for (int j = 0; j < boardState.GetLength(1); j++) {
				ss += " " + boardState[i,j] + " ";
			}
			ss += "\n";
		}
		Debug.Log(ss);
	}

	private int [,] GetBoardState() {
		int boardWidth = 2 * boardRadius;

		int[,] boardState = new int[boardWidth, boardWidth];
		foreach (Slot s in this.slots) {
			int sx = s.x;
			int sy = s.y;
			if (s.tile != null) {
				Actor a = s.tile.gameObject.GetComponent<Actor> ();
				boardState [sy, sx] = a.myActorId;
			}
		}

		return boardState;
	}

	private void MakeInstructions() {
		this.instructions.Clear ();
		foreach (Transform child in this.instructionArea.transform) {
			GameObject.Destroy(child.gameObject);
		}

		int x = this.mouse.slot.x;
		int y = this.mouse.slot.y;

		int randomAmount = Random.Range (level, 2 * level);

		int[,] boardState = this.GetBoardState ();
		Debug.Log (randomAmount);
		this.PrintBoardState (boardState);
		while (this.instructions.Count < randomAmount) {
			InstructionSet instructionSet = this.RandomValidNextInstruction (boardState);
			if (instructionSet == null) {
				this.instructions.RemoveAt (this.instructions.Count - 1);
				continue;
			}

			GameObject g = Instantiate (this.instructionPrefab) as GameObject;
			Instruction instruction = g.GetComponent<Instruction>();
			instruction.SetupWithInstructionSet (instructionSet);
			instruction.gameObject.transform.SetParent (this.instructionArea.gameObject.transform, false);

			this.instructions.Add (instruction);

			this.ApplyMove (boardState, instructionSet.actorId, instructionSet.direction);
			this.PrintBoardState (boardState);
		}
//			
//		int randomAmount = Random.Range (boardRadius, 2 * boardRadius);
//		while (this.instructions.Count < randomAmount) {
//			Direction randomDirection = (Direction)Random.Range (0, 4);
//			switch (randomDirection) {
//			case Direction.Up:
//				if (y == boardRadius * 2 - 1) {
//					continue;
//				}
//				y++;
//				break;
//			case Direction.Down:
//				if (y == 0) {
//					continue;
//				}
//				y--;
//				break;
//			case Direction.Left:
//				if (x == 0) {
//					continue;
//				}
//				x--;
//				break;
//			case Direction.Right:
//				if (x == boardRadius * 2 - 1) {
//					continue;
//				}
//				x++;
//				break;
//			default:
//				break;
//			}
//
//			Actor actor = this.mouse.GetComponent<Actor> ();
//			Instruction instruction = Instantiate (this.instructionPrefab);
//			instruction.SetupWithActorAndDirection (actor.myActorId, randomDirection);
//			instruction.gameObject.transform.SetParent (this.instructionArea.gameObject.transform, false);
//
//			this.instructions.Add (instruction);
//		}
	}

	private int IndexDeltaForDirection(Direction direction) {
		switch (direction) {
			case Direction.Up:
				return boardRadius * 2;
			case Direction.Down:
				return -boardRadius * 2;
			case Direction.Left:
				return -1;
			case Direction.Right:
				return 1;
			default:
				return 0;
		}
	}

	public void CheesePlaced(Slot cheeseSlot) { // sometimes guy teleports so need actual system for moving actors
		this.hasCheeseBeenPlaced = true;
		this.cheese = Instantiate (BoardManager.Instance().cheesePrefab).GetComponentInChildren<Tile>();
		this.cheese.gameObject.transform.position = cheeseSlot.transform.position;
		this.cheese.SetSlot (cheeseSlot);
		
		List<Instruction>.Enumerator e = this.instructions.GetEnumerator ();
		int i = 0;
		float delay = Tile.animationDuration;
		while (e.MoveNext ()) {
			Instruction instruction = e.Current;
			this.StartCoroutine (this.SetMovement ((Tile.animationDuration + delay) * i , instruction.instructionSet.actorId, instruction.instructionSet.direction));

			i++;
		}
		this.StartCoroutine (Reveal ((Tile.animationDuration + delay) * i));

		Animator mouseAnimator = this.mouse.GetComponent<Animator> ();
		mouseAnimator.SetTrigger ("IdleToMove");
	}

	private IEnumerator SetMovement(float delay, int actorId, Direction direction) {
		yield return new WaitForSeconds (delay);
		Actor.TriggerMove (actorId, direction);
	}

	public void MoveTileInDirection(Tile actor, Direction direction) {
		Slot nextSlot = this.SlotForIndex (actor.slot.index + this.IndexDeltaForDirection (direction));
		this.UnjoinSlotAndTile (actor.slot, actor);
		this.JoinSlotAndTile (nextSlot, actor);
	}

	private IEnumerator Reveal(float delay) {
		yield return new WaitForSeconds (delay);
		bool found = this.cheese.slot == this.mouse.slot;
//		this.target.ShowView (found);
		Animator mouseAnimator = this.mouse.GetComponent<Animator> ();
		if (found) {
			SpriteRenderer sr = this.cheese.GetComponentInChildren<SpriteRenderer> ();
			sr.sprite = this.noCheeseSprite;
			mouseAnimator.SetTrigger ("MoveToWin");
			this.level++;
			this.bonusTime += this.bonusTimeInSeconds;
		} else {
			mouseAnimator.SetTrigger ("MoveToLose");
			this.lives--;
			this.UpdateLives();
		}
		this.StartCoroutine (RestartCoroutine ());
	}

	public Sprite SpriteForActorId(int actorId) {
		GameObject g = this.actorPrefabs[actorId - 1]; //FFFF
		if (g == null) {
			return null;
		}

		Tile tile = g.GetComponent<Tile> ();
		return tile.sprite;
	}

	private IEnumerator RestartCoroutine() {
		yield return new WaitForSeconds (1.5f);
		this.Restart ();
		//Application.LoadLevel("Main");
	}

	private List<Actor> GenerateBlocks() {
		List<Actor> blockActors = new List<Actor> ();
		foreach (int blockId in Actor.blockIds) {
			GameObject prefab = this.actorPrefabs [blockId - 1];
			Actor actor = prefab.GetComponent<Actor> ();
			if (actor.minLevel > this.level) {
				continue;
			}

			blockActors.Add (actor);

			// choose random params
			int numPieces = Random.Range(actor.avgPieces - actor.avgPieceDeviation, actor.avgPieces + actor.avgPieceDeviation + 1);
			int mouseProximity = Random.Range (actor.avgMouseProximity - actor.avgMouseDeviation, actor.avgMouseProximity + actor.avgMouseDeviation + 1);

			// generate prefab at locations and attach to slots
			Slot initialSlot = this.mouse.slot;
			int distance = mouseProximity;
			List<Tile> block = new List<Tile> ();
			for (int i = 0; i < numPieces; i++) {
				Slot slot = this.RandomEmptySlotNearSlot (initialSlot, distance);
				if (slot == null) {
					break;
				}
				GameObject g = Instantiate (prefab) as GameObject;
				Tile tile = g.GetComponent<Tile> ();
				this.JoinSlotAndTile (slot, tile);
				tile.gameObject.transform.position = tile.slot.gameObject.transform.position;
				block.Add (tile);

				Actor a = g.GetComponent<Actor> ();
				blockActors.Add (a);

				distance = 1;

				List<Tile> selectableTiles = new List<Tile> (block);
				bool found = false;
				while (selectableTiles.Count > 0 && !found) {
					int randomIndex = Random.Range (0, selectableTiles.Count);
					Tile t = selectableTiles [randomIndex];
					if (this.NumberOfUnoccupiedNeighbors (t.slot) > 0) {
						found = true;
						initialSlot = t.slot;
					} else {
						selectableTiles.RemoveAt (randomIndex);
					}
				}

				if (!found) {
					break;
				}
			}
		}

		return blockActors;
	}

	public int NumberOfUnoccupiedNeighbors(Slot slot) {
		int total = 0;
		Slot s = this.SlotInDirectionFromSlot(slot, Direction.Up);
		if (s != null && s.tile == null) {
			total++;
		}
		s = this.SlotInDirectionFromSlot(slot, Direction.Down);
		if (s != null && s.tile == null) {
			total++;
		}

		s = this.SlotInDirectionFromSlot(slot, Direction.Left);
		if (s != null && s.tile == null) {
			total++;
		}

		s = this.SlotInDirectionFromSlot(slot, Direction.Right);
		if (s != null && s.tile == null) {
			total++;
		}

		return total;
	}

	public Slot RandomEmptySlotNearSlot(Slot startSlot, int distance) {
		List<Slot> blacklist = new List<Slot> ();
		Slot ret = null;
		int attempts = 0;
		int maxAttempts = 100;
		while ((ret == null || blacklist.Contains (ret)) && attempts < maxAttempts) {
			int x = startSlot.x;
			int y = startSlot.y;
			int moves = 0;

			while (moves < distance) {
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

				moves++;
			}

			ret = this.SlotForCoordinate (x, y);
			if (ret.tile != null) {
				blacklist.Add (ret);
				attempts++;
			}
		}

		if (attempts >= maxAttempts) {
			return null;
		}

		return ret;
	}


	#region - UI

	public void UpdateLevelText() {
		this.levelText.text = string.Format("Level {0}", this.level);
	}

	public void UpdateTimerText() {
		int seconds = this.RemainingTimeDisplay();
		System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);
		string timeText = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
		this.timerText.text = timeText;
	}

	public int RemainingTimeDisplay() {
		return Mathf.RoundToInt(this.RemainingTime());
	}

	public float RemainingTime() {
		return this.startTime - Time.time + this.initialTimeInSeconds + this.bonusTime;
	}

	public void UpdateLives() {
		for (int i = 0; i < this.initialLives - this.lives; i++) {
			this.mouseLivesImages[i].sprite = this.lifeLoseSprite;
		}
	}

	public void BackButtonPressed() {
		// to do: show prompt
		Application.LoadLevel("SplashScreen");
	}

	#endregion
}
