using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Utils;

[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
	public PlayerSettings playerSettings;
	[HideInInspector] public PlayerInventory inventory;
	[HideInInspector] public GameManager gameManager;
	[HideInInspector] public CameraPerspective cameraPerspective;
	[HideInInspector] public Armature armature;
	[HideInInspector] public uint color; // rrggbbaa
	[HideInInspector] public string id;
	[HideInInspector] public float level;
	[HideInInspector] public float xp;
	[HideInInspector] public ChatController chat;
	[HideInInspector] public PostProcessing postProcessing;
	new public string name { get { return gameObject.name; } }

	private System.Random random;
	private CharacterController controller;
	private GameObject groundCheck;
	private Vector3 currentGravity = Vector3.zero;
	private RaycastHit hit;
	private bool hasHit = false;

	public void DropItem(Item item, Vector3? position = null) 
	{
		if (item.IsEmpty) { return; }

		DroppedItem obj = new DroppedItem(item, (position == null) ? armature.head.transform.position : (Vector3)position, position == null);
		Vector3 offset = new Vector3(-0.5f, -0.5f, -0.5f);
		obj.gameObject.transform.localScale = Vector3.one * 0.3f;
		obj.gameObject.transform.eulerAngles = new Vector3(0f, (float)random.Next(0, 180), 0f);
		obj.body.angularDrag = 0.02f;

		if (position == null) { obj.body.AddForce(-armature.head.transform.right * 5f, ForceMode.Impulse); }

		if (item.id.EndsWith("-block")) 
		{
			VoxelProperty property = gameManager.voxelProperties[gameManager.GetVoxelTypeById(item.id)];
			for (int f = 0; f < Chunk.blockVertices.GetLength(0); f++) 
			{
				for (int v = 0; v < Chunk.blockVertices.GetLength(1); v++) 
				{ obj.Add(Chunk.blockVertices[f, v] + offset); }

				int index = obj.vertexCount - 4;
				obj.Add(index + 0, index + 3, index + 1, index + 1, index + 3, index + 2);

				Vector2byte coords = property.textureCoords[f];
				float coordsy = ((float)gameManager.voxelTextures.height / 16f) - (float)coords.y - 1;
				float uvx = (16f * (float)coords.x) / (float)gameManager.voxelTextures.width;
				float uvy = (16f * coordsy) / (float)gameManager.voxelTextures.height;
				float uvsizex = 16f / (float)gameManager.voxelTextures.width;
				float uvsizey = 16f / (float)gameManager.voxelTextures.height;

				obj.Add(
					new Vector2(uvx, uvy),
					new Vector2(uvx + uvsizex, uvy),
					new Vector2(uvx + uvsizex, uvy + uvsizey),
					new Vector2(uvx, uvy + uvsizey)
				);
			}

			obj.renderer.material.SetTexture("_MainTex", gameManager.voxelTextures);
		}

		if (item.id.EndsWith("-model")) 
		{
			obj.Add(gameManager.modelMeshes.Where(x => x.Key == item.id).ToArray()[0].Value);
			obj.renderer.material.SetTexture("_MainTex", gameManager.voxelTextures);
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

			Vector2byte coords = gameManager.itemProperties[gameManager.GetItemTypeById(item.id + "-item")].textureCoords;
			float coordsy = ((float)gameManager.itemTextures.height / 16f) - (float)coords.y - 1;
			float uvx = (16f * (float)coords.x) / (float)gameManager.itemTextures.width;
			float uvy = (16f * coordsy) / (float)gameManager.itemTextures.height;
			float uvsizex = 16f / (float)gameManager.itemTextures.width;
			float uvsizey = 16f / (float)gameManager.itemTextures.height;

			obj.Add(
				new Vector2(uvx, uvy),
				new Vector2(uvx + uvsizex, uvy),
				new Vector2(uvx + uvsizex, uvy + uvsizey),
				new Vector2(uvx, uvy + uvsizey)
			);

			obj.renderer.material.SetTexture("_MainTex", gameManager.itemTextures);
		}

		obj.Update();
	}

	void Start() 
	{
		chat = gameObject.GetComponent<ChatController>();
		postProcessing = gameObject.GetComponentsInChildren<PostProcessing>()[0];

		Transform[] transforms = gameObject.GetComponentsInChildren<Transform>();
		armature.chest = transforms.Where(x => x.name == "Chest").ToList()[0].gameObject;
		armature.head = transforms.Where(x => x.name == "Head").ToList()[0].gameObject;
		armature.armL = transforms.Where(x => x.name == "Arm.L").ToList()[0].gameObject;
		armature.armR = transforms.Where(x => x.name == "Arm.R").ToList()[0].gameObject;
		armature.legL = transforms.Where(x => x.name == "Leg.L").ToList()[0].gameObject;
		armature.legR = transforms.Where(x => x.name == "Leg.R").ToList()[0].gameObject;
		groundCheck = transforms.Where(x => x.name == "GroundCheck").ToList()[0].gameObject;

		random = new System.Random((int)gameObject.name.GetHashCode());
		controller = gameObject.GetComponent<CharacterController>();
		inventory = gameObject.GetComponent<PlayerInventory>();
		id = gameObject.name + "-player";
		color = 0x007f7fff; // rrggbbaa

		Material material = new Material(Shader.Find("Custom/Player"));
		Texture2D texture = new Texture2D(516, 258);

		// ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Objects/player/texture-template.png"), false);
		try { ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Objects/player/Textures/" + id + ".png"), false); }
		catch { ImageConversion.LoadImage(texture, File.ReadAllBytes("Assets/Objects/player/texture-default.png"), false); }
		texture.filterMode = FilterMode.Point;

		((SkinnedMeshRenderer)gameObject.GetComponentInChildren(typeof(SkinnedMeshRenderer))).material = material;
		material.SetTexture("_MainTex", texture);

		gameManager = (GameManager)GameObject.FindObjectOfType(typeof(GameManager));
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

		if (hasHit && !inventory.IsOpen && !chat.IsOpen) 
		{
			if (hit.transform.gameObject.tag != "ChunkMesh") { return; }
			ChunkController chunkController = hit.transform.parent.gameObject.GetComponent<ChunkController>();

			if (Input.GetKeyDown(playerSettings.controlls.keyCodes.Attack)) { chunkController.OnPlayerRemoveVoxel(this, hit); }
			if (Input.GetKeyDown(playerSettings.controlls.keyCodes.PickBlock)) { chunkController.OnPlayerPickVoxel(this, hit); }
			if (Input.GetKeyDown(playerSettings.controlls.keyCodes.UseItem)) { chunkController.OnPlayerPlaceVoxel(this, hit); }
		}

		Cursor.lockState = (inventory.IsOpen || chat.IsOpen) ? CursorLockMode.None : CursorLockMode.Locked;
		Cursor.visible = inventory.IsOpen || chat.IsOpen;
	}

	void FixedUpdate() 
	{
		bool isGrounded = Physics.CheckSphere(groundCheck.transform.position, 0.1f, 1 << 10);
		Vector3 move = Vector3.zero;
		Vector3 jump = Vector3.zero;
		float speed = 0f;

		if (isGrounded) { currentGravity = Vector3.zero; }
		else { currentGravity += gameManager.gameSettings.player.gravity; }

		if (inventory.IsOpen || chat.IsOpen) { controller.Move(currentGravity); return; }

		hasHit = Physics.Raycast(armature.head.transform.position, -armature.head.transform.right, out hit, gameManager.gameSettings.player.reachingDistance, (1 << 10) + (1 << 11));

		if (Input.GetKey(playerSettings.controlls.keyCodes.Sneak)) { speed = gameManager.gameSettings.player.sneakingSpeed; }
		else if (Input.GetKey(playerSettings.controlls.keyCodes.Sprint)) { speed = gameManager.gameSettings.player.sprintingSpeed; }
		else { speed = gameManager.gameSettings.player.walkingSpeed; }

		if (Input.GetKey(playerSettings.controlls.keyCodes.MoveForward)) { move += gameObject.transform.forward; }
		if (Input.GetKey(playerSettings.controlls.keyCodes.MoveBackwards)) { move += -gameObject.transform.forward; }
		if (Input.GetKey(playerSettings.controlls.keyCodes.MoveLeft)) { move += -gameObject.transform.right; }
		if (Input.GetKey(playerSettings.controlls.keyCodes.MoveRight)) { move += gameObject.transform.right; }

		if (Input.GetKey(playerSettings.controlls.keyCodes.Jump) && isGrounded) 
		{ jump = Vector3.up * Math2.Sqrt(gameManager.gameSettings.player.jumpHeight * -2f * gameManager.gameSettings.player.gravity.y) - currentGravity; }

		controller.Move(move.normalized * speed + currentGravity + jump);
	}

	void OnTriggerEnter(Collider collider) 
	{
		switch (collider.tag) 
		{
			case "Liquid":
				postProcessing.SetTextureEffect(gameManager.textureEffects["underwater"]);
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