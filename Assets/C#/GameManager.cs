﻿using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;

using Utils.Files;

namespace Minecraft 
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager instance { private set; get; }
		public static Dictionary<string, ObjectMesh> modelMeshes;
		public static Dictionary<string, ObjectMesh> singleTextureModelMeshes;
		public static Dictionary<string, Texture2D> textureEffects;
		public static VoxelProperty[] voxelProperties;
		public static ItemProperty[] itemProperties;
		public static CraftingProperty[] craftingProperties;
		public static List<Player> players;

		void Awake() { GameManager.Initialize(this); }
		public static void Initialize(GameManager instance) 
		{
			GameSettings.Load("$(DefaultData)/gameSettings.json");
			GameManager.instance = instance;

			if (!Directory.Exists(GameSettings.path.GameData)) { Directory.CreateDirectory(GameSettings.path.GameData); }
			if (!Directory.Exists(GameSettings.path.GameSaves)) { Directory.CreateDirectory(GameSettings.path.GameSaves); }
			if (!Directory.Exists(GameSettings.path.WorldData)) { Directory.CreateDirectory(GameSettings.path.WorldData); }
			if (!Directory.Exists(GameSettings.path.WorldProperties)) { Directory.CreateDirectory(GameSettings.path.WorldProperties); }
			if (!Directory.Exists(GameSettings.path.WorldTextures)) { Directory.CreateDirectory(GameSettings.path.WorldTextures); }

			Console.Initialize();
			Noise.Initialize(new System.Random(GameSettings.world.seed));

			GameManager.modelMeshes = new Dictionary<string, ObjectMesh>();
			GameManager.singleTextureModelMeshes = new Dictionary<string, ObjectMesh>();
			GameManager.textureEffects = new Dictionary<string, Texture2D>();


			GameManager.voxelProperties = Json.FromJsonFile<ArrayWrapper<VoxelProperty>>(GameManager.FormatPath(GameSettings.path.voxelProperties)).content;
			GameManager.itemProperties = Json.FromJsonFile<ArrayWrapper<ItemProperty>>(GameManager.FormatPath(GameSettings.path.itemProperties)).content;
			GameManager.craftingProperties = Json.FromJsonFile<ArrayWrapper<CraftingProperty>>(GameManager.FormatPath(GameSettings.path.craftingProperties)).content;

			GameManager.players = new List<Player>();
			GameManager.players = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Player>()).ToList();

			string[] files = Directory.GetFiles(GameManager.FormatPath(GameSettings.path.textureEffects), "*.png");
			for (int i = 0; i < files.Length; i++) 
			{
				Texture2D texture = new Texture2D(1, 1);
				ImageConversion.LoadImage(texture, File.ReadAllBytes(files[i].Replace("\\", "/")), false);
				GameManager.InitializeTexture(ref texture);
				GameManager.textureEffects.Add(files[i].Remove(0, files[i].LastIndexOf("\\") + 1).Replace(".png", ""), texture);
			}

			for (int i = 0; i < GameManager.itemProperties.Length; i++) 
			{
				if (GameManager.itemProperties[i].stackSize == 0) { GameManager.itemProperties[i].stackSize = 1; continue; }
				if (GameManager.itemProperties[i].stackSize > GameSettings.player.maxStackSize) 
				{ GameManager.itemProperties[i].stackSize = GameSettings.player.maxStackSize; }
			}

			if (GameManager.voxelProperties.Length > 0xffff + 1) { Console.Warning("Max ammount of volxel properties is 0xffff + 1. If you index a property above this limit you will have unexpexted behaviour."); }

			for (int i = 0; i < GameManager.voxelProperties.Length; i++) 
			{ 
				if (GameManager.voxelProperties[i].light > 15) 
				{ new System.OverflowException("VoxelProperty.light must be less than 16. (at " + GameManager.voxelProperties[i].id + ")"); }
			}

			// #if UNITY_EDITOR
			// UnityEditor.EditorApplication.playModeStateChanged += (UnityEditor.PlayModeStateChange change) => 
			// {
			// 	if (change == UnityEditor.PlayModeStateChange.EnteredPlayMode) { GameManager.LoadGame(); }
			// 	if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) { GameManager.SaveGame(); }
			// };
			// #endif

			singleTextureModelMeshes.Add("-stairs", new ObjectMesh(ConstMeshData.stairsVertices, ConstMeshData.stairsTriangles, ConstMeshData.stairsUvs));
			singleTextureModelMeshes.Add("-slab", new ObjectMesh(ConstMeshData.slabVertices, ConstMeshData.slabTriangles, ConstMeshData.slabUvs));
		}

		// #if !UNITY_EDITOR
		// void Start() { GameManager.LoadGame(); }
		// #if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX || UNITY_STANDALONE_OSX
		// void OnApplicationQuit() { GameManager.SaveGame(); }
		// #else
		// void OnApplicationFocus(bool hasFocus) { if (!hasFocus) { GameManager.SaveGame(); } }
		// #endif
		// #endif

		public static void InitializeTexture(ref Texture2D texture) 
		{
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;
		}

		public static void SaveGame() 
		{
			throw new System.NotImplementedException();
			// File.WriteAllText(GameManager.FormatPath("$(WorldData)/settings.json"), JsonUtility.ToJson(GameSettings.instance, true));
			// Player.Save(GameManager.players.Select(x => x.ToJson()).ToArray());
			// Chunk.Save(TerrainManager.modifiedChunks.Select(x => x.ToJson()).ToArray());
			// DroppedItem.Save(GameObject.FindObjectsOfType(typeof(DroppedItemController)).Select(
			// 	x => ((DroppedItemController)x).droppedItem.ToJson()).ToArray()
			// );
		}

		public static void LoadGame() 
		{
			throw new System.NotImplementedException();
			// Player.Load();
			// Chunk.Load();
			// DroppedItem.Load();
		}

		public static string Encode(string str) { Encode(ref str); return str; }
		public static void Encode(ref string str) { str = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(str).Select(x => (byte)(0xff - x)).ToArray()); }

		public static string Decode(string str) { Decode(ref str); return str; }
		public static void Decode(ref string str) { str = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(str).Select(x => (byte)(0xff - x)).ToArray()); }

		public static Player GetPlayerByName(string name) 
		{
			try { return players.Where(x => x.name == name).ToArray()[0]; }
			catch { return null; }
		}

		public static uint GetVoxelTypeById(string id) 
		{ for (uint i = 0; i < GameManager.voxelProperties.Length; i++) 
			{ if (GameManager.voxelProperties[i].id == id) { return i; } 
		} return GetVoxelTypeById("undefined-block"); }

		public static VoxelProperty GetVoxelPropertyById(string id) 
		{ for (uint i = 0; i < GameManager.voxelProperties.Length; i++) 
			{ if (GameManager.voxelProperties[i].id == id) { return GameManager.voxelProperties[i]; } 
		} return GetVoxelPropertyById("undefined-block"); }

		public static uint GetItemTypeById(string id) 
		{ for (uint i = 0; i < GameManager.itemProperties.Length; i++) 
			{ if (GameManager.itemProperties[i].id == id) { return i; } 
		} return GetItemTypeById("undefined-item"); }

		public static ItemProperty GetItemPropertyById(string id) 
		{ for (uint i = 0; i < GameManager.itemProperties.Length; i++) 
			{ if (GameManager.itemProperties[i].id == id) { return GameManager.itemProperties[i]; } 
		} return GetItemPropertyById("undefined-item"); }

		public static string FormatPath(string path) 
		{
			int i = 0;
			for (; i < path.Length; i++) 
			{
				if (!(path[i] == '$' && path[i + 1] == '(')) { continue; }

				i += 2;
				int t = i;
				int bracketCount = 1;
				string expression = "";
				for (; t < path.Length; t++) 
				{
					if (path[t] == '(') { bracketCount++; }
					if (path[t] == ')') { bracketCount--; }
					if (path[t] == ')' && bracketCount == 0) { break; }
					expression += path[t];
				}

				if (t >= path.Length) { i += expression.Length; }
				else 
				{
					i -= 2;
					int length = expression.Length;
					expression = expression.Replace(" ", "");
					path = path.Remove(i, length + 3);

					path = path.Insert(i, (Tools.HasMember(typeof(Settings.Path), "System.String get_" + expression + "()"))
						? typeof(Settings.Path).GetProperty(expression).GetValue(GameSettings.path, null).ToString()
						: Utils.Calculators.BasicCalculator.Solve(expression)
					);
				}
			}

			path = path.Replace("/", "\\").Replace("\\\\", "\\");
			return (path.Contains("$(")) ? GameManager.FormatPath(path) : path;
		}
	}
}