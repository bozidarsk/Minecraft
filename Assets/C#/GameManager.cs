using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Minecraft 
{
	public class GameManager : MonoBehaviour
	{
		#pragma warning disable CS0649
		[SerializeField] private GameSettingsObject defaultGameSettings;
		#pragma warning restore CS0649

		public static GameManager instance { private set; get; }
		public static Dictionary<string, Mesh> modelMeshes;
		public static Dictionary<string, Texture2D> textureEffects;
		public static VoxelProperty[] voxelProperties;
		public static ItemProperty[] itemProperties;
		public static CraftingProperty[] craftingProperties;
		public static EnchantmentProperty[] enchantmentProperties;
		public static Player[] players;

		void Awake() { GameManager.Initialize(this); }
		public static void Initialize(GameManager instance) 
		{
			GameManager.instance = instance;

			Console.Initialize();
			GameSettings.Initialize(instance.defaultGameSettings);
			instance.defaultGameSettings = null;

			GameManager.modelMeshes = new Dictionary<string, Mesh>();
			GameManager.textureEffects = new Dictionary<string, Texture2D>();
			GameManager.voxelProperties = JsonUtility.FromJson<PropertyArray<VoxelProperty>>(File.ReadAllText(GameSettings.path.voxelProperties)).properties;
			GameManager.itemProperties = JsonUtility.FromJson<PropertyArray<ItemProperty>>(File.ReadAllText(GameSettings.path.itemProperties)).properties;
			GameManager.craftingProperties = JsonUtility.FromJson<PropertyArray<CraftingProperty>>(File.ReadAllText(GameSettings.path.craftingProperties)).properties;
			GameManager.enchantmentProperties = JsonUtility.FromJson<PropertyArray<EnchantmentProperty>>(File.ReadAllText(GameSettings.path.enchantmentProperties)).properties;
			GameManager.players = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Player>()).ToArray();

			string[] files = Directory.GetFiles(GameSettings.path.textureEffects, "*.png");
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

			if (GameManager.voxelProperties.Length > 0xffff + 1) { Console.Warning("Max ammount of volxel properties is 0xffff + 1, every property above this limit will be ignored."); }
			for (int i = 0; i < GameManager.voxelProperties.Length; i++) 
			{
				// if (GameManager.voxelProperties[i].id.Length > 50) { throw new System.OverflowException("VoxelProperty.id.Length must be less than 50. (at " + GameManager.voxelProperties[i].id + ")"); }
				if (GameManager.voxelProperties[i].light > 15) { throw new System.OverflowException("VoxelProperty.light must be less than 16. (at " + GameManager.voxelProperties[i].id + ")"); }
			}
		}

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
	}
}