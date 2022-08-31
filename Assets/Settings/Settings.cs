using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public static class Settings 
	{
		[Serializable]
		public struct Path 
		{
			[Header("Textures")]
			public string voxelTextures;
			public string itemTextures;
			public string liquidTextures;
			public string textureEffects;

			[Header("Properties")]
			public string voxelProperties;
			public string itemProperties;
			public string craftingProperties;
			public string enchantmentProperties;

			[Header("Shaders")]
			public string chunkShader;
			public string postProcessingShader;
			public string droppedItemShader;
			public string playerShader;
			public string liquidShader;
		}

		[Serializable]
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

		[Serializable]
		public struct Terrain 
		{
			public int dirtDepth; // <= 0
		}

		[Serializable]
		public struct World 
		{
			public Vector3 gravity;
			public int worldSize; // <= 0
			public int chunkSize; // <= 0
			public int chunkHeight; // <= 0
		}

		[Serializable]
		public struct Player 
		{
			public float walkingSpeed; // < 0
			public float sprintingSpeed; // < 0
			public float sneakingSpeed; // < 0
			public float jumpSpeed; // < 0
			public float jumpHeight; // < 0
			public float reachingDistance; // < 0
			public Vector2 rotationLimit;
			public uint maxStackSize;
			public uint queuedMessagesLength;
		}

		public struct Textures 
		{
			public Texture2D voxel;
			public Texture2D item;
			public Texture2D liquid;

			[HideInInspector] public int voxelWidth;
			[HideInInspector] public int voxelHeight;
			[HideInInspector] public int itemWidth;
			[HideInInspector] public int itemHeight;
			[HideInInspector] public int liquidWidth;
			[HideInInspector] public int liquidHeight;
		}

		public struct Materials 
		{
			public Material liquid;
			public Material chunk;
			public Material droppedItem;
			public Material postProcessing;
			public Material player;
		}

		/*******************************************/

		[Serializable]
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

		[Serializable]
		public struct Controlls 
		{
			public float scrollSensitivity;
			public Vector2 sensitivity; // <= 0
			public bool invertMouse;
			public bool autoJump;
			public KeyCodes keyCodes;
		}

		[Serializable]
		public struct Sound 
		{
			public float musicVolume; // <= 0
			public bool muteMusic;
			public float soundVolume; // <= 0
			public bool muteSound;
		}

		[Serializable]
		public struct Graphics 
		{
			public float FOV; // <= 0
			public float GUIScale; // <= 0
			public int renderDistance; // <= 0
			public int maxFPS; // <= 0
		}
	}
}