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
		public PlayerSettingsObject playerSettings;
		public Transform playerCenter;
		public Armature armature;
		[HideInInspector] public PlayerInventory inventory;
		[HideInInspector] public CameraPerspective cameraPerspective;
		[HideInInspector] public uint color; // rrggbbaa
		[HideInInspector] public string id;
		[HideInInspector] public float level;
		[HideInInspector] public float xp;
		[HideInInspector] public ChatController chat;
		[HideInInspector] public PostProcessing postProcessing;
		new public string name { get { return gameObject.name; } }

		private System.Random random;
		private MovementController movementController;
		private Vector3 gravity = Vector3.zero;
		private VoxelHit voxelHit;
		private RaycastHit raycastHit;
		private float jumpedHeight = 0f;
		private bool isJumping = false;

		public void DropItem(Item item, Vector3? position = null) 
		{
			if (item.IsEmpty) { return; }

			DroppedItem obj = new DroppedItem(item, (position == null) ? armature.head.transform.position : (Vector3)position, position == null);
			Vector3 offset = new Vector3(-0.5f, -0.5f, -0.5f);
			obj.gameObject.transform.eulerAngles = new Vector3(0f, (float)random.Next(0, 180), 0f);

			if (position == null) { obj.body.AddForce(-armature.head.transform.right * 5f, ForceMode.Impulse); }

			if (item.id.EndsWith("-block")) 
			{
				VoxelProperty property = GameManager.voxelProperties[GameManager.GetVoxelTypeById(item.id)];
				for (int f = 0; f < Chunk.blockVertices.GetLength(0); f++) 
				{
					for (int v = 0; v < Chunk.blockVertices.GetLength(1); v++) 
					{ obj.Add(Chunk.blockVertices[f, v] + offset); }

					int index = obj.vertexCount - 4;
					obj.Add(index + 0, index + 3, index + 1, index + 1, index + 3, index + 2);

					Vector2byte coords = property.textureCoords[f];
					float coordsy = ((float)GameSettings.textures.voxel.height / 16f) - (float)coords.y - 1;
					float uvx = (16f * (float)coords.x) / (float)GameSettings.textures.voxel.width;
					float uvy = (16f * coordsy) / (float)GameSettings.textures.voxel.height;
					float uvsizex = 16f / (float)GameSettings.textures.voxel.width;
					float uvsizey = 16f / (float)GameSettings.textures.voxel.height;

					obj.Add(
						new Vector2(uvx, uvy),
						new Vector2(uvx + uvsizex, uvy),
						new Vector2(uvx + uvsizex, uvy + uvsizey),
						new Vector2(uvx, uvy + uvsizey)
					);
				}

				obj.renderer.material.SetTexture("_MainTex", GameSettings.textures.voxel);
			}

			if (item.id.EndsWith("-model")) 
			{
				obj.Add(GameManager.modelMeshes.Where(x => x.Key == item.id).ToArray()[0].Value);
				obj.renderer.material.SetTexture("_MainTex", GameSettings.textures.voxel);
			}

			if (obj.vertexCount == 0) 
			{
				obj.Add(
					new Vector3(-0.5f, -0.5f, 0f),
					new Vector3(0.5f, -0.5f, 0f),
					new Vector3(0.5f, 0.5f, 0f),
					new Vector3(-0.5f, 0.5f, 0f)
				);

				obj.Add(0, 1, 2, 2, 3, 0);

				Vector2byte coords = GameManager.itemProperties[GameManager.GetItemTypeById(item.id + "-item")].textureCoords;
				float coordsy = ((float)GameSettings.textures.item.height / 16f) - (float)coords.y - 1;
				float uvx = (16f * (float)coords.x) / (float)GameSettings.textures.item.width;
				float uvy = (16f * coordsy) / (float)GameSettings.textures.item.height;
				float uvsizex = 16f / (float)GameSettings.textures.item.width;
				float uvsizey = 16f / (float)GameSettings.textures.item.height;

				obj.Add(
					new Vector2(uvx, uvy),
					new Vector2(uvx + uvsizex, uvy),
					new Vector2(uvx + uvsizex, uvy + uvsizey),
					new Vector2(uvx, uvy + uvsizey)
				);

				obj.renderer.material.SetTexture("_MainTex", GameSettings.textures.item);
			}

			obj.Update();
		}

		void Start() 
		{
			chat = gameObject.GetComponent<ChatController>();
			postProcessing = gameObject.GetComponentsInChildren<PostProcessing>()[0];

			random = new System.Random((int)gameObject.name.GetHashCode());
			inventory = gameObject.GetComponent<PlayerInventory>();
			id = gameObject.name + "-player";
			color = 0x007f7fff; // rrggbbaa

			Material material = GameSettings.materials.player;
			Texture2D texture = new Texture2D(516, 258);

			// ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Objects/player/texture-template.png"), false);
			try { ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Objects/player/Textures/" + id + ".png"), false); }
			catch { ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Objects/player/texture-default.png"), false); }
			texture.filterMode = FilterMode.Point;
			texture.wrapMode = TextureWrapMode.Clamp;

			((SkinnedMeshRenderer)gameObject.GetComponentInChildren(typeof(SkinnedMeshRenderer))).material = material;
			material.SetTexture("_MainTex", texture);

			movementController = gameObject.GetComponent<MovementController>();
			movementController.Initialize(playerCenter, Vector3.zero);
		}

		void Update() 
		{
			if (Input.GetKeyDown(playerSettings.controlls.keyCodes.TogglePerspective)) 
			{ cameraPerspective = ++cameraPerspective; if ((int)cameraPerspective >= 3) { cameraPerspective = 0; } }

			if (!chat.IsOpen && Input.GetKeyDown(playerSettings.controlls.keyCodes.ToggleInventory)) 
			{ inventory.Toggle(); }

			if (inventory.IsOpen && Input.GetKeyDown(KeyCode.Escape)) 
			{ inventory.Toggle(); }

			if (!inventory.IsOpen && !chat.IsOpen && Input.GetKeyDown(playerSettings.controlls.keyCodes.OpenChat)) 
			{ chat.IsOpen = true; }

			if (chat.IsOpen && Input.GetKeyDown(KeyCode.Escape)) 
			{ chat.IsOpen = false; }

			int mouseButtons = 0 | 
			((Input.GetKeyDown(playerSettings.controlls.keyCodes.Attack)) ? 1 : 0) << 0 | 
			((Input.GetKeyDown(playerSettings.controlls.keyCodes.UseItem)) ? 1 : 0) << 1 | 
			((Input.GetKeyDown(playerSettings.controlls.keyCodes.PickBlock)) ? 1 : 0) << 2;

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
			if (Input.GetKey(playerSettings.controlls.keyCodes.Sneak)) { v = GameSettings.player.sneakingSpeed; }
			else if (Input.GetKey(playerSettings.controlls.keyCodes.Sprint)) { v = GameSettings.player.sprintingSpeed; }
			else { v = GameSettings.player.walkingSpeed; }

			if (Input.GetKey(playerSettings.controlls.keyCodes.Jump) && IsGrounded) { isJumping = true; jumpedHeight = 0f; }
			// if (Input.GetKeyUp(playerSettings.controlls.keyCodes.Jump)) { isJumping = false; jumpedHeight = 0f; }

			if (isJumping && jumpedHeight < GameSettings.player.jumpHeight) 
			{
				Vector3 movement = gameObject.transform.up * GameSettings.player.jumpSpeed * t;
				if (movementController.Move(movement)) { jumpedHeight += Math2.Length(movement); }
				else { isJumping = false; jumpedHeight = 0f; }
			}

			if (Input.GetKey(playerSettings.controlls.keyCodes.MoveForward)) { movementController.Move(gameObject.transform.forward * v * t); }
			if (Input.GetKey(playerSettings.controlls.keyCodes.MoveBackwards)) { movementController.Move(-gameObject.transform.forward * v * t); }
			if (Input.GetKey(playerSettings.controlls.keyCodes.MoveLeft)) { movementController.Move(-gameObject.transform.right * v * t); }
			if (Input.GetKey(playerSettings.controlls.keyCodes.MoveRight)) { movementController.Move(gameObject.transform.right * v * t); }
		}

		public bool IsGrounded { get { return !movementController.CanApplyGravity(GameSettings.world.gravity); } }

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
	}
}