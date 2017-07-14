using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Actor : MonoBehaviour {
	public int myActorId = -1;

	public int minLevel = 0;
	public int avgPieces = 1;
	public int avgPieceDeviation = 0;
	public int avgMouseProximity = 0;
	public int avgMouseDeviation = 0;

	// add spawning parameters like min level, avg connected pieces, deviation, spawn proximity to mouse
	// add 

	public static readonly int mouseId = 1;
	public static readonly List<int> blockIds = new List<int> {2, 3};

	public delegate void MoveAction (int actorId, Direction direction, List<int> blacklist = null);
	public static event MoveAction OnMoveTrigger;

	public static void TriggerMove(int actorId, Direction direction) {
		OnMoveTrigger (actorId, direction);
	}
		
	void OnEnable() {
		Actor.OnMoveTrigger += this.HandleMove;
	}

	void OnDisable() {
		Actor.OnMoveTrigger -= this.HandleMove;
	}

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	private void HandleMove(int actorId, Direction direction, List<int>blacklist = null) {
		if (this.myActorId != actorId) {
			return;
		}

		if (blacklist == null) {
			blacklist = new List<int> ();
		}
		this.MoveInDirection (direction, blacklist);
	}

	public void MoveInDirection(Direction direction, List<int> blacklist) {
		blacklist.Add (this.myActorId);
		Tile tile = this.gameObject.GetComponent<Tile> ();

		Tile influencedNeighbor = BoardManager.Instance ().TileInDirectionFromTile (tile, direction);
		if (influencedNeighbor != null) {
			Actor influencedActor = influencedNeighbor.gameObject.GetComponent<Actor> ();
			if (influencedActor != null && influencedActor != this && !blacklist.Contains(influencedActor.myActorId)) {
				Actor.TriggerMove (influencedActor.myActorId, direction);
			}
		}

		BoardManager.Instance ().MoveTileInDirection (tile, direction);
	}
}
