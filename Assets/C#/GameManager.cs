using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Minecraft 
{
	public class GameManager : MonoBehaviour
	{
		// public ComputeShader generateChunk;
		#pragma warning disable CS0649
		[SerializeField] private GameSettingsObject gameSettings;
		[SerializeField] private GameManagerTextures textures;
		[SerializeField] private GameManagerMaterials materials;
		#pragma warning restore CS0649

		public MonoBehaviour monoBehaviour { get { return this; } }
		[HideInInspector] public Dictionary<string, Mesh> modelMeshes;
		[HideInInspector] public Dictionary<string, Texture2D> textureEffects;
		[HideInInspector] public TerrainManager terrainManager;
		[HideInInspector] public ChatManager chatManager;
		[HideInInspector] public CraftingProperty[] craftingProperties;
		[HideInInspector] public VoxelProperty[] voxelProperties;
		[HideInInspector] public ItemProperty[] itemProperties;
		[HideInInspector] public EnchantmentProperty[] enchantmentProperties;
		[HideInInspector] public Player[] players;

		void Awake() 
		{
			textures.voxel.filterMode = FilterMode.Point;
			textures.voxel.wrapMode = TextureWrapMode.Clamp;
			textures.voxelWidth = textures.voxel.width;
			textures.voxelHeight = textures.voxel.height;

			textures.item.filterMode = FilterMode.Point;
			textures.item.wrapMode = TextureWrapMode.Clamp;
			textures.itemWidth = textures.item.width;
			textures.itemHeight = textures.item.height;

			textures.liquid.filterMode = FilterMode.Point;
			textures.liquid.wrapMode = TextureWrapMode.Clamp;
			textures.liquidWidth = textures.liquid.width;
			textures.liquidHeight = textures.liquid.height;

			Console.Initialize();
			GameSettings.Initialize(gameSettings, textures, materials);

			modelMeshes = new Dictionary<string, Mesh>();
			textureEffects = new Dictionary<string, Texture2D>();
			craftingProperties = JsonUtility.FromJson<PropertyArray<CraftingProperty>>(File.ReadAllText("Assets/Objects/Properties/crafting.json")).properties;
			voxelProperties = JsonUtility.FromJson<PropertyArray<VoxelProperty>>(File.ReadAllText("Assets/Objects/Properties/voxel.json")).properties;
			itemProperties = JsonUtility.FromJson<PropertyArray<ItemProperty>>(File.ReadAllText("Assets/Objects/Properties/item.json")).properties;
			enchantmentProperties = JsonUtility.FromJson<PropertyArray<EnchantmentProperty>>(File.ReadAllText("Assets/Objects/Properties/enchantment.json")).properties;
			players = GameObject.FindGameObjectsWithTag("Player").Select(x => x.GetComponent<Player>()).ToArray();
			terrainManager = gameObject.GetComponent<TerrainManager>();
			chatManager = gameObject.GetComponent<ChatManager>();

			string[] files = Directory.GetFiles("Assets/Objects/player/TextureEffects", "*.png");
			for (int i = 0; i < files.Length; i++) 
			{
				Texture2D texture = new Texture2D(1920, 1080);
				texture.filterMode = FilterMode.Point;
				texture.wrapMode = TextureWrapMode.Clamp;
				ImageConversion.LoadImage(texture, File.ReadAllBytes(files[i].Replace("\\", "/")), false);
				textureEffects.Add(files[i].Remove(0, files[i].LastIndexOf("\\") + 1).Replace(".png", ""), texture);
			}

			if (voxelProperties.Length > 0xffff + 1) { Console.Warning("Max ammount of volxel properties is 0xffff + 1, every property above this limit will be ignored."); }
			for (int i = 0; i < voxelProperties.Length; i++) { if (voxelProperties[i].id.Length > 50) { throw new System.OverflowException("VoxelProperty.id.Length must be less than 50. (id: " + voxelProperties[i].id + ")"); } }

			for (int i = 0; i < itemProperties.Length; i++) 
			{
				if (itemProperties[i].stackSize == 0) { itemProperties[i].stackSize = 1; continue; }
				if (itemProperties[i].stackSize > GameSettings.player.stackSize) 
				{ itemProperties[i].stackSize = GameSettings.player.stackSize; }
			}
		}

		public Player GetPlayerByName(string name) 
		{
			try { return players.Where(x => x.name == name).ToArray()[0]; }
			catch { return null; }
		}

		public ushort GetVoxelTypeById(string id) 
		{ for (ushort i = 0; i < voxelProperties.Length; i++) 
			{ if (voxelProperties[i].id == id) { return i; } 
		} return GetVoxelTypeById("undefined-block"); }

		public VoxelProperty GetVoxelPropertyById(string id) 
		{ for (ushort i = 0; i < voxelProperties.Length; i++) 
			{ if (voxelProperties[i].id == id) { return voxelProperties[i]; } 
		} return GetVoxelPropertyById("undefined-block"); }


		public ushort GetItemTypeById(string id) 
		{ for (ushort i = 0; i < itemProperties.Length; i++) 
			{ if (itemProperties[i].id == id) { return i; } 
		} return GetItemTypeById("undefined-item"); }

		public ItemProperty GetItemPropertyById(string id) 
		{ for (ushort i = 0; i < itemProperties.Length; i++) 
			{ if (itemProperties[i].id == id) { return itemProperties[i]; } 
		} return GetItemPropertyById("undefined-item"); }
	}
}