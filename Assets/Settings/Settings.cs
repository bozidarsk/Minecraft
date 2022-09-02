using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public static class Settings 
	{
		[Serializable]
		public struct Biome 
		{
			public string id;
			public uint seaLevel;
			public uint dirtDepth;
		}

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

			[Header("Saved Data")]
			public string savedChunks;
			public string savedPlayers;
			public string savedInteractables;
			public string savedEntities;
			public string savedDroppedItems;

			public string GameData { get { return Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\.minecraft2"; } }
			public string GameSaves { get { return GameSettings.path.GameData + "\\saves"; } }
			public string WorldData { get { return GameSettings.path.GameSaves + "\\" + GameSettings.world.name; } }
			public string WorldProperties { get { return GameSettings.path.WorldData + "\\properties"; } }
			public string WorldTextures { get { return GameSettings.path.WorldData + "\\textures"; } }
			public string DefaultData { get { return "Assets/Settings/Defaults"; } }
			public string DefaultProperties { get { return DefaultData + "\\properties"; } }
			public string DefaultTextures { get { return DefaultData + "\\textures"; } }
		}

		[Serializable]
		public struct Noise 
		{
			public int maxHeight;
			public float persistance;
			public float lacunarity;
			public float scale;
			public uint octaves;
			public Vector2 offset;
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
			public Biome[] biomes;

			public Biome GetBiomeById(string id) 
			{ for (uint i = 0; i < biomes.Length; i++) 
				{ if (biomes[i].id == id) { return biomes[i]; } 
			} return GetBiomeById("plains-biome"); }
		}

		[Serializable]
		public struct World 
		{
			public string name;
			public int seed;
			public Vector3 gravity;
			public int chunkSize;
			public int chunkHeight;
		}

		[Serializable]
		public struct Player 
		{
			public float walkingSpeed;
			public float sprintingSpeed;
			public float sneakingSpeed;
			public float jumpSpeed;
			public float jumpHeight;
			public float reachingDistance;
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
			public Vector2 sensitivity;
			public bool invertMouse;
			public bool autoJump;
			public KeyCodes keyCodes;
		}

		[Serializable]
		public struct Sound 
		{
			public float musicVolume;
			public bool muteMusic;
			public float soundVolume;
			public bool muteSound;
		}

		[Serializable]
		public struct Graphics 
		{
			public float FOV;
			public float GUIScale;
			public int renderDistance;
			public int maxFPS;
		}
	}
}