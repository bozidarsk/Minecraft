using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using Utils;
using TMPro;

namespace Minecraft 
{
	public class ObjectMesh 
	{
		public int vertexCount { get { return vertices.Count; } }
		public Mesh mesh 
		{
			get 
			{
				Mesh mesh = new Mesh();
				mesh.indexFormat = IndexFormat.UInt32;
				mesh.vertices = vertices.ToArray();
				mesh.triangles = triangles.ToArray();
				mesh.uv = uvs.ToArray();
				mesh.RecalculateNormals();
				mesh.Optimize();
				return mesh;
			}
		}

		public List<Vector3> vertices;
		public List<int> triangles;
		public List<Vector2> uvs;

		public virtual void Clear() 
		{
			vertices = new List<Vector3>();
			triangles = new List<int>();
			uvs = new List<Vector2>();
		}

		public void Add(params Vector3[] x) { for (int i = 0; i < x.Length; i++) { vertices.Add(x[i]); } }
		public void Add(params int[] x) { for (int i = 0; i < x.Length; i++) { triangles.Add(x[i]); } }
		public void Add(params Vector2[] x) { for (int i = 0; i < x.Length; i++) { uvs.Add(x[i]); } }
		public void Add(params Mesh[] x) 
		{
			for (int i = 0; i < x.Length; i++) 
			{
				Add(x[i].vertices);
				Add(x[i].triangles);
				Add(x[i].uv);
			}
		}

		public ObjectMesh() 
		{
			this.vertices = new List<Vector3>();
			this.triangles = new List<int>();
			this.uvs = new List<Vector2>();
		}
	}

	public class DroppedItem : ObjectMesh
	{
		public GameObject gameObject { get; }
		public Item item { set; get; }

		public MeshRenderer renderer { set; get; }
		public MeshFilter filter { set; get; }
		public MeshCollider collider { set; get; }
		public Rigidbody body { set; get; }

		public override void Clear() 
		{
			vertices = new List<Vector3>();
			triangles = new List<int>();
			uvs = new List<Vector2>();
			Update();
		}

		public void GenerateMesh() 
		{
			if (item.id.EndsWith("-block")) 
			{
				Vector3 offset = Vector3.one / -2f;
				VoxelProperty property = GameManager.voxelProperties[GameManager.GetVoxelTypeById(item.id)];
				for (int f = 0; f < Chunk.blockVertices.GetLength(0); f++) 
				{
					for (int v = 0; v < Chunk.blockVertices.GetLength(1); v++) 
					{ Add(Chunk.blockVertices[f, v] + offset); }

					int index = vertexCount - 4;
					Add(index + 0, index + 3, index + 1, index + 1, index + 3, index + 2);

					Vector2byte coords = property.textureCoords[f];
					float coordsy = ((float)GameSettings.textures.voxel.height / 16f) - (float)coords.y - 1;
					float uvx = (16f * (float)coords.x) / (float)GameSettings.textures.voxel.width;
					float uvy = (16f * coordsy) / (float)GameSettings.textures.voxel.height;
					float uvsizex = 16f / (float)GameSettings.textures.voxel.width;
					float uvsizey = 16f / (float)GameSettings.textures.voxel.height;

					Add(
						new Vector2(uvx, uvy),
						new Vector2(uvx + uvsizex, uvy),
						new Vector2(uvx + uvsizex, uvy + uvsizey),
						new Vector2(uvx, uvy + uvsizey)
					);
				}

				renderer.material.SetTexture("_MainTex", GameSettings.textures.voxel);
			}

			if (item.id.EndsWith("-model")) 
			{
				Add(GameManager.modelMeshes.Where(x => x.Key == item.id).ToArray()[0].Value);
				renderer.material.SetTexture("_MainTex", GameSettings.textures.voxel);
			}

			if (vertexCount == 0) 
			{
				Add(
					new Vector3(-0.5f, -0.5f, 0f),
					new Vector3(0.5f, -0.5f, 0f),
					new Vector3(0.5f, 0.5f, 0f),
					new Vector3(-0.5f, 0.5f, 0f)
				);

				Add(0, 1, 2, 2, 3, 0);

				Vector2byte coords = GameManager.itemProperties[GameManager.GetItemTypeById(item.id + "-item")].textureCoords;
				float coordsy = ((float)GameSettings.textures.item.height / 16f) - (float)coords.y - 1;
				float uvx = (16f * (float)coords.x) / (float)GameSettings.textures.item.width;
				float uvy = (16f * coordsy) / (float)GameSettings.textures.item.height;
				float uvsizex = 16f / (float)GameSettings.textures.item.width;
				float uvsizey = 16f / (float)GameSettings.textures.item.height;

				Add(
					new Vector2(uvx, uvy),
					new Vector2(uvx + uvsizex, uvy),
					new Vector2(uvx + uvsizex, uvy + uvsizey),
					new Vector2(uvx, uvy + uvsizey)
				);

				renderer.material.SetTexture("_MainTex", GameSettings.textures.item);
			}
		}

		public void Update() 
		{
			Mesh mesh = new Mesh();
			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.RecalculateNormals();
			filter.mesh = mesh;
			collider.sharedMesh = mesh;
			collider.convex = true;
		}

		public DroppedItem(Item item, Vector3 position, bool useCooldown) 
		{
			this.item = item;
			this.gameObject = new GameObject(item.ToString());
			this.gameObject.name = item.ToString();
			this.gameObject.tag = "DroppedItem";
			this.gameObject.layer = 13;
			this.gameObject.transform.position = position;
			this.gameObject.transform.eulerAngles = Vector3.zero;
			this.gameObject.transform.localScale = Vector3.one * 0.3f;

			DroppedItemController controller = (DroppedItemController)this.gameObject.AddComponent<DroppedItemController>();
			controller.useCooldown = useCooldown;
			controller.droppedItem = this;
			controller.movementController = (MovementController)this.gameObject.AddComponent<MovementController>();
			controller.movementController.Initialize(
				this.gameObject.transform,
				-GameSettings.world.gravity.normalized * (0.3f * 0.5f)
			);

			this.renderer = (MeshRenderer)this.gameObject.AddComponent<MeshRenderer>();
			this.filter = (MeshFilter)this.gameObject.AddComponent<MeshFilter>();
			this.collider = (MeshCollider)this.gameObject.AddComponent<MeshCollider>();

			GameObject colliderObject = new GameObject("collider");
			colliderObject.transform.SetParent(this.gameObject.transform);
			colliderObject.transform.localPosition = Vector3.zero;
			colliderObject.transform.eulerAngles = Vector3.zero;
			colliderObject.transform.localScale = Vector3.one;
			colliderObject.layer = 14;
			SphereCollider sphere = (SphereCollider)colliderObject.AddComponent<SphereCollider>();
			sphere.center = new Vector3(0f, -0.707f, 0f);
			sphere.radius = 0.2f;

			this.body = ((Rigidbody)this.gameObject.AddComponent<Rigidbody>());
			this.body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
			this.body.isKinematic = true;
			this.body.angularDrag = 0.02f;

			this.renderer.material = GameSettings.materials.droppedItem;

			this.vertices = new List<Vector3>();
			this.triangles = new List<int>();
			this.uvs = new List<Vector2>();
		}

		public string ToJson() 
		{
			SavedData data = new SavedData(item, gameObject.transform.position);
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
			File.WriteAllText(GameManager.FormatPath(GameSettings.path.savedDroppedItems), output);
		}

		public static void Load() 
		{
			SavedData[] data = JsonUtility.FromJson<ArrayWrapper<SavedData>>(File.ReadAllText(GameSettings.path.savedDroppedItems)).content;
			for (int i = 0; i < data.Length; i++) 
			{
				DroppedItem obj = new DroppedItem(data[i].item, data[i].position, true);
				obj.gameObject.transform.eulerAngles = new Vector3(0f, (float)GameManager.random.Next(0, 180), 0f);
				obj.GenerateMesh();
				obj.Update();
			}
		}

		[Serializable]
		public struct SavedData 
		{
			public Item item;
			public Vector3 position;

			public SavedData(Item item, Vector3 position) 
			{
				this.item = item;
				this.position = position;
			}
		}
	}

	public class ChunkMesh : ObjectMesh
	{
		public GameObject gameObject { get; }
		public string name { set { gameObject.name = value; } get { return gameObject.name; } }

		public MeshRenderer renderer { set; get; }
		public MeshFilter filter { set; get; }
		public MeshCollider collider { set; get; }

		public override void Clear() 
		{
			vertices = new List<Vector3>();
			triangles = new List<int>();
			uvs = new List<Vector2>();
			Update();
		}

		public void Update() 
		{
			Mesh mesh = this.mesh;
			filter.mesh = mesh;
			collider.sharedMesh = mesh;
		}

		public ChunkMesh(Transform parent, string name, int layer) 
		{
			this.gameObject = new GameObject(name);
			this.gameObject.tag = "ChunkMesh";
			this.gameObject.layer = layer;
			this.gameObject.transform.SetParent(parent);
			this.gameObject.transform.position = parent.position;
			this.gameObject.transform.eulerAngles = Vector3.zero;
			this.gameObject.transform.localScale = Vector3.one;
			this.renderer = (MeshRenderer)this.gameObject.AddComponent<MeshRenderer>();
			this.filter = (MeshFilter)this.gameObject.AddComponent<MeshFilter>();
			this.collider = (MeshCollider)this.gameObject.AddComponent<MeshCollider>();

			this.vertices = new List<Vector3>();
			this.triangles = new List<int>();
			this.uvs = new List<Vector2>();
		}
	}

	public class InventorySlot 
	{
		public Item item { set; get; }
		public string name { set; get; }
		public string opositeSlot { set; get; }
		public GameObject gameObject { get; }

		private PlayerInventory inventory;
		private List<string> filters;
		private TextMeshProUGUI text;
		private RawImage icon;
		private int index;

		public void Update() 
		{
			if (item.ammount == 0) { item.id = "air-block"; }
			text.SetText((!item.IsEmpty && item.ammount > 1) ? Convert.ToString(item.ammount) : "");

			Vector2byte coords = GameManager.itemProperties[GameManager.GetItemTypeById(item.id)].textureCoords;
			float coordsy = ((float)GameSettings.textures.item.height / 16f) - (float)coords.y - 1;
			float uvx = (16f * (float)coords.x) / (float)GameSettings.textures.item.width;
			float uvy = (16f * coordsy) / (float)GameSettings.textures.item.height;
			float uvsizex = 16f / (float)GameSettings.textures.item.width;
			float uvsizey = 16f / (float)GameSettings.textures.item.height;

			icon.uvRect = new Rect(uvx, uvy, uvsizex, uvsizey);
		}

		public void ClearFilters() { filters = null; }
		public bool IsInFilter(string id) 
		{
			if (filters == null) { return true; }
			for (int i = 0; i < filters.Count; i++) { if (filters[i].Contains(id)) { return true; } }
			return false;
		}

		public void AddFilters(params string[] id) 
		{
			if (filters == null) { filters = new List<string>(); }
			for (int i = 0; i < id.Length; i++) { filters.Add(id[i]); }
		}

		public InventorySlot(GameObject obj, string name, string opositeSlot, PlayerInventory inventory, params string[] filters) 
		{
			this.name = name;
			this.opositeSlot = opositeSlot;
			this.inventory = inventory;
			this.item = new Item("air-block", 1);
			this.filters = (filters == null) ? null : filters.ToList();
			this.text = obj.GetComponentsInChildren<TextMeshProUGUI>()[0];
			this.icon = obj.GetComponentsInChildren<RawImage>()[1];
			this.gameObject = obj;

			if (obj.TryGetComponent(typeof(SlotController), out Component controller)) 
			{
				this.index = this.inventory.buttonIndex++;
				((SlotController)controller).index = this.index;
				((SlotController)controller).onLeftClick += inventory.SlotLeftClicked;
				((SlotController)controller).onRightClick += inventory.SlotRightClicked;
				// ((SlotController)controller).onDoubleClick += inventory.SlotDoubleClicked;
				((SlotController)controller).onHighlight += inventory.SlotHighlighted;
			}

			this.Update();
		}
	}

	[Serializable]
	public class Item 
	{
		public string id;
		public uint ammount;

		public override string ToString() { return id + ": " + Convert.ToString(ammount); }
		public override int GetHashCode() { return (int)id.GetHashCode() + (int)ammount; }
		public bool IsEmpty { get { { return id == null || id == "air-block" || id == "air-item" || id == "" || ammount <= 0; } } }

		public Item(string id, uint ammount) 
		{
			this.id = id;
			this.ammount = ammount;
		}
	}

	public class Command 
	{	
		public string args { get; }
		public string name { get; }
		public Type[] argTypes { get; }
		public Action<dynamic[]> Execute { get; }

		public Command(string name, Action<dynamic[]> function, string[] argNames, Type[] argTypes) 
		{
			this.name = name;
			this.argTypes = argTypes;
			this.Execute = function;

			if (argNames.Length != argTypes.Length) { throw new ArgumentException(); }

			for (int i = 0; i < argNames.Length; i++) 
			{
				this.args += argNames[i] + "(" + Commands.TypeToString(argTypes[i]) + ")";
				this.args += (i < argNames.Length - 1) ? ", " : "";
			}
		}
	}

	public class VoxelHit 
	{
		public Vector3 point { private set; get; }
		public Vector3 normal { private set; get; }
		public float distance { private set; get; }
		public Player player { private set; get; }
		public Chunk chunk { private set; get; }
		public VoxelFace face { private set; get; }
		public VoxelProperty property { private set; get; }
		public VoxelHit previousHit { private set; get; }

		public static bool Check(Vector3 origin, Vector3 direction, Player player, out VoxelHit voxelHit) 
		{
			voxelHit = new VoxelHit();
			voxelHit.previousHit = new VoxelHit();
			voxelHit.player = player;
			voxelHit.previousHit.player = player;

			Vector3 dir = direction * 0.1f;
			Vector3 point = dir;

			for (float step = 1f; Math2.Length(point) < Math2.Length(direction); step += 1f) 
			{
				voxelHit.chunk = TerrainManager.GetChunkFromPosition(origin + point);
				if (voxelHit.chunk == null) { point = dir * step; continue; }

				VoxelProperty property = GameManager.voxelProperties[voxelHit.chunk.GetVoxelTypeFromPoint(origin + point)];

				if (property.id != "air-block" && !property.id.EndsWith("-liquid")) 
				{
					voxelHit.point = origin + point;
					voxelHit.normal = -direction.normalized;
					voxelHit.distance = Math2.Length(point);
					voxelHit.property = property;
					voxelHit.face = Chunk.GetFaceFromNormal(voxelHit.normal);
					return true;
				}
				else 
				{
					voxelHit.previousHit.chunk = voxelHit.chunk;
					voxelHit.previousHit.point = origin + point;
					voxelHit.previousHit.normal = -direction.normalized;
					voxelHit.previousHit.distance = Math2.Length(point);
					voxelHit.previousHit.property = property;
					voxelHit.previousHit.face = Chunk.GetFaceFromNormal(voxelHit.previousHit.normal);
					voxelHit.previousHit.previousHit = null;
				}

				point = dir * step;
			}

			voxelHit = null;
			return false;
		}

		private VoxelHit() {}
		public VoxelHit(Vector3 point, Vector3 normal, float distance, Player player, Chunk chunk, VoxelProperty property, VoxelFace face, VoxelHit previousHit) 
		{
			this.point = point;
			this.normal = normal;
			this.distance = distance;
			this.player = player;
			this.chunk = chunk;
			this.property = property;
			this.face = face;
			this.previousHit = previousHit;
		}
	}

	public class ErrorException : Exception 
	{
		public ErrorException(string message) : base(message) {}
	}

	public enum Cull 
	{
		Off,
		Back,
		Front,
	}

	public enum VoxelFace 
	{
		Up,
		Down,
		Left,
		Right,
		Forward,
		Back
	}

	public enum CameraPerspective 
	{
		FirstPerson,
		Back,
		Front
	}

	[Serializable]
	public struct ArrayWrapper<T> 
	{
		public T[] content;
		public ArrayWrapper(T[] content) { this.content = content; }
	}

	// public struct StringStruct 
	// {
	// 	public char[] id;
	// 	public StringStruct(string id) { this.id = id.ToCharArray(); }
	// }

	public unsafe struct SecurityAttributes 
	{
		public uint nLength;
		public void* lpSecurityDescriptor;
		public bool bInheritHandle;
	}

	public struct COORD 
	{
		public short x;
		public short y;
	}

	[Serializable]
	public struct Armature 
	{
		public GameObject chest;
		public GameObject head;
		public GameObject armL;
		public GameObject armR;
		public GameObject legL;
		public GameObject legR;
	}

	[Serializable]
	public struct Vector2byte 
	{
		public byte x;
		public byte y;
	}

	[Serializable]
	public struct Vector2bool 
	{
		public bool x;
		public bool y;

		public Vector2bool(bool x, bool y) 
		{
			this.x = x;
			this.y = y;
		}
	}

	[Serializable]
	public struct Range 
	{
		[Range(0f, 1f)] public float min;
		[Range(0f, 1f)] public float max;
	}

	[Serializable]
	public struct RangeInt 
	{
		public int min;
		public int max;
	}

	[Serializable]
	public struct RangeUInt 
	{
		public uint min;
		public uint max;
	}

	[Serializable]
	public struct Vector3bool 
	{
		public bool x;
		public bool y;
		public bool z;
	}

	[Serializable]
	public struct Enchantment 
	{
		public string id;
		public uint level;

		public override string ToString() { return id + ": " + Convert.ToString(level); }
	}
}