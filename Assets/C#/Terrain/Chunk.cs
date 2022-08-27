using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	public class Chunk 
	{
		public GameObject gameObject { get; }
		public Vector3 position { get { return gameObject.transform.position; } }
		public string name { get { return gameObject.name; } }

		private GameManager gameManager;
		private ushort[,,] voxelTypes;
		private int[,,][] voxelTriangles;
		private bool canUpdate;

		private MonoBehaviour monoBehaviour { get { return gameManager.monoBehaviour; } }
		private MeshFilter filter;
		private ObjectMesh objectMesh;

		private List<dynamic[]> liquidsToGenerate;
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
							(y < h - GameSettings.terrain.dirtDepth)
							? gameManager.GetVoxelTypeById("stone-block")
							: gameManager.GetVoxelTypeById("dirt-block");
							SetVoxelType(type, x, y, z);
						}
						else { SetVoxelType(gameManager.GetVoxelTypeById("air-block"), x, y, z); }

						if (GetVoxelType(x, y - 1, z) == gameManager.GetVoxelTypeById("dirt-block") && 
							(GetVoxelType(x, y, z) == gameManager.GetVoxelTypeById("air-block"))
						) { SetVoxelType(gameManager.GetVoxelTypeById("grass-block"), x, y - 1, z); }

						if (GetVoxelType(x, y - 1, z) == gameManager.GetVoxelTypeById("grass-block") && 
							x > 5 && x < 10 && z > 5 && z < 10
						) { SetVoxelType(gameManager.GetVoxelTypeById("water-liquid"), x, y - 1, z); }

						// if (GetVoxelType(x, y - 1, z) == gameManager.GetVoxelTypeById("grass-block") && 
						// 	(GetVoxelType(x, y, z) == gameManager.GetVoxelTypeById("air-block"))
						// ) { SetVoxelType(gameManager.GetVoxelTypeById("grass-quads"), x, y, z); }
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

		private void DrawBlock(Vector3 offset, VoxelProperty property, Matrix4x4 matrix) 
		{
			List<int> triangles = new List<int>(12);
			for (int face = 0; face < blockVertices.GetLength(0); face++)
			{
				if (!CanDrawFace((int)offset.x, (int)offset.y, (int)offset.z, (VoxelFace)face)) { continue; }

				objectMesh.Add(
					matrix.MultiplyPoint3x4(blockVertices[face, 0] + offset),
					matrix.MultiplyPoint3x4(blockVertices[face, 1] + offset),
					matrix.MultiplyPoint3x4(blockVertices[face, 2] + offset),
					matrix.MultiplyPoint3x4(blockVertices[face, 3] + offset)
				);

				Vector2byte coords = property.textureCoords[face];
				float coordsy = ((float)GameSettings.textures.voxelHeight / 16f) - (float)coords.y - 1;
				float uvx = (16f * (float)coords.x) / (float)GameSettings.textures.voxelWidth;
				float uvy = (16f * coordsy) / (float)GameSettings.textures.voxelHeight;
				float uvsizex = 16f / (float)GameSettings.textures.voxelWidth;
				float uvsizey = 16f / (float)GameSettings.textures.voxelHeight;

				objectMesh.Add(
					new Vector2(uvx, uvy),
					new Vector2(uvx + uvsizex, uvy),
					new Vector2(uvx + uvsizex, uvy + uvsizey),
					new Vector2(uvx, uvy + uvsizey)
				);

				int index = objectMesh.vertexCount - 4;
				int[] tris = { index + 0, index + 3, index + 1, index + 1, index + 3, index + 2 };
				CullTriangles(ref tris, property.cull);
				objectMesh.Add(tris);
				triangles.AddRange(tris);
			}

			voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = triangles.ToArray();
		}

		private void DrawQuads(Vector3 offset, VoxelProperty property, Matrix4x4 matrix) 
		{
			objectMesh.Add(
				matrix.MultiplyPoint3x4(quadsVertices[0] + offset),
				matrix.MultiplyPoint3x4(quadsVertices[1] + offset),
				matrix.MultiplyPoint3x4(quadsVertices[2] + offset),
				matrix.MultiplyPoint3x4(quadsVertices[3] + offset),
				matrix.MultiplyPoint3x4(quadsVertices[4] + offset),
				matrix.MultiplyPoint3x4(quadsVertices[5] + offset),
				matrix.MultiplyPoint3x4(quadsVertices[6] + offset),
				matrix.MultiplyPoint3x4(quadsVertices[7] + offset)
			);

			Vector2byte coords = property.textureCoords[0];
			float coordsy = ((float)GameSettings.textures.voxelHeight / 16f) - (float)coords.y - 1;
			float uvx = (16f * (float)coords.x) / (float)GameSettings.textures.voxelWidth;
			float uvy = (16f * coordsy) / (float)GameSettings.textures.voxelHeight;
			float uvsizex = 16f / (float)GameSettings.textures.voxelWidth;
			float uvsizey = 16f / (float)GameSettings.textures.voxelHeight;

			objectMesh.Add(
				new Vector2(uvx, uvy),
				new Vector2(uvx + uvsizex, uvy),
				new Vector2(uvx + uvsizex, uvy + uvsizey),
				new Vector2(uvx, uvy + uvsizey),
				new Vector2(uvx + uvsizex, uvy),
				new Vector2(uvx + uvsizex, uvy + uvsizey),
				new Vector2(uvx, uvy + uvsizey),
				new Vector2(uvx, uvy)
			);

			int[] triangles = 
			{
				0, 1, 2, 2, 3, 0,
				4, 5, 6, 6, 7, 4
			};

			CullTriangles(ref triangles, property.cull);
			objectMesh.Add(triangles);
			voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = triangles;
		}

		private void DrawModel(Vector3 offset, VoxelProperty property, Matrix4x4 matrix) 
		{
			Mesh mesh = gameManager.modelMeshes[property.id];

			for (int i = 0; i < mesh.vertices.Length; i++) 
			{ objectMesh.Add(matrix.MultiplyPoint3x4(mesh.vertices[i])); }

			int[] tris = mesh.triangles;
			CullTriangles(ref tris, property.cull);
			objectMesh.Add(tris);
			objectMesh.Add(mesh.uv);

			voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = tris;
		}

		private void DrawLiquid(Vector3 offset, VoxelProperty property, Matrix4x4 matrix) 
		{
			int i = 0;
			List<int> triangles = new List<int>(12);
			for (; i < liquidMeshes.Count; i++) { if (liquidMeshes[i].name == property.id) { break; } }
			if (i >= liquidMeshes.Count) 
			{
				liquidMeshes.Add(new ChunkMesh(gameObject.transform, property.id, 12));
				i = liquidMeshes.Count - 1;
				liquidMeshes[i].renderer.material = GameSettings.materials.cullBack;
				liquidMeshes[i].renderer.material.SetTexture("_MainTex", GameSettings.textures.liquid);
				liquidMeshes[i].gameObject.tag = "Liquid";
				liquidMeshes[i].collider.convex = true;
				liquidMeshes[i].collider.isTrigger = true;
			}

			for (VoxelFace face = 0; (int)face < 6; face = (VoxelFace)((int)face + 1)) 
			{
				if (!CanDrawFace((int)offset.x, (int)offset.y, (int)offset.z, face)) { continue; }

				liquidMeshes[i].Add(new Vector3[] {
					matrix.MultiplyPoint3x4(blockVertices[(int)face, 0] + offset),
					matrix.MultiplyPoint3x4(blockVertices[(int)face, 1] + offset),
					matrix.MultiplyPoint3x4(blockVertices[(int)face, 2] + offset),
					matrix.MultiplyPoint3x4(blockVertices[(int)face, 3] + offset)
				});

				float uvx = 32f / (float)GameSettings.textures.liquidWidth;
				float uvy = 1f - (32f / (float)GameSettings.textures.liquidHeight);

				liquidMeshes[i].Add(new Vector2[] {
					new Vector2(0f, uvy),
					new Vector2(uvx, uvy),
					new Vector2(uvx, 1f),
					new Vector2(0f, 1f)
				});

				int index = liquidMeshes[i].vertexCount - 4;
				int[] tris = { index + 0, index + 3, index + 1, index + 1, index + 3, index + 2 };
				CullTriangles(ref tris, property.cull);
				liquidMeshes[i].Add(tris);
				triangles.AddRange(tris);
			}

			voxelTriangles[(int)offset.x, (int)offset.y, (int)offset.z] = triangles.ToArray();
		}

		private void GenerateMesh() 
		{
			for (int i = 0; i < liquidMeshes.Count; i++) 
			{ liquidMeshes[i].Clear(); }

			for (int y = 0; y < voxelTypes.GetLength(1); y++) 
			{
				for (int z = 0; z < voxelTypes.GetLength(2); z++) 
				{
					for (int x = 0; x < voxelTypes.GetLength(0); x++) 
					{
						AddVoxel(GetVoxelType(x, y, z), x, y, z, Matrix4x4.identity);
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

		public void AddVoxel(ushort type, int x, int y, int z, Matrix4x4 matrix) 
		{
			if (x < 0 || x >= voxelTypes.GetLength(0) || 
				y < 0 || y >= voxelTypes.GetLength(1) || 
				z < 0 || z >= voxelTypes.GetLength(2)
			) { return; }

			SetVoxelType(type, x, y, z);
			if (gameManager.voxelProperties[type].id == "air-block") { return; }

			VoxelProperty property = gameManager.voxelProperties[type];
			Vector3 offset = new Vector3((float)x, (float)y, (float)z);

			if (property.id.EndsWith("-block")) { DrawBlock(offset, property, matrix); return; }
			if (property.id.EndsWith("-quads")) { DrawQuads(offset, property, matrix); return; }
			if (property.id.EndsWith("-liquid")) { liquidsToGenerate.Add(new dynamic[] { offset, property, matrix }); return; }
			if (property.id.EndsWith("-model")) { DrawModel(offset, property, matrix); return; }
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
				property.id.EndsWith("-liquid") || 
				property.id == "air-block"
			) { return; }

			if (!saveType) { SetVoxelType(gameManager.GetVoxelTypeById("air-block"), x, y, z); }

			int i = GetTriangleIndexFromPosition(x, y, z);
			objectMesh.triangles.RemoveRange(i, voxelTriangles[x, y, z].Length);

			/* if saveType is false remove all vertices and uvs for this voxel */
			/* when adding a new voxel first check if this position already has vertices or uvs, and reuse them */

			// for (; i < objectMesh.triangles.Count; i++) 
			// { objectMesh.triangles[i] -= voxelTriangles[x, y, z].Length; }

			// List<Vector3> vertices = new List<Vector3>(objectMesh.vertices.Count - voxelTriangles[x, y, z].Length);
			// List<Vector2> uvs = new List<Vector2>(objectMesh.uvs.Count - voxelTriangles[x, y, z].Length);
			// for (i = 0; i < objectMesh.vertices.Count; i++) 
			// {
			// 	if (Array.IndexOf(voxelTriangles[x, y, z], i) >= 0) { continue; }
			// 	vertices.Add((objectMesh.vertices[i]));
			// 	uvs.Add((objectMesh.uvs[i]));
			// }

			// objectMesh.vertices = vertices;
			// objectMesh.uvs = uvs;

			voxelTriangles[x, y, z] = null;
		}

		[Obsolete("It's probably not working.")]
		public void RemoveFace(int x, int y, int z, VoxelFace face) 
		{
			if (x < 0 || x >= voxelTypes.GetLength(0) || 
				y < 0 || y >= voxelTypes.GetLength(1) || 
				z < 0 || z >= voxelTypes.GetLength(2)
			) { return; }

			VoxelProperty property = gameManager.voxelProperties[GetVoxelType(x, y, z)];

			if (voxelTriangles[x, y, z] == null || 
				voxelTriangles[x, y, z].Length == 0 || 
				!property.id.EndsWith("-block") || 
				property.id == "air-block"
			) { return; }

			int i = GetTriangleIndexFromPosition(x, y, z);
			for (int t = i; t - i < voxelTriangles[x, y, z].Length; t += 6) 
			{
				if (GetFaceFromTriangleIndex(objectMesh.vertices, objectMesh.triangles, new Vector3(x, y, z), t) == face) 
				{
					objectMesh.triangles.RemoveRange(t, 6);
					List<int> list = voxelTriangles[x, y, z].ToList();
					list.RemoveRange(t - i, 6);
					voxelTriangles[x, y, z] = list.ToArray();
					return;
				}
			}
		}

		public int GetTriangleIndexFromPosition(int x, int y, int z) 
		{
			for (int i = 0; i < objectMesh.triangles.Count; i++) 
			{
				if (objectMesh.triangles[i] != voxelTriangles[x, y, z][0]) 
				{ continue; }

				int t = 0;
				while (t < voxelTriangles[x, y, z].Length) 
				{
					if (objectMesh.triangles[i + t] != voxelTriangles[x, y, z][t]) { break; }
					t++;
				}

				if (t >= voxelTriangles[x, y, z].Length) { return i; }
			}

			return -1;
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

		public void Update() { monoBehaviour.StartCoroutine(Update(true)); }
		private IEnumerator Update(bool forceUpdate = false) 
		{
			while (!forceUpdate && !canUpdate) { yield return null; }
			while (liquidsToGenerate.Count > 0) 
			{
				DrawLiquid(liquidsToGenerate[0][0], liquidsToGenerate[0][1], liquidsToGenerate[0][2]);
				liquidsToGenerate.RemoveAt(0);
			}

			filter.mesh = objectMesh.mesh;
			for (int i = 0; i < liquidMeshes.Count; i++) { liquidMeshes[i].Update(); }
		}

		public ushort GetVoxelTypeFromPoint(Vector3 point) 
		{
			Vector3Int position = GetVoxelPositionFromPoint(point);
			return GetVoxelType(position.x, position.y, position.z);
		}

		public string GetVoxelIdFromPoint(Vector3 point) 
		{
			try { return gameManager.voxelProperties[GetVoxelTypeFromPoint(point)].id; }
			catch { return "undefined-block"; }
		}

		public Vector3Int GetVoxelPositionFromPoint(Vector3 point) 
		{
			Vector3Int coords = new Vector3Int(
				Mathf.FloorToInt(point.x),
				Mathf.FloorToInt(point.y),
				Mathf.FloorToInt(point.z)
			);

			Vector3Int offset = new Vector3Int(
				-(int)gameObject.transform.position.x,
				-(int)gameObject.transform.position.y,
				-(int)gameObject.transform.position.z
			);

			return coords + offset;
		}

		public bool ContainsInList(List<Chunk> list) 
		{
			for (int i = 0; i < list.Count; i++) 
			{ if (this.gameObject.name == list[i].gameObject.name) { return true; } }
			return false;
		}

		/* Add delay based on mining speed, force, etc. */
		public void OnPlayerRemoveVoxel(Player player, VoxelHit hit) 
		{
			if (!ContainsInList(gameManager.terrainManager.modifiedChunks)) { gameManager.terrainManager.modifiedChunks.Add(this); }
			Vector3Int position = GetVoxelPositionFromPoint(hit.point);

			RemoveVoxel(position.x, position.y, position.z, false);
			for (int face = 0; face < 6; face++) 
			{
				int x = position.x;
				int y = position.y;
				int z = position.z;
				Chunk.AddFaceToPosition(ref x, ref y, ref z, (VoxelFace)face);
				RemoveVoxel(x, y, z, true);
				AddVoxel(GetVoxelType(x, y, z), x, y, z, Matrix4x4.identity);
			}

			Update();

			Vector3 offset = (Vector3.one * 0.5f) + gameObject.transform.position;
			player.DropItem(new Item(hit.property.dropItem, 1), new Vector3((float)position.x, (float)position.y, (float)position.z) + offset);
		}

		/* Request the player to swap a slot with a hit id with the current hand slot. */
		public void OnPlayerPickVoxel(Player player, VoxelHit hit) 
		{
			Console.Log("Picking: " + hit.property.id);
		}

		/* Get item from the current hand slot. */
		/* Check if you can place on the hit block. */
		/* Check for avaliable rotations from the property. */
		public void OnPlayerPlaceVoxel(Player player, VoxelHit hit) 
		{
			if (!ContainsInList(gameManager.terrainManager.modifiedChunks)) { gameManager.terrainManager.modifiedChunks.Add(this); }
			Vector3Int position = GetVoxelPositionFromPoint(hit.previousHit.point);

			AddVoxel(gameManager.GetVoxelTypeById("grass-block"), position.x, position.y, position.z, Matrix4x4.identity);

			for (int face = 0; face < 6; face++) 
			{
				int x = position.x;
				int y = position.y;
				int z = position.z;
				Chunk.AddFaceToPosition(ref x, ref y, ref z, (VoxelFace)face);
				RemoveVoxel(x, y, z, true);
				AddVoxel(GetVoxelType(x, y, z), x, y, z, Matrix4x4.identity);
			}

			Update();
		}

		public Chunk(GameManager gameManager, Vector3 position, Transform parent) 
		{
			this.gameManager = gameManager;
			this.liquidMeshes = new List<ChunkMesh>();
			this.voxelTypes = new ushort[
				GameSettings.terrain.chunkSize,
				GameSettings.terrain.chunkHeight,
				GameSettings.terrain.chunkSize
			];

			this.canUpdate = false;
			this.voxelTriangles = new int[voxelTypes.GetLength(0), voxelTypes.GetLength(1), voxelTypes.GetLength(2)][];
			this.liquidsToGenerate = new List<dynamic[]>();
			this.objectMesh = new ObjectMesh();

			this.gameObject = new GameObject(position.ToString());
			this.gameObject.transform.SetParent(parent);
			this.gameObject.transform.position = position;
			this.gameObject.transform.eulerAngles = Vector3.zero;
			this.gameObject.transform.localScale = Vector3.one;
			this.filter = (MeshFilter)this.gameObject.AddComponent(typeof(MeshFilter));
			this.gameObject.tag = "Chunk";

			MeshRenderer renderer = (MeshRenderer)this.gameObject.AddComponent(typeof(MeshRenderer));
			renderer.material = GameSettings.materials.cullBack;
			renderer.material.SetTexture("_MainTex", GameSettings.textures.voxel);

			new Thread(new ThreadStart(Generate)).Start();
			monoBehaviour.StartCoroutine(this.Update(false));
		}

		private void Generate() 
		{
			canUpdate = false;
			GenerateVoxels();
			GenerateMesh();
			canUpdate = true;
		}

		public static int[] CullTriangles(int[] triangles, Cull mode) { CullTriangles(ref triangles, mode); return triangles; }
		public static void CullTriangles(ref int[] triangles, Cull mode) 
		{
			if (triangles.Length % 3 != 0) { throw new ArgumentException("Triangle array must be multiple of 3."); }

			switch (mode) 
			{
				case Cull.Back:
					return;
				case Cull.Front:
					for (int i = 0; i < triangles.Length; i += 3) 
					{
						int tmp = triangles[i + 1];
						triangles[i + 1] = triangles[i + 2];
						triangles[i + 2] = tmp;
					}

					return;
				case Cull.Off:
					List<int> list = triangles.ToList();
					for (int i = 0; i < list.Count; i += 6) { list.InsertRange(i, new int[] { list[i + 0], list[i + 2], list[i + 1] }); }
					triangles = list.ToArray();
					return;
			}
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
}