using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	public class Player : MonoBehaviour
	{
		public string playerSettingsPath;
		public Transform playerCenter;
		public Armature armature;
		// [HideInInspector] public PlayerSettingsObject playerSettings;
		[HideInInspector] public CameraPerspective cameraPerspective;
		[HideInInspector] public PlayerInventory inventory;
		[HideInInspector] public PostProcessing postProcessing;
		[HideInInspector] public ChatController chat;
		[HideInInspector] public uint color = 0xffffffff; // rrggbbaa
		[HideInInspector] public string id = "default-player";
		[HideInInspector] public float level = 0f;
		[HideInInspector] public float xp = 0f;
		[HideInInspector] public int health = 9;
		[HideInInspector] public int food = 9;
		public bool canUseCommands { private set; get; }
		public bool IsGrounded { get { return !movementController.CanApplyGravity(GameSettings.world.gravity); } }
		public static Player instance;

		private System.Random random;
		private MovementController movementController;
		private Vector3 gravity = Vector3.zero;
		private VoxelHit voxelHit;
		private float jumpedHeight = 0f;
		private bool isJumping = false;

		public void DropItem(Item item, Vector3? position = null) 
		{
			if (item.IsEmpty) { return; }

			DroppedItem obj = new DroppedItem(item, (position == null) ? armature.head.transform.position : (Vector3)position, position == null);
			obj.gameObject.transform.eulerAngles = new Vector3(0f, (float)random.Next(0, 180), 0f);

			if (position == null) { obj.body.AddForce(-armature.head.transform.right * 5f, ForceMode.Impulse); }

			obj.GenerateMesh();
			obj.Update();
		}

		void Awake() { Player.Initialize(this); }
		public static void Initialize(Player instance) 
		{
			Player.instance = instance;
			// Player.instance.playerSettings = playerSettings;
			PlayerSettings.Load(instance.playerSettingsPath);
		}

		void Start() 
		{
			chat = gameObject.GetComponent<ChatController>();
			postProcessing = gameObject.GetComponentsInChildren<PostProcessing>()[0];

			random = new System.Random((int)gameObject.name.GetHashCode());
			inventory = gameObject.GetComponent<PlayerInventory>();
			id = gameObject.name + "-player";
			color = 0x007f7fff; // rrggbbaa
			canUseCommands = true;

			Material material = GameSettings.materials.player;
			Texture2D texture = new Texture2D(516, 258);

			try { ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Objects/player/Skins/" + id + ".png"), false); }
			catch { ImageConversion.LoadImage(texture, File.ReadAllBytes(GameManager.FormatPath("$(DefaultTextures)/skins/default.png")), false); }
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;

			((SkinnedMeshRenderer)gameObject.GetComponentInChildren(typeof(SkinnedMeshRenderer))).material = material;
			material.SetTexture("_MainTex", texture);

			movementController = gameObject.GetComponent<MovementController>();
			movementController.Initialize(playerCenter, Vector3.zero);
		}

		void Update() 
		{
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.TogglePerspective)) 
			{ cameraPerspective = ++cameraPerspective; if ((int)cameraPerspective >= 3) { cameraPerspective = 0; } }

			if (!chat.IsOpen && Input.GetKeyDown(PlayerSettings.controlls.keyCodes.ToggleInventory)) 
			{ inventory.Toggle(); }

			if (inventory.IsOpen && Input.GetKeyDown(KeyCode.Escape)) 
			{ inventory.Toggle(); }

			if (!inventory.IsOpen && !chat.IsOpen && Input.GetKeyDown(PlayerSettings.controlls.keyCodes.OpenChat)) 
			{ chat.IsOpen = true; }

			if (chat.IsOpen && Input.GetKeyDown(KeyCode.Escape)) 
			{ chat.IsOpen = false; }

			int mouseButtons = 0 | 
			((Input.GetKeyDown(PlayerSettings.controlls.keyCodes.Attack)) ? 1 : 0) << 0 | 
			((Input.GetKeyDown(PlayerSettings.controlls.keyCodes.UseItem)) ? 1 : 0) << 1 | 
			((Input.GetKeyDown(PlayerSettings.controlls.keyCodes.PickBlock)) ? 1 : 0) << 2;

			if (Tools.GetAnyBit(mouseButtons)) 
			{
				if (VoxelHit.Check(armature.head.transform.position, -armature.head.transform.right * GameSettings.player.reachingDistance, this, out voxelHit) && !inventory.IsOpen && !chat.IsOpen) 
				{
					if (mouseButtons >> 0 == 1) { voxelHit.chunk.OnPlayerRemoveVoxel(this, voxelHit); }
					if (mouseButtons >> 2 == 1) { voxelHit.chunk.OnPlayerPickVoxel(this, voxelHit); }
					if (mouseButtons >> 1 == 1) { voxelHit.chunk.OnPlayerPlaceVoxel(this, voxelHit); }
				}
			}


			Cursor.lockState = (inventory.IsOpen || chat.IsOpen) ? CursorLockMode.None : CursorLockMode.Locked;
			Cursor.visible = inventory.IsOpen || chat.IsOpen;
		}

		void FixedUpdate() 
		{
			movementController.ApplyGravity(GameSettings.world.gravity);
			float t = movementController.t;

			if (inventory.IsOpen || chat.IsOpen) { return; }

			float v = 0f;
			if (Input.GetKey(PlayerSettings.controlls.keyCodes.Sneak)) { v = GameSettings.player.sneakingSpeed; }
			else if (Input.GetKey(PlayerSettings.controlls.keyCodes.Sprint)) { v = GameSettings.player.sprintingSpeed; }
			else { v = GameSettings.player.walkingSpeed; }

			if (Input.GetKey(PlayerSettings.controlls.keyCodes.Jump) && IsGrounded) { isJumping = true; jumpedHeight = 0f; }
			// if (Input.GetKeyUp(PlayerSettings.controlls.keyCodes.Jump)) { isJumping = false; jumpedHeight = 0f; }

			if (isJumping && jumpedHeight < GameSettings.player.jumpHeight) 
			{
				Vector3 movement = gameObject.transform.up * GameSettings.player.jumpSpeed * t;
				if (movementController.Move(movement)) { jumpedHeight += Math2.Length(movement); }
				else { isJumping = false; jumpedHeight = 0f; }
			}

			if (Input.GetKey(PlayerSettings.controlls.keyCodes.MoveForward)) { movementController.Move(gameObject.transform.forward * v * t); }
			if (Input.GetKey(PlayerSettings.controlls.keyCodes.MoveBackwards)) { movementController.Move(-gameObject.transform.forward * v * t); }
			if (Input.GetKey(PlayerSettings.controlls.keyCodes.MoveLeft)) { movementController.Move(-gameObject.transform.right * v * t); }
			if (Input.GetKey(PlayerSettings.controlls.keyCodes.MoveRight)) { movementController.Move(gameObject.transform.right * v * t); }
		}

		void OnTriggerEnter(Collider collider) 
		{
			switch (collider.tag) 
			{
				case "Liquid":
					postProcessing.SetTextureEffect(GameManager.textureEffects["underwater"]);
					break;
				case "DroppedItem":
					collider.name = collider.name.Replace(":", "");
					Item item = new Item(collider.name.Split()[0], Convert.ToUInt32(collider.name.Split()[1]));

					item.ammount = inventory.TryAddItem(item);
					if (item.ammount == 0) { Destroy(collider.gameObject); return; }

					collider.name = item.ToString();
					break;
				default:
					postProcessing.RemoveTextureEffect();
					break;
			}
		}

		void OnTriggerExit(Collider collider) { if (collider.tag == "Liquid") { postProcessing.RemoveTextureEffect(); } }

		public string ToJson() 
		{
			SavedData data = new SavedData(gameObject.name, health, food, gameObject.transform.position, gameObject.transform.eulerAngles, inventory.slots.Select(x => x.item).ToArray());
			string json = JsonUtility.ToJson(data);
			json.Remove(json.IndexOf("{"), 1);
			json.Remove(json.LastIndexOf("}"), 1);
			return json;
		}

		public static void Save(string[] jsons) 
		{
			string output = "{\n\t\"content\":\n\t[\n";
			for (int i = 0; i < jsons.Length; i++) { output += "\t\t" + jsons[i] + ((i < jsons.Length - 1) ? ",\n" : "\n"); }
			output += "\t]\n}";
			File.WriteAllText(GameManager.FormatPath(GameSettings.path.savedPlayers), output);
		}

		public static void Load() 
		{
			throw new System.NotImplementedException();
		}

		[System.Serializable]
		public struct SavedData 
		{
			public string name;
			public int health;
			public int food;
			public Vector3 position;
			public Vector3 eulerAngles;
			public Item[] inventoryItems;

			public SavedData(string name, int health, int food, Vector3 position, Vector3 eulerAngles, Item[] inventoryItems) 
			{
				this.name = name;
				this.food = food;
				this.health = health;
				this.position = position;
				this.eulerAngles = eulerAngles;
				this.inventoryItems = inventoryItems;
				/* PLAYER EFFECTS */
			}
		}
	}
}