using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Player Settings", menuName = "Player Settings")]
public class PlayerSettings : ScriptableObject
{
	[System.Serializable]
	public struct KeyCodes 
	{
		[Header("Movement")]
		public KeyCode Jump;
		public KeyCode Sneak;
		public KeyCode Sprint;
		public KeyCode MoveForward;
		public KeyCode MoveBackwards;
		public KeyCode MoveLeft;
		public KeyCode MoveRight;

		[Header("Gameplay")]
		public KeyCode Attack;
		public KeyCode PickBlock;
		public KeyCode UseItem;
		
		[Header("Inventory")]
		public KeyCode DropSelectedItem;
		public KeyCode HotbarSlot1;
		public KeyCode HotbarSlot2;
		public KeyCode HotbarSlot3;
		public KeyCode HotbarSlot4;
		public KeyCode HotbarSlot5;
		public KeyCode HotbarSlot6;
		public KeyCode HotbarSlot7;
		public KeyCode HotbarSlot8;
		public KeyCode HotbarSlot9;
		public KeyCode ToggleInventory;
		public KeyCode SwapItemWithOffhand;

		[Header("Miscellaneous")]
		public KeyCode TakeScreenshot;
		public KeyCode TogglePerspective;
		public KeyCode ToggleDebugView;
		public KeyCode OpenChat;
	}

	[System.Serializable]
	public struct Controlls 
	{
		public float scrollSensitivity;
		public Vector2 sensitivity; // <= 0
		public bool invertMouse;
		public bool autoJump;
		public KeyCodes keyCodes;
	}

	[System.Serializable]
	public struct Sound 
	{
		public float musicVolume; // <= 0
		public bool muteMusic;
		public float soundVolume; // <= 0
		public bool muteSound;
	}

	[System.Serializable]
	public struct Graphics 
	{
		public float FOV; // <= 0
		public float GUIScale; // <= 0
		public int renderDistance; // <= 0
		public int maxFPS; // <= 0
	}

	public Sound sound;
	public Graphics graphics;
	public Controlls controlls;

	void OnValidate() 
	{
		if (controlls.scrollSensitivity < 1f) { controlls.scrollSensitivity = 1f; }
		if (controlls.sensitivity.x < 0f) { controlls.sensitivity.x = 1f; }
		if (controlls.sensitivity.y < 0f) { controlls.sensitivity.y = 1f; }
		if (sound.soundVolume < 0f) { sound.soundVolume = 0f; }
		if (sound.musicVolume < 0f) { sound.musicVolume = 0f; }
		if (graphics.FOV < 1f) { graphics.FOV = 1f; }
		if (graphics.GUIScale < 1f) { graphics.GUIScale = 1f; }
		if (graphics.renderDistance < 1) { graphics.renderDistance = 1; }
		if (graphics.maxFPS < 1) { graphics.maxFPS = 1; }
	}
}