using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Game Settings", menuName = "Game Settings")]
public class GameSettings : ScriptableObject
{
	[System.Serializable]
	public struct PostProcessing 
	{
		public Color fogColor;
	    public float fogDensity;
	    public float fogOffset;
	    public float exposure;
	    public float temperature;
	    public float tint;
	    public float contrast;
	    public float brightness;
	    public float colorFiltering;
	    public float saturation;
	    public float gamma;
	}

	[System.Serializable]
	public struct Terrain 
	{
		public int chunkSize; // <= 0
		public int chunkHeight; // <= 0
		public int dirtDepth; // <= 0
	}

	[System.Serializable]
	public struct Player 
	{
		public float walkingSpeed; // < 0
		public float sprintingSpeed; // < 0
		public float sneakingSpeed; // < 0
		public float jumpHeight; // < 0
		public float reachingDistance; // < 0
		public Vector2 rotationLimit;
		public Vector3 gravity;
		public uint stackSize;
		public uint queuedMessagesLength;
	}

	public Player player;
	public PostProcessing postProcessing;
	public Terrain terrain;

	void OnValidate() 
	{
		if (terrain.chunkSize < 1) { terrain.chunkSize = 1; }
		if (terrain.chunkHeight < 1) { terrain.chunkHeight = 1; }
		if (terrain.dirtDepth < 1) { terrain.dirtDepth = 1; }
		if (player.walkingSpeed < 0f) { player.walkingSpeed = 0f; }
		if (player.sprintingSpeed < 0f) { player.sprintingSpeed = 0f; }
		if (player.sneakingSpeed < 0f) { player.sneakingSpeed = 0f; }
		if (player.jumpHeight < 0f) { player.jumpHeight = 0f; }
		if (player.reachingDistance < 0f) { player.reachingDistance = 0f; }
		if (player.rotationLimit.x < 10f) { player.rotationLimit.x = 10f; }
		if (player.rotationLimit.x > 90f) { player.rotationLimit.x = 90f; }
		if (player.rotationLimit.y < 10f) { player.rotationLimit.y = 10f; }
		if (player.rotationLimit.y > 180f) { player.rotationLimit.y = 180f; }
	}
}