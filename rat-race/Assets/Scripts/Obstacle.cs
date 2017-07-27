using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour {
	public int minLevel = 0;
	public int avgPieces = 1;
	public int avgPieceDeviation = 0;
	public int avgMouseProximity = 0;
	public int avgMouseDeviation = 0;

	public int interactionAmount = 0; // +1 is move affected object forward by 1, -1 is move it back (bounce), 0 is nothing
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
