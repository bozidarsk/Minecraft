using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public static class Settings 
	{
		public class PerlinNoise 
		{
			public float persistance;
			public float lacunarity;
			public float scale;
			public uint octaves;
			public Vector2 offset;
		}

		public class Biome 
		{
			public string id;
			public uint seaLevel;
			public uint surfaceDepth;
			public RangeUInt height;
			public Range range;
			public PerlinNoise heightNoise;
		}

		public class Terrain 
		{
			public PerlinNoise biomeMapNoise;
			public Biome[] biomes;
		}

		public class Path 
		{
			[Header("Textures")]
			public string voxelTextures;
			public string itemTextures;
			public string liquidTextures;
			public string textureEffects;
			public string destroyStageTextures;
			public string skins;

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
			public string Skins { get { return skins; } }
		}

		public class PostProcessing 
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

		public class World 
		{
			public string name;
			public int seed;
			public Vector3 gravity;
			public int chunkSize;
			public int chunkHeight;
		}

		public class Player 
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
			public uint destroyStageLength;
		}

		public class Textures 
		{
			public Texture2D voxel;
			public Texture2D liquid;

			[HideInInspector] public int voxelWidth;
			[HideInInspector] public int voxelHeight;
			[HideInInspector] public int liquidWidth;
			[HideInInspector] public int liquidHeight;
		}

		public class Materials 
		{
			public Material liquid;
			public Material chunk;
			public Material droppedItem;
			public Material postProcessing;
			public Material player;
		}

		/*******************************************/

		public class KeyCodes 
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

		public class Controlls 
		{
			public float scrollSensitivity;
			public Vector2 sensitivity;
			public bool invertMouse;
			public bool autoJump;
			public KeyCodes keyCodes;
		}

		public class Sound 
		{
			public float musicVolume;
			public bool muteMusic;
			public float soundVolume;
			public bool muteSound;
		}

		public class Graphics 
		{
			public float FOV;
			public float GUIScale;
			public uint discardDistance;
			public uint renderDistance;
			public uint maxFPS;
		}
	}
}