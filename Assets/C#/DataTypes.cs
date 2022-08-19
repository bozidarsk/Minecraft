using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;

public class DroppedItem 
{
	public GameObject gameObject { get; }
	public int vertexCount { get { return vertices.Count; } }
	public Item item { set; get; }

	public MeshRenderer renderer { set; get; }
	public MeshFilter filter { set; get; }
	public MeshCollider collider { set; get; }
	public Rigidbody body { set; get; }

	private List<Vector3> vertices;
	private List<int> triangles;
	private List<Vector2> uvs;

	public void Clear() 
	{
		vertices = new List<Vector3>();
		triangles = new List<int>();
		uvs = new List<Vector2>();
		Update();
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
		this.gameObject.transform.localScale = Vector3.one;
		((DroppedItemController)this.gameObject.AddComponent(typeof(DroppedItemController))).useCooldown = useCooldown;
		this.renderer = (MeshRenderer)this.gameObject.AddComponent(typeof(MeshRenderer));
		this.filter = (MeshFilter)this.gameObject.AddComponent(typeof(MeshFilter));
		this.collider = (MeshCollider)this.gameObject.AddComponent(typeof(MeshCollider));

		GameObject colliderObject = new GameObject("collider");
		colliderObject.transform.SetParent(this.gameObject.transform);
		colliderObject.transform.localPosition = Vector3.zero;
		colliderObject.transform.eulerAngles = Vector3.zero;
		colliderObject.transform.localScale = Vector3.one;
		colliderObject.layer = 14;
		SphereCollider sphere = (SphereCollider)colliderObject.AddComponent(typeof(SphereCollider));
		sphere.center = new Vector3(0f, -0.707f, 0f);
		sphere.radius = 0.2f;

		this.body = ((Rigidbody)this.gameObject.AddComponent(typeof(Rigidbody)));
		this.body.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

		this.renderer.material = new Material(Shader.Find("Custom/DroppedItem"));

		this.vertices = new List<Vector3>();
		this.triangles = new List<int>();
		this.uvs = new List<Vector2>();
	}
}

public class ChunkMesh 
{
	public GameObject gameObject { get; }
	public int vertexCount { get { return vertices.Count; } }
	public string name { set { gameObject.name = value; } get { return gameObject.name; } }
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
			return mesh;
		}
	}

	public MeshRenderer renderer { set; get; }
	public MeshFilter filter { set; get; }
	public MeshCollider collider { set; get; }

	public List<Vector3> vertices;
	public List<int> triangles;
	public List<Vector2> uvs;

	public void Clear() 
	{
		vertices = new List<Vector3>();
		triangles = new List<int>();
		uvs = new List<Vector2>();
		Update();
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

	public void Update() 
	{
		Mesh mesh = this.mesh;
		mesh.Optimize();
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
		this.renderer = (MeshRenderer)this.gameObject.AddComponent(typeof(MeshRenderer));
		this.filter = (MeshFilter)this.gameObject.AddComponent(typeof(MeshFilter));
		this.collider = (MeshCollider)this.gameObject.AddComponent(typeof(MeshCollider));

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
		text.SetText((!item.IsEmpty && item.ammount > 1) ? Convert.ToString(item.ammount) : "");

		Vector2byte coords = inventory.gameManager.itemProperties[inventory.gameManager.GetItemTypeById(item.id)].textureCoords;
		float coordsy = ((float)inventory.gameManager.itemTextures.height / 16f) - (float)coords.y - 1;
		float uvx = (16f * (float)coords.x) / (float)inventory.gameManager.itemTextures.width;
		float uvy = (16f * coordsy) / (float)inventory.gameManager.itemTextures.height;
		float uvsizex = 16f / (float)inventory.gameManager.itemTextures.width;
		float uvsizey = 16f / (float)inventory.gameManager.itemTextures.height;

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

public class Item 
{
	public string id { set; get; }
	public uint ammount { set; get; }

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
public struct PropertyArray<T> 
{ public T[] properties; }

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
public struct Vector3bool 
{
	public bool x;
	public bool y;
	public bool z;
}

[Serializable]
public struct VoxelProperty 
{
	public string id;
	public string dropItem;
	public string preferedTool;
	public int dropMultiplier;
	public int miningForce;
	public float miningSpeed;
	public float speedMultiplier;
	public float xp;
	public bool useCollision;
	public bool indestructible;
	public Vector3bool enableRotation;
	public bool[] usingFullFace;
	public Vector2byte[] textureCoords;
}

[Serializable]
public struct ToolProperty 
{
	public float damage;
	public float durability;
	public float miningSpeed;
	public float miningForce;
}

[Serializable]
public struct ArmourProperty 
{
	public float protection;
	public float blastProtection;
	public float projectileProtection;
}

[Serializable]
public struct Enchantment 
{
	public string id;
	public uint level;
}

[Serializable]
public struct ItemProperty 
{
	public string id;
	public string description;
	public Vector2 descriptionSize;
	public Vector2byte textureCoords;
	public uint stackSize;
	public string[] allowedEnchantments;
	public Enchantment[] enchantments;
	public ToolProperty toolProperty;
	public ArmourProperty armourProperty;
}

[Serializable]
public struct EnchantmentProperty 
{
	public string id;
	public string shader;
	public uint maxLevel;
	public string[] incompatibleEnchantments;
}