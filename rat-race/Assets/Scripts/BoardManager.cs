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

public struct BoardAnimation {
	public int actorId;
	public Direction direction;
	public int movementAmount;
}

public class BoardManager : MonoBehaviour {
	public static float unit = 0.28f;

	[SerializeField]
	int boardRadius;
	[SerializeField]
	Slot[] slotPrefabs;
	[SerializeField]
	GameObject instructionPrefab;
	[SerializeField]
	GameObject mousePrefab;
	[SerializeField]
	GameObject cheesePrefab;
	[SerializeField]
	GameObject instructionArea;
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
	Sprite lifeLoseBackground;

	[SerializeField]
	Text levelText;
	[SerializeField]
	Text timerText;
	[SerializeField]
	GameObject livesContainer;
	[SerializeField]
	List<Image> mouseLivesImages;
	[SerializeField]
	List<Image> mouseLivesBackgrounds;
	[SerializeField]
	GameObject resultsView;
	[SerializeField]
	Text resultScoreText;
	[SerializeField]
	Text resultNewHighScoreText;
	[SerializeField]
	Text resultCongratsText;
	[SerializeField]
	Text flashText;

	public bool hasCheeseBeenPlaced = false;
	private bool hasLost = false;
	private int rounds = 0;
	private float startTime;
	private int bonusTime;
	private int lives;
	private bool hasNewHighScore = false;

	private List<Slot> slots = new List<Slot>();
	private List<Instruction> instructions = new List<Instruction>();
	private List<List<BoardAnimation>> boardAnimations = new List<List<BoardAnimation>>();
	private int[,] _boardState;
	private Tile mouse;
	private Tile cheese;
	private List<Actor> actors = new List<Actor>();
	private Vector3 flashTextTarget;

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
		if (!this.hasLost) {
			this.UpdateTimerText();
			this.CheckLoss();
		} else if (!this.resultsView.activeInHierarchy) {
			this.ShowResults();
		}
	}

	private void CheckLoss() {
		if ((this.lives <= 0 && this.initialLives > 0) || this.RemainingTime() <= 0) {
			UpdateHighScore();

			this.hasLost = true;
		}
	}

	private void UpdateHighScore() {
		int highScore = PlayerPrefs.GetInt("highScore");
		int level = this.CurrentLevel();
		if (level > highScore) {
			this.hasNewHighScore = true;
			PlayerPrefs.SetInt("highScore", level);
			Debug.LogFormat("New high score: {0}", level);
		} else {
			Debug.LogFormat("Score: {0}", level);
		}
	}

	private void Initialize() {
		this.startTime = Time.time;
		this.lives = this.initialLives;
		int difficulty = PlayerPrefs.GetInt("difficulty");
		int level = Mathf.Max(1, difficulty);
		this.rounds = this.DefaultRoundsForLevel(level);
		this.resultsView.SetActive(false);
		this.livesContainer.SetActive(this.initialLives > 0);
		this.flashText.gameObject.SetActive(false);
	}

	private void Restart() {
		this.UpdateLevelText();

		this.MakeBoard ();
		this.MakeTiles ();
		this.MakeInstructions ();
		this.UpdateLives();
	}

	private int BoardRadiusForLevel() {
		return Mathf.Max(this.CurrentLevel() / 2, 1); //1 + ((int)Mathf.Log (this.CurrentLevel()));
	}

	private void MakeBoard() {
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
				slot.gameObject.transform.SetParent(this.gameObject.transform);
				slot.gameObject.transform.localPosition = new Vector3 (offset + j * unit, offset + i * unit, 0);
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
		g.transform.SetParent(this.gameObject.transform);
	
		this.mouse = g.GetComponent<Tile> ();

		this.JoinSlotAndTile (this.RandomUnoccupiedSlot (), this.mouse);
		this.mouse.gameObject.transform.localPosition = this.mouse.slot.gameObject.transform.localPosition;

		Actor mouseActor = this.mouse.GetComponent<Actor> ();
		this.actors.Add (mouseActor);
		List<Actor> blockActors = this.GenerateObstacles ();
		this.actors.AddRange (blockActors);
	}

	public Slot SlotInDirectionFromSlot(Slot slot, Direction direction) {
		if (!this.IsValidDirectionFromSlot(slot, direction)) {
			return null;
		}
		return this.SlotForIndex (slot.index + this.IndexDeltaForDirection (direction));
	}

	public bool IsValidDirectionFromSlot(Slot slot, Direction direction) {
		if (slot == null) {
			Debug.Log("hmmmm");
		}
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

	public Direction ReverseDirection(Direction direction) {
		switch (direction) {
		case Direction.Down:
			return Direction.Up;
		case Direction.Left:
			return Direction.Right;
		case Direction.Right:
			return Direction.Left;
		case Direction.Up:
			return Direction.Down;
		}

		return direction; // shouldn't get here
	}

	public Tile TileInDirectionFromTile(Tile tile, Direction direction) {
		Slot nextSlot = this.SlotInDirectionFromSlot (tile.slot, direction);
		if (nextSlot == null) {
			return null;
		}

		return nextSlot.tile;
	}

	private void JoinSlotAndTile(Slot slot, Tile tile) {
		if (tile == null || slot == null) {
			return;
		}
		slot.SetTile (tile);
		tile.SetSlot (slot);
	}
		
	private void UnjoinSlotAndTile(Slot slot, Tile tile) {
		if (tile == null || slot == null) {
			return;
		}
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

	private List<BoardAnimation> ApplyMove(int [,] boardState, int actorId, Direction direction, List<int> blacklist = null) {
		List<BoardAnimation> animations = new List<BoardAnimation>{new BoardAnimation {actorId = actorId, direction = direction, movementAmount = 1}};
		if (blacklist == null) {
			blacklist = new List<int> ();
		}
		blacklist.Add (actorId);
		Dictionary<int, Direction> influencedActors = new Dictionary<int, Direction> ();
		List<Vector2> positions = new List<Vector2> ();
		for (int i = boardState.GetLength (0) - 1; i >= 0; i--) {
			for (int j = 0; j < boardState.GetLength (1); j++) {
				int val = boardState [i, j];
				if (val != actorId) {
					continue;
				}
				Actor actor = this.ActorsWithId(actorId)[0];
				Direction reverseDirection = this.ReverseDirection(direction);
					
				int frontNeighborId = this.ValueInDirection (boardState, i, j, direction);
				int backNeighborId = this.ValueInDirection(boardState, i, j, reverseDirection);

				if (frontNeighborId >= 0) {
					positions.Add(this.PositionInDirection(i, j, direction)); // places this tile will move to
				}

				if (frontNeighborId > 0 && actor.isBlock) {
					if (!influencedActors.ContainsKey (frontNeighborId) && !blacklist.Contains(frontNeighborId)) {
						influencedActors.Add(frontNeighborId, direction);
					}
				}
				if (backNeighborId > 0 && actor.isMagnet) {
					if (!influencedActors.ContainsKey (backNeighborId) && !blacklist.Contains(backNeighborId)) {
						influencedActors.Add(backNeighborId, direction);
					}
				}
					
				boardState [i, j] = 0;
			}
		}

		foreach (KeyValuePair<int, Direction> item in influencedActors) {
			int influencedActorId = item.Key;
			Direction influencedDirection = item.Value;
			List<BoardAnimation> additionalAnimations = this.ApplyMove (boardState, influencedActorId, influencedDirection, blacklist);
			animations.AddRange(additionalAnimations);
		}

		foreach (Vector2 position in positions) {
			boardState [(int)position.x, (int)position.y] = actorId;
		}

		return animations;
	}

	private bool IsLegalMove(int [,] boardState, int actorId, Direction direction, List<int> blacklist = null) {
		if (blacklist == null) {
			blacklist = new List<int> ();
		}
		blacklist.Add (actorId);
		bool ret = true;
		Dictionary<int, Direction> influencedActors = new Dictionary<int, Direction> ();
		for (int i = boardState.GetLength (0) - 1; i >= 0; i--) {
			for (int j = 0; j < boardState.GetLength (1); j++) {
				int val = boardState [i, j];
				if (val != actorId) {
					continue;
				}
				Actor actor = this.ActorsWithId(actorId)[0];
				Direction reverseDirection = this.ReverseDirection(direction);

				int frontNeighborId = this.ValueInDirection(boardState, i, j, direction);
				int backNeighborId = this.ValueInDirection(boardState, i, j, reverseDirection);

				if (actor.isBlock && frontNeighborId < 0) {
					return false;
				}

				if (frontNeighborId > 0 && actor.isBlock && !influencedActors.ContainsKey (frontNeighborId) && !blacklist.Contains (frontNeighborId)) {
					influencedActors.Add (frontNeighborId, direction);
				}

				if (backNeighborId > 0 && actor.isMagnet && !influencedActors.ContainsKey (backNeighborId) && !blacklist.Contains (backNeighborId)) {
					influencedActors.Add(backNeighborId, direction);
				}
			}
		}

		foreach (KeyValuePair<int, Direction> item in influencedActors) {
			int influencedActorId = item.Key;
			Direction influencedDirection = item.Value;
			ret = ret && this.IsLegalMove (boardState, influencedActorId, influencedDirection, blacklist);
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
		this.boardAnimations.Clear();
		foreach (Transform child in this.instructionArea.transform) {
			GameObject.Destroy(child.gameObject);
		}

		int level = this.CurrentLevel();
		//int randomAmount = Random.Range (level, 2 * level);
		int numberOfInstructions = Mathf.Min(level, 10);

		_boardState = this.GetBoardState ();
//		this.PrintBoardState (_boardState);
		while (this.instructions.Count < numberOfInstructions) {
			InstructionSet instructionSet = this.RandomValidNextInstruction (_boardState);
			if (instructionSet == null) {
				this.instructions.RemoveAt (this.instructions.Count - 1);
				continue;
			}

			GameObject g = Instantiate (this.instructionPrefab) as GameObject;
			g.transform.SetParent(this.gameObject.transform);

			Instruction instruction = g.GetComponent<Instruction>();
			instruction.SetupWithInstructionSet (instructionSet);
			instruction.gameObject.transform.SetParent (this.instructionArea.gameObject.transform, false);

			this.instructions.Add (instruction);

			List<BoardAnimation> animationsForInstruction = this.ApplyMove (_boardState, instructionSet.actorId, instructionSet.direction);
			this.boardAnimations.Add(animationsForInstruction);
//			this.PrintBoardState (boardState);
		}
			
		this.PrintBoardState (_boardState);
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

	public void CheesePlaced(Slot cheeseSlot) {
		this.hasCheeseBeenPlaced = true;
		this.cheese = Instantiate (BoardManager.Instance().cheesePrefab).GetComponentInChildren<Tile>();
		this.cheese.gameObject.transform.SetParent(this.gameObject.transform);
		this.cheese.gameObject.transform.localPosition = cheeseSlot.transform.localPosition;
		this.cheese.SetTarget(cheeseSlot.transform.localPosition);
//		this.cheese.SetSlot (cheeseSlot);

		List<List<BoardAnimation>>.Enumerator e = this.boardAnimations.GetEnumerator ();
		int i = 0;
		float delay = Tile.animationDuration;
		while (e.MoveNext ()) {
			List<BoardAnimation> lb = e.Current;
			List<BoardAnimation>.Enumerator ee = lb.GetEnumerator();
			while (ee.MoveNext()) {
				BoardAnimation b = ee.Current;
				this.StartCoroutine (this.SetMovement ((Tile.animationDuration + delay) * i , b.actorId, b.direction));
			}

			i++;
		}

		bool wins = this._boardState[cheeseSlot.y, cheeseSlot.x] == Actor.mouseId;
		this.StartCoroutine (Reveal ((Tile.animationDuration + delay) * i, wins));

//		List<Instruction>.Enumerator e = this.instructions.GetEnumerator ();
//		int i = 0;
//		float delay = Tile.animationDuration;
//		while (e.MoveNext ()) {
//			Instruction instruction = e.Current;
//			this.StartCoroutine (this.SetMovement ((Tile.animationDuration + delay) * i , instruction.instructionSet.actorId, instruction.instructionSet.direction));
//
//			i++;
//		}
//		this.StartCoroutine (Reveal ((Tile.animationDuration + delay) * i));
	}

	private IEnumerator SetMovement(float delay, int actorId, Direction direction) {
		yield return new WaitForSeconds (delay);
		Actor.TriggerMove (actorId, direction);
	}

	public void MoveTileInDirection(Tile actor, Direction direction, int actorId) {
		actor.MoveInDirection(direction);
//		Slot nextSlot = this.SlotForIndex (actor.slot.index + this.IndexDeltaForDirection (direction));
//		this.UnjoinSlotAndTile (actor.slot, actor);
//		this.JoinSlotAndTile (nextSlot, actor);
		if (actorId == Actor.mouseId) {
			this.PlayMouseAnimationInDirection(direction);
		}
	}

	public void PlayMouseAnimationInDirection(Direction direction) {
		Animator mouseAnimator = this.mouse.GetComponent<Animator> ();
		switch (direction) {
		case Direction.Down:
			mouseAnimator.SetTrigger("MouseMoveUp"); // tell gus to fix this
			break;
		case Direction.Left:
			mouseAnimator.SetTrigger("MouseMoveLeft");
			break;
		case Direction.Right:
			mouseAnimator.SetTrigger("MouseMoveRight");
			break;
		case Direction.Up:
			mouseAnimator.SetTrigger("MouseMoveDown");
			break;
		}
	}
		
	private IEnumerator Reveal(float delay, bool found) {
		yield return new WaitForSeconds (delay);
//		bool found = this.cheese.slot == this.mouse.slot;
		Animator mouseAnimator = this.mouse.GetComponent<Animator> ();
		if (found) {
			Animator cheeseAnimator = this.cheese.GetComponentInChildren<Animator> ();
			cheeseAnimator.SetTrigger("CheeseWin");
			mouseAnimator.SetTrigger ("MouseWin");
			this.rounds++;
			this.bonusTime += this.bonusTimeInSeconds;
			this.FlashTimeDeltaText(this.bonusTimeInSeconds);
		} else {
			mouseAnimator.SetTrigger ("MouseLose");
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

	public Sprite BackgroundSpriteForActorId(int actorId) {
		GameObject g = this.actorPrefabs[actorId - 1]; //FFFF
		if (g == null) {
			return null;
		}

		Tile tile = g.GetComponent<Tile> ();
		return tile.backgroundSprite;
	}

	public Colorable ColorableForActorId(int actorId) {
		List<Actor> a = this.ActorsWithId(actorId);
		if (a.Count == 0) {
			return null;
		}

		GameObject g = a[0].gameObject;
		Colorable c = g.GetComponent<Colorable> ();
		return c;
	}

	private IEnumerator RestartCoroutine() {
		yield return new WaitForSeconds (1.5f);
		if (this.hasLost) {
			// don't start next level if lost
		} else {
			this.Restart();
		}
	}

	private List<Actor> GenerateObstacles() {
		List<Actor> blockActors = new List<Actor> ();
		foreach (GameObject prefab in this.actorPrefabs) {
			Actor actor = prefab.GetComponent<Actor> ();
			if (!(actor.isBlock || actor.isMagnet) || actor.myActorId == Actor.mouseId) {
				continue;
			}
			if (actor.minLevel > this.CurrentLevel()) {
				continue;
			}
				
			// choose random params
			int level = this.CurrentLevel() - actor.minLevel;
			float numPiecesScale = Mathf.Max(level * actor.avgPieceLevelScale, 1.0f);
			float mouseProximityScale = Mathf.Max(level * actor.avgMouseLevelScale, 1.0f);
			int numPieces = (int)(numPiecesScale * Random.Range(actor.avgPieces - actor.avgPieceDeviation, actor.avgPieces + actor.avgPieceDeviation + 1));
			int mouseProximity = (int)(mouseProximityScale * Random.Range(actor.avgMouseProximity - actor.avgMouseDeviation, actor.avgMouseProximity + actor.avgMouseDeviation + 1));

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
				g.transform.SetParent(this.gameObject.transform);

				Tile tile = g.GetComponent<Tile> ();
				this.JoinSlotAndTile (slot, tile);
				tile.gameObject.transform.localPosition = tile.slot.gameObject.transform.localPosition;
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

	public int CurrentLevel() {
		return this.LevelForRounds(this.rounds);
	}

	public int LevelForRounds(int rounds) {
		int i = 1;
		while (rounds > i) {
			rounds -= i;
			i++;
		}

		return i;
	}

	public int DefaultRoundsForLevel(int level) {
		return (level * (level + 1)) / 2;
	}

	#region - UI

	public void UpdateLevelText() {
		this.levelText.text = string.Format("Level {0}", this.CurrentLevel());
	}

	public void UpdateTimerText() {
		int seconds = this.RemainingTimeDisplay();
		System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(seconds);
		string timeText = string.Format("{0:D2}:{1:D2}", timeSpan.Minutes, timeSpan.Seconds);
		this.timerText.text = timeText;

		// handle flash text movement, alpha, dissappear here
		//if (flashText.IsActive()) {
			//float rate = 0.05f;
			//Color flashTextColor = flashText.color;
			//flashTextColor.a -= rate;
			//flashText.color = flashTextColor;
			//flashText.gameObject.transform.position = Vector3.Lerp(flashText.gameObject.transform.position, this.flashTextTarget, 0.1f);

			//if (flashText.color.a <= 0.0f) {
				//flashText.gameObject.SetActive(false);
			//}
		//}
	}

	public void FlashTimeDeltaText(int secondsDelta) {
		if (secondsDelta == 0) {
			return;
		}
		bool isNegative = secondsDelta < 0;
		string secondsDeltaString = string.Format("{0}{1}", isNegative ? "" : "+", secondsDelta);

		this.flashText.text = secondsDeltaString;
		this.flashText.color = isNegative ? Color.red : Color.blue;
		//this.flashText.transform.position = this.timerText.transform.position;
		this.flashText.gameObject.SetActive(true);
		Vector3 flashTextTargetOffset = (isNegative ? -1 : 1) * new Vector3(0.0f, 25.0f, 0.0f);
		this.flashTextTarget = this.timerText.transform.position + flashTextTargetOffset;
	}


	public int RemainingTimeDisplay() {
		return Mathf.RoundToInt(this.RemainingTime());
	}

	public float RemainingTime() {
		return this.startTime - Time.time + this.initialTimeInSeconds + this.bonusTime;
	}

	public void UpdateLives() {
		if (this.initialLives < 1) {
			return;
		}

		for (int i = 0; i < this.mouseLivesImages.Count && i < this.mouseLivesBackgrounds.Count; i++) {
			if (i < this.initialLives - this.lives) {
				this.mouseLivesImages[i].sprite = this.lifeLoseSprite;
				this.mouseLivesBackgrounds[i].sprite = this.lifeLoseBackground;
			}
			if (this.mouse != null) {
				this.mouseLivesBackgrounds[i].color = this.mouse.colorable.GetSelectedColor();
			}
		}
	}

	public void BackButtonPressed() {
		UpdateHighScore();
		Application.LoadLevel("SplashScreen");
	}

	public void ShowResults() {
		this.resultsView.SetActive(true);
		this.resultScoreText.text = string.Format("Score: {0}", this.CurrentLevel()); // should be based on time for each, w/ auto level setup using some default time
		if (!this.hasNewHighScore) {
			this.resultNewHighScoreText.text = "";
		}
	}

	#endregion
}