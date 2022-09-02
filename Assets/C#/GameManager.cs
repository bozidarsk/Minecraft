using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Minecraft 
{
	public class GameManager : MonoBehaviour
	{
		public static GameManager instance { private set; get; }
		public static Dictionary<string, Mesh> modelMeshes;
		public static Dictionary<string, Texture2D> textureEffects;
		public static VoxelProperty[] voxelProperties;
		public static ItemProperty[] itemProperties;
		public static CraftingProperty[] craftingProperties;
		public static EnchantmentProperty[] enchantmentProperties;
		public static Player[] players;
		public static System.Random random;

		void Awake() { GameManager.Initialize(this); }
		public static void Initialize(GameManager instance) 
		{
			GameManager.instance = instance;

			GameSettings.Initialize(JsonUtility.FromJson<GameSettingsObject>(File.ReadAllText(GameManager.FormatPath("$(DefaultData)/gameSettings.json"))));
			if (!Directory.Exists(GameSettings.path.GameData)) { Directory.CreateDirectory(GameSettings.path.GameData); }
			if (!Directory.Exists(GameSettings.path.GameSaves)) { Directory.CreateDirectory(GameSettings.path.GameSaves); }
			if (!Directory.Exists(GameSettings.path.WorldData)) { Directory.CreateDirectory(GameSettings.path.WorldData); }
			if (!Directory.Exists(GameSettings.path.WorldProperties)) { Directory.CreateDirectory(GameSettings.path.WorldProperties); }
			if (!Directory.Exists(GameSettings.path.WorldTextures)) { Directory.CreateDirectory(GameSettings.path.WorldTextures); }

			Console.Initialize();
			Noise.Initialize(new System.Random(GameSettings.world.seed));
			GameManager.random = new System.Random(GameSettings.world.seed);

			GameManager.modelMeshes = new Dictionary<string, Mesh>();
			GameManager.textureEffects = new Dictionary<string, Texture2D>();
			GameManager.voxelProperties = JsonUtility.FromJson<ArrayWrapper<VoxelProperty>>(File.ReadAllText(GameManager.FormatPath(GameSettings.path.voxelProperties))).content;
			GameManager.itemProperties = JsonUtility.FromJson<ArrayWrapper<ItemProperty>>(File.ReadAllText(GameManager.FormatPath(GameSettings.path.itemProperties))).content;
			GameManager.craftingProperties = JsonUtility.FromJson<ArrayWrapper<CraftingProperty>>(File.ReadAllText(GameManager.FormatPath(GameSettings.path.craftingProperties))).content;
			GameManager.enchantmentProperties = JsonUtility.FromJson<ArrayWrapper<EnchantmentProperty>>(File.ReadAllText(GameManager.FormatPath(GameSettings.path.enchantmentProperties))).content;
			GameManager.players = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Player>()).ToArray();

			string[] files = Directory.GetFiles(GameManager.FormatPath(GameSettings.path.textureEffects), "*.png");
			for (int i = 0; i < files.Length; i++) 
			{
				Texture2D texture = new Texture2D(1920, 1080);
				GameManager.InitializeTexture(ref texture);
				ImageConversion.LoadImage(texture, File.ReadAllBytes(files[i].Replace("\\", "/")), false);
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
				{ Console.Exception(new System.OverflowException("VoxelProperty.light must be less than 16. (at " + GameManager.voxelProperties[i].id + ")")); Console.Crash(); }
			}

			// #if UNITY_EDITOR
			// UnityEditor.EditorApplication.playModeStateChanged += (UnityEditor.PlayModeStateChange change) => 
			// {
			// 	if (change == UnityEditor.PlayModeStateChange.EnteredPlayMode) { GameManager.LoadGame(); }
			// 	if (change == UnityEditor.PlayModeStateChange.ExitingPlayMode) { GameManager.SaveGame(); }
			// };
			// #endif
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

		public static Player GetPlayerByName(string name) 
		{
			try { return GameManager.players.Where(x => x.name == name).ToArray()[0]; }
			catch { return null; }
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
		public static void Encode(ref string str) 
		{
			byte[] data = Encoding.UTF8.GetBytes(str);
			for (int i = 0; i < data.Length; i++) { data[i] = (byte)(0xff - data[i]); }
			str = Encoding.UTF8.GetString(data);
		}

		public static string Decode(string str) { Decode(ref str); return str; }
		public static void Decode(ref string str) 
		{
			byte[] data = Encoding.UTF8.GetBytes(str);
			for (int i = 0; i < data.Length; i++) { data[i] = (byte)(0xff - data[i]); }
			str = Encoding.UTF8.GetString(data);
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
						: Calculator.Solve(expression)
					);
				}
			}

			path = path.Replace("/", "\\").Replace("\\\\", "\\");
			return path;
		}
	}
}