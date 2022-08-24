using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	public GameSettings gameSettings;

	public GameManagerTextures textures;
	public GameManagerMaterials materials;

	[HideInInspector] public Dictionary<string, Mesh> modelMeshes;
	[HideInInspector] public Dictionary<string, Texture2D> textureEffects;
	[HideInInspector] public TerrainManager terrainManager;
	[HideInInspector] public ChatManager chatManager;
	[HideInInspector] public CraftingProperty[] craftingProperties;
	[HideInInspector] public VoxelProperty[] voxelProperties;
	[HideInInspector] public ItemProperty[] itemProperties;
	[HideInInspector] public EnchantmentProperty[] enchantmentProperties;
	[HideInInspector] public List<Chunk> modifiedChunks;
	[HideInInspector] public Player[] players;

	void Awake() 
	{
		modifiedChunks = new List<Chunk>();
		textures.voxel.filterMode = FilterMode.Point;
		textures.voxel.wrapMode = TextureWrapMode.Clamp;
		textures.item.filterMode = FilterMode.Point;
		textures.item.wrapMode = TextureWrapMode.Clamp;
		textures.liquid.filterMode = FilterMode.Point;
		textures.liquid.wrapMode = TextureWrapMode.Clamp;

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

		if (voxelProperties.Length > 0xffff + 1) { Debug.LogWarning("Max ammount of volxel properties is 0xffff + 1, every property above this limit will be ignored."); }

		for (int i = 0; i < itemProperties.Length; i++) 
		{
			if (itemProperties[i].stackSize == 0) { itemProperties[i].stackSize = 1; continue; }
			if (itemProperties[i].stackSize > gameSettings.player.stackSize) 
			{ itemProperties[i].stackSize = gameSettings.player.stackSize; }
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