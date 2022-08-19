using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class Chunk 
{
	public GameObject gameObject { get; }
	public Vector3 position { get { return gameObject.transform.position; } }

	private GameManager gameManager;
	private ChunkController controller;
	private ushort[,,] voxelTypes;
	private int[,,][] voxelTriangles;

	private ChunkMesh withCollision;
	private ChunkMesh withoutCollision;
	private List<ChunkMesh> liquidMeshes;

	private void GenerateVoxels() 
	{
		int h = 3;

		for (int y = 0; y < voxelTypes.GetLength(1); y++) 
		{
			for (int z = 0; z < voxelTypes.GetLength(2); z++) 
			{
				for (int x = 0; x < voxelTypes.GetLength(0); x++) 
				{
					if (y < h) 
					{
						ushort type = 
						(y < h - gameManager.gameSettings.terrain.dirtDepth)
						? gameManager.GetVoxelTypeById("stone-block")
						: gameManager.GetVoxelTypeById("dirt-block");
						SetVoxelType(type, x, y, z);
					}
					else { SetVoxelType(gameManager.GetVoxelTypeById("air-block"), x, y, z); }

					if (GetVoxelType(x, y - 1, z) == gameManager.GetVoxelTypeById("dirt-block") && 
						(GetVoxelType(x, y, z) == gameManager.GetVoxelTypeById("air-block"))
					) { SetVoxelType(gameManager.GetVoxelTypeById("grass-block"), x, y - 1, z); }

					// if (GetVoxelType(x, y - 1, z) == gameManager.GetVoxelTypeById("grass-block")) 
					// { GetVoxelType(x, y, z) = gameManager.GetVoxelTypeById("grass-quads"); }

					if (GetVoxelType(x, y - 1, z) == gameManager.GetVoxelTypeById("grass-block") && 
						x > 5 && x < 10 && z > 5 && z < 10
					) { SetVoxelType(gameManager.GetVoxelTypeById("water-liquid"), x, y - 1, z); }
				}
			}
		}
	}

	private bool CanDrawFace(int x, int y, int z, VoxelFace face) 
	{
		bool isThisALiquid = gameManager.voxelProperties[GetVoxelType(x, y, z)].id.EndsWith("-liquid");
		AddFaceToPosition(ref x, ref y, ref z, face);

		if (x < 0 || x >= voxelTypes.GetLength(0) || 
			y < 0 || y >= voxelTypes.GetLength(1) || 
			z < 0 || z >= voxelTypes.GetLength(2) || 
			(!isThisALiquid && gameManager.voxelProperties[GetVoxelType(x, y, z)].id.EndsWith("-liquid"))
		) { return true; }

		VoxelProperty property = gameManager.voxelProperties[GetVoxelType(x, y, z)];
		return !property.usingFullFace[(int)OpositeFace(face)];
	}

	private void DrawBlock(Vector3 offset, VoxelProperty property) 
	{
		List<int> triangles = new List<int>(12);
		for (int face = 0; face < blockVertices.GetLength(0); face++)
		{
			if (!CanDrawFace((int)offset.x, (int)offset.y, (int)offset.z, (VoxelFace)face)) { continue; }

			for (int v = 0; v < blockVertices.GetLength(1); v++) 
			{ ((property.useCollision) ? withCollision : withoutCollision).Add(blockVertices[face, v] + offset); }

			Vector2byte coords = property.textureCoords[face];
			float coordsy = ((float)gameManager.voxelTextures.height / 16f) - (float)coords.y - 1;
			float uvx = (16f * (float)coords.x) / (float)gameManager.voxelTextures.width;
			float uvy = (16f * coordsy) / (float)gameManager.voxelTextures.height;
			float uvsizex = 16f / (float)gameManager.voxelTextures.width;
			float uvsizey = 16f / (float)gameManager.voxelTextures.height;

			((property.useCollision) ? withCollision : withoutCollision).Add(
				new Vector2(uvx, uvy),
				new Vector2(uvx + uvsizex, uvy),
				new Vector2(uvx + uvsizex, uvy + uvsizey),
				new Vector2(uvx, uvy + uvsizey)
			);

			int index = ((property.useCollision) ? withCollision : withoutCollision).vertexCount - 4;
			((property.useCollision) ? withCollision : withoutCollision).Add(
				index + 0, index + 3, index + 1, index + 1, index + 3, index + 2
			);

			triangles.AddRange(new int[] { index + 0, index + 3, index + 1, index + 1, index + 3, index + 2 });
		}

		voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = triangles.ToArray();
	}

	private void DrawQuads(Vector3 offset, VoxelProperty property) 
	{
		((property.useCollision) ? withCollision : withoutCollision).Add(
			quadsVertices[0] + offset,
			quadsVertices[1] + offset,
			quadsVertices[2] + offset,
			quadsVertices[3] + offset,
			quadsVertices[4] + offset,
			quadsVertices[5] + offset,
			quadsVertices[6] + offset,
			quadsVertices[7] + offset
		);

		Vector2byte coords = property.textureCoords[0];
		float coordsy = ((float)gameManager.voxelTextures.height / 16f) - (float)coords.y - 1;
		float uvx = (16f * (float)coords.x) / (float)gameManager.voxelTextures.width;
		float uvy = (16f * coordsy) / (float)gameManager.voxelTextures.height;
		float uvsizex = 16f / (float)gameManager.voxelTextures.width;
		float uvsizey = 16f / (float)gameManager.voxelTextures.height;

		((property.useCollision) ? withCollision : withoutCollision).Add(
			new Vector2(uvx, uvy),
			new Vector2(uvx + uvsizex, uvy),
			new Vector2(uvx + uvsizex, uvy + uvsizey),
			new Vector2(uvx, uvy + uvsizey),
			new Vector2(uvx + uvsizex, uvy),
			new Vector2(uvx + uvsizex, uvy + uvsizey),
			new Vector2(uvx, uvy + uvsizey),
			new Vector2(uvx, uvy)
		);

		((property.useCollision) ? withCollision : withoutCollision).Add(
			0, 1, 2, 2, 3, 0,
			4, 5, 6, 6, 7, 4
		);

		voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = new int[] 
		{
			0, 1, 2, 2, 3, 0,
			4, 5, 6, 6, 7, 4
		};
	}

	private void DrawModel(Vector3 offset, VoxelProperty property) 
	{
		Mesh mesh = gameManager.modelMeshes[property.id];

		for (int i = 0; i < mesh.vertices.Length; i++) 
		{ ((property.useCollision) ? withCollision : withoutCollision).Add(mesh.vertices[i]); }

		((property.useCollision) ? withCollision : withoutCollision).Add(mesh.triangles);
		((property.useCollision) ? withCollision : withoutCollision).Add(mesh.uv);

		voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = mesh.triangles;
	}

	private void DrawLiquid(Vector3 offset, VoxelProperty property) 
	{
		int i = 0;
		List<int> triangles = new List<int>(12);
		for (; i < liquidMeshes.Count; i++) { if (liquidMeshes[i].name == property.id) { break; } }
		if (i >= liquidMeshes.Count) 
		{
			liquidMeshes.Add(new ChunkMesh(gameObject.transform, property.id, 12));
			i = liquidMeshes.Count - 1;
			liquidMeshes[i].renderer.material = new Material(Shader.Find("Custom/Liquid"));
			liquidMeshes[i].renderer.material.SetTexture("_MainTex", gameManager.liquidTextures);
			liquidMeshes[i].gameObject.tag = "Liquid";
			liquidMeshes[i].collider.convex = true;
			liquidMeshes[i].collider.isTrigger = true;
		}

		for (VoxelFace face = 0; (int)face < 6; face = (VoxelFace)((int)face + 1)) 
		{
			if (!CanDrawFace((int)offset.x, (int)offset.y, (int)offset.z, face)) { continue; }

			liquidMeshes[i].Add(new Vector3[] {
				blockVertices[(int)face, 0] + offset,
				blockVertices[(int)face, 1] + offset,
				blockVertices[(int)face, 2] + offset,
				blockVertices[(int)face, 3] + offset
			});

			float uvx = 32f / (float)gameManager.liquidTextures.width;
			float uvy = 1f - (32f / (float)gameManager.liquidTextures.height);

			liquidMeshes[i].Add(new Vector2[] {
				new Vector2(0f, uvy),
				new Vector2(uvx, uvy),
				new Vector2(uvx, 1f),
				new Vector2(0f, 1f)
			});

			int index = liquidMeshes[i].vertexCount - 4;
			liquidMeshes[i].Add(
				index + 0, index + 3, index + 1, index + 1, index + 3, index + 2
			);

			triangles.AddRange(new int[] { index + 0, index + 3, index + 1, index + 1, index + 3, index + 2 });
		}

		voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = triangles.ToArray();
	}

	private void GenerateMesh() 
	{
		withCollision.Clear();
		withoutCollision.Clear();
		for (int i = 0; i < liquidMeshes.Count; i++) 
		{ liquidMeshes[i].Clear(); }

		for (int y = 0; y < voxelTypes.GetLength(1); y++) 
		{
			for (int z = 0; z < voxelTypes.GetLength(2); z++) 
			{
				for (int x = 0; x < voxelTypes.GetLength(0); x++) 
				{
					AddVoxel(GetVoxelType(x, y, z), x, y, z);
				}
			}
		}
	}

	public void SetVoxelType(ushort type, int x, int y, int z) 
	{
		try { voxelTypes[x, y, z] = type; }
		catch {}
	}

	public ushort GetVoxelType(int x, int y, int z) 
	{
		try { return voxelTypes[x, y, z]; }
		catch { return gameManager.GetVoxelTypeById("air-block"); }
	}

	public void AddVoxel(ushort type, int x, int y, int z) 
	{
		if (x < 0 || x >= voxelTypes.GetLength(0) || 
			y < 0 || y >= voxelTypes.GetLength(1) || 
			z < 0 || z >= voxelTypes.GetLength(2)
		) { return; }

		SetVoxelType(type, x, y, z);
		if (gameManager.voxelProperties[type].id == "air-block") { return; }

		VoxelProperty property = gameManager.voxelProperties[type];
		Vector3 offset = new Vector3((float)x, (float)y, (float)z);

		if (property.id.EndsWith("-block")) { DrawBlock(offset, property); return; }
		if (property.id.EndsWith("-quads")) { DrawQuads(offset, property); return; }
		if (property.id.EndsWith("-liquid")) { DrawLiquid(offset, property); return; }
		if (property.id.EndsWith("-model")) { DrawModel(offset, property); return; }
	}

	public void RemoveVoxel(int x, int y, int z, bool saveType) 
	{
		if (x < 0 || x >= voxelTypes.GetLength(0) || 
			y < 0 || y >= voxelTypes.GetLength(1) || 
			z < 0 || z >= voxelTypes.GetLength(2)
		) { return; }

		VoxelProperty property = gameManager.voxelProperties[GetVoxelType(x, y, z)];

		if (voxelTriangles[x, y, z] == null || 
			voxelTriangles[x, y, z].Length == 0 || 
			property.id.EndsWith("-liquid")
		) { return; }

		if (!saveType) { SetVoxelType(gameManager.GetVoxelTypeById("air-block"), x, y, z); }

		int i = 0;
		for (; i < ((property.useCollision) ? withCollision : withoutCollision).triangles.Count; i++) 
		{
			if (((property.useCollision) ? withCollision : withoutCollision).triangles[i] != voxelTriangles[x, y, z][0]) 
			{ continue; }

			int t = 0;
			while (t < voxelTriangles[x, y, z].Length) 
			{
				if (((property.useCollision) ? withCollision : withoutCollision).triangles[i + t] != voxelTriangles[x, y, z][t]) 
				{ break; }
				t++;
			}

			if (t >= voxelTriangles[x, y, z].Length) { break; }
		}

		((property.useCollision) ? withCollision : withoutCollision).triangles.RemoveRange(i, voxelTriangles[x, y, z].Length);

		// for (; i < ((property.useCollision) ? withCollision : withoutCollision).triangles.Count; i++) 
		// { ((property.useCollision) ? withCollision : withoutCollision).triangles[i] -= voxelTriangles[x, y, z].Length; }

		List<Vector3> vertices = new List<Vector3>(((property.useCollision) ? withCollision : withoutCollision).vertices.Count - voxelTriangles[x, y, z].Length);
		List<Vector2> uvs = new List<Vector2>(((property.useCollision) ? withCollision : withoutCollision).uvs.Count - voxelTriangles[x, y, z].Length);
		for (i = 0; i < ((property.useCollision) ? withCollision : withoutCollision).vertices.Count; i++) 
		{
			if (Array.IndexOf(voxelTriangles[x, y, z], i) >= 0) { continue; }
			vertices.Add((((property.useCollision) ? withCollision : withoutCollision).vertices[i]));
			uvs.Add((((property.useCollision) ? withCollision : withoutCollision).uvs[i]));
		}

		// ((property.useCollision) ? withCollision : withoutCollision).vertices = vertices;
		// ((property.useCollision) ? withCollision : withoutCollision).uvs = uvs;
		((property.useCollision) ? withCollision : withoutCollision).Update();
		voxelTriangles[x, y, z] = null;
	}

	public static VoxelFace GetFaceFromTriangleIndex(List<Vector3> vertices, List<int> triangles, Vector3 voxel, int index) 
	{
		if (vertices[triangles[index + 0]].x == vertices[triangles[index + 1]].x && vertices[triangles[index + 1]].x == vertices[triangles[index + 2]].x) 
		{ return (vertices[triangles[index]].x == voxel.x) ? VoxelFace.Left : VoxelFace.Right; }

		if (vertices[triangles[index + 0]].y == vertices[triangles[index + 1]].y && vertices[triangles[index + 1]].y == vertices[triangles[index + 2]].y) 
		{ return (vertices[triangles[index]].y == voxel.y) ? VoxelFace.Down : VoxelFace.Up; }

		if (vertices[triangles[index + 0]].z == vertices[triangles[index + 1]].z && vertices[triangles[index + 1]].z == vertices[triangles[index + 2]].z) 
		{ return (vertices[triangles[index]].z == voxel.z) ? VoxelFace.Back : VoxelFace.Forward; }

		return VoxelFace.Down;
	}

	public static void AddFaceToPosition(ref int x, ref int y, ref int z, VoxelFace face) 
	{
		switch (face) 
		{
			case VoxelFace.Up:
				y++;
				break;
			case VoxelFace.Down:
				y--;
				break;
			case VoxelFace.Left:
				x++;
				break;
			case VoxelFace.Right:
				x--;
				break;
			case VoxelFace.Forward:
				z++;
				break;
			case VoxelFace.Back:
				z--;
				break;
		}
	}

	public static VoxelFace OpositeFace(VoxelFace face) 
	{
		if ((int)face % 2 == 0) { return (VoxelFace)((int)face + 1); }
		else { return (VoxelFace)((int)face - 1); }
	}

	public static VoxelFace GetFaceFromNormal(Vector3 normal) 
	{
		if (Math2.Abs(normal.y) > 0.707f) { return (normal.y < 0f) ? VoxelFace.Down : VoxelFace.Up; }
		if (Math2.Abs(normal.x) > 0.707f) { return (normal.x < 0f) ? VoxelFace.Left : VoxelFace.Right; }
		if (Math2.Abs(normal.z) > 0.707f) { return (normal.z < 0f) ? VoxelFace.Back : VoxelFace.Forward; }

		return VoxelFace.Down;
	}

	public static bool IsInsideVoxel(Vector3 voxel, Vector3 vertex) 
	{
		return 
		vertex.x >= voxel.x && vertex.x <= voxel.x + 1f && 
		vertex.y >= voxel.y && vertex.y <= voxel.y + 1f && 
		vertex.z >= voxel.z && vertex.z <= voxel.z + 1f;
	}

	public ushort GetAdjacentVoxelType(int x, int y, int z, VoxelFace face) 
	{
		AddFaceToPosition(ref x, ref y, ref z, face);

		if (x < 0 || x >= voxelTypes.GetLength(0) || 
			y < 0 || y >= voxelTypes.GetLength(1) || 
			z < 0 || z >= voxelTypes.GetLength(2)
		) { return gameManager.GetVoxelTypeById("air-block"); }

		return GetVoxelType(x, y, z);
	}

	public void Update() 
	{
		withCollision.Update();
		withoutCollision.Update();
		for (int i = 0; i < liquidMeshes.Count; i++) { liquidMeshes[i].Update(); }
	}

	public ushort GetVoxelTypeFromPoint(Vector3 point, Vector3 normal) 
	{ Vector3Int position = GetVoxelPositionFromPoint(point, normal); return GetVoxelType(position.x, position.y, position.z); }

	public string GetVoxelIdFromPoint(Vector3 point, Vector3 normal) 
	{
		try { return gameManager.voxelProperties[GetVoxelTypeFromPoint(point, normal)].id; }
		catch { return "undefined-block"; }
	}

	public Vector3Int GetVoxelPositionFromPoint(Vector3 point, Vector3 normal) 
	{
		Vector3Int coords = new Vector3Int(
			Mathf.FloorToInt(point.x),
			Mathf.FloorToInt(point.y),
			Mathf.FloorToInt(point.z)
		);

		Vector3Int offset = new Vector3Int(
			-(int)gameObject.transform.position.x + ((normal == Vector3.right) ? -1 : 0),
			-(int)gameObject.transform.position.y + ((normal == Vector3.up) ? -1 : 0),
			-(int)gameObject.transform.position.z + ((normal == Vector3.forward) ? -1 : 0)
		);

		return coords + offset;
	}

	public bool ContainsInList(List<Chunk> list) 
	{
		for (int i = 0; i < list.Count; i++) 
		{ if (this.gameObject.name == list[i].gameObject.name) { return true; } }
		return false;
	}

	public Chunk(GameManager gameManager, Vector3 position, Transform parent) 
	{
		this.gameManager = gameManager;
		this.liquidMeshes = new List<ChunkMesh>();
		this.voxelTypes = new ushort[
			gameManager.gameSettings.terrain.chunkSize,
			gameManager.gameSettings.terrain.chunkHeight,
			gameManager.gameSettings.terrain.chunkSize
		];

		this.voxelTriangles = new int[voxelTypes.GetLength(0), voxelTypes.GetLength(1), voxelTypes.GetLength(2)][];
		this.gameObject = new GameObject(position.ToString());
		this.gameObject.transform.SetParent(parent);
		this.gameObject.transform.position = position;
		this.gameObject.transform.eulerAngles = Vector3.zero;
		this.gameObject.transform.localScale = Vector3.one;
		this.gameObject.tag = "Chunk";

		this.controller = (ChunkController)this.gameObject.AddComponent(typeof(ChunkController));
		this.controller.gameManager = this.gameManager;
		this.controller.chunk = this;

		this.withCollision = new ChunkMesh(this.gameObject.transform, "With Collider", 10);
		this.withCollision.renderer.material = new Material(Shader.Find("Custom/CullBack"));
		this.withCollision.renderer.material.SetTexture("_MainTex", this.gameManager.voxelTextures);

		this.withoutCollision = new ChunkMesh(this.gameObject.transform, "Without Collider", 11);
		this.withoutCollision.renderer.material = new Material(Shader.Find("Custom/CullOff"));
		this.withoutCollision.renderer.material.SetTexture("_MainTex", this.gameManager.voxelTextures);

		this.GenerateVoxels();
		this.GenerateMesh();
		this.Update();
	}

	public static readonly Vector3[,] blockVertices = 
	{
		{ new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, 1f, 1f), new Vector3(0f, 1f, 1f) },
		{ new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, 0f) },
		{ new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, 1f), new Vector3(1f, 1f, 1f), new Vector3(1f, 1f, 0f) },
		{ new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 1f, 1f) },
		{ new Vector3(1f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f), new Vector3(1f, 1f, 1f) },
		{ new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(1f, 1f, 0f), new Vector3(0f, 1f, 0f) },
	};

	public	static readonly Vector3[] quadsVertices = 
	{
		new Vector3(0f, 0f, 0f),
		new Vector3(1f, 0f, 1f),
		new Vector3(1f, 1f, 1f),
		new Vector3(0f, 1f, 0f),
		new Vector3(1f, 0f, 0f),
		new Vector3(1f, 1f, 0f),
		new Vector3(0f, 1f, 1f),
		new Vector3(0f, 0f, 1f)
	};
}