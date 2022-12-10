using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using Utils;
using Utils.Files;
using Minecraft.UI;

namespace Minecraft 
{
	public class Chunk 
	{
		public GameObject gameObject { private set; get; }
		public Vector3 position { private set; get; }
		public string name { private set; get; }
		public static int ChunkSize { get { return GameSettings.world.chunkSize; } }
		public static int ChunkHeight { get { return GameSettings.world.chunkHeight; } }
		public bool IsActive { set { gameObject.SetActive(value); } get { return gameObject.activeSelf; } }

		private uint[] voxels; // 00empty 0_light __type __type
		private int[,,][] voxelTriangles;
		private Matrix4x4[,,] matrices;
		private bool canUpdate;

		private ObjectMesh objectMesh;
		private MeshRenderer renderer;
		private MeshFilter filter;

		private List<dynamic[]> liquidsToGenerate;
		private List<ChunkMesh> liquidMeshes;

		private void GenerateVoxels() 
		{
			for (int y = 0; y < ChunkHeight; y++) 
			{
				// if (y >= 10) { break; }
				bool skip = false;
				for (int z = 0; z < ChunkSize; z++) 
				{
					for (int x = 0; x < ChunkSize; x++) 
					{
						if (skip) { skip = false; continue; }
						string id = "air-block";

						// if (y < 10) { id = "stone-block"; }

						Settings.Biome biome = TerrainManager.GetBiomeById("plains-biome");
						float result = Noise.Perlin.Value2D(
							new Vector2((float)x + position.x, (float)z + position.z),
							biome.heightNoise
						);

						int h = (int)Mathf.Lerp(biome.height.min, biome.height.max, result);

						skip = y > h;
						if (!skip) { id = "stone-block"; }
						if (h < biome.seaLevel) { id = "water-liquid"; }

						uint type = GameManager.GetVoxelTypeById(id);
						VoxelProperty property = GameManager.voxelProperties[type];
						SetVoxelType(type, x, y, z);
					}
				}
			}
		}

		private void GenerateMesh() 
		{
			for (int i = 0; i < liquidMeshes.Count; i++) { liquidMeshes[i].Clear(); }

			for (int y = 0; y < ChunkHeight; y++) 
			{
				for (int z = 0; z < ChunkSize; z++) 
				{
					for (int x = 0; x < ChunkSize; x++) 
					{
						AddVoxel(GetVoxelType(x, y, z), x, y, z);
					}
				}
			}
		}

		private bool CanDrawFace(int x, int y, int z, VoxelFace face) 
		{
			bool isThisALiquid = GameManager.voxelProperties[GetVoxelType(x, y, z)].id.EndsWith("-liquid");
			AddFaceToPosition(ref x, ref y, ref z, face);

			if (IsOutOfBounds(x, y, z) || 
				(!isThisALiquid && GameManager.voxelProperties[GetVoxelType(x, y, z)].id.EndsWith("-liquid"))
			) { return true; }

			VoxelProperty property = GameManager.voxelProperties[GetVoxelType(x, y, z)];
			return !property.usingFullFace[(int)OpositeFace(face)];
		}

		private void DrawBlock(Vector3Int position, VoxelProperty property, Matrix4x4 matrix) 
		{
			Vector3 offset = -Vector3.one / 2f;
			List<int> triangles = new List<int>(36);
			for (int face = 0; face < 6; face++)
			{
				if (!CanDrawFace(position.x, position.y, position.z, (VoxelFace)face)) { continue; }

				for (int v = 0; v < 4; v++) 
				{ objectMesh.Add((matrix.MultiplyPoint3x4(ConstMeshData.blockVertices[face][v] + offset) - offset) + property.offset); }

				Utils.Collections.Generic.Vector2<byte> coords = property.textureCoords[face];
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

				int[] tris = ConstMeshData.blockTriangles(objectMesh.vertexCount - 1);
				Tools.CullTriangles(ref tris, property.cull);
				objectMesh.Add(tris);
				triangles.AddRange(tris);
			}

			voxelTriangles[position.x, position.y, position.z] = triangles.ToArray();
		}

		private void DrawQuads(Vector3Int position, VoxelProperty property, Matrix4x4 matrix) 
		{
			Vector3 offset = -Vector3.one / 2f;

			for (int v = 0; v < ConstMeshData.quadsVertices.Length; v++) 
			{ objectMesh.Add((matrix.MultiplyPoint3x4(ConstMeshData.quadsVertices[v] + offset) - offset) + property.offset); }

			Utils.Collections.Generic.Vector2<byte> coords = property.textureCoords[0];
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

			int[] triangles = ConstMeshData.quadsTriangles;
			Tools.CullTriangles(ref triangles, property.cull);
			objectMesh.Add(triangles);
			voxelTriangles[position.x, position.y, position.z] = triangles;
		}

		private bool DrawModel(Vector3Int position, VoxelProperty property, Matrix4x4 matrix) 
		{
			Vector3 offset = -Vector3.one / 2f;

			ObjectMesh mesh;
			try { mesh = GameManager.modelMeshes[property.id]; }
			catch { return false; }

			for (int i = 0; i < mesh.vertices.Count; i++) 
			{ objectMesh.Add((matrix.MultiplyPoint3x4(mesh.vertices[i] + offset) - offset) + property.offset); }

			int index = objectMesh.vertexCount;
			int[] tris = mesh.triangles.Select(x => x + index).ToArray();
			Tools.CullTriangles(ref tris, property.cull);
			objectMesh.Add(tris);
			objectMesh.Add(mesh.uvs.ToArray());

			voxelTriangles[position.x, position.y, position.z] = tris;
			return true;
		}

		private bool DrawSingleTexureModel(Vector3Int position, VoxelProperty property, Matrix4x4 matrix) 
		{
			string[] tokens = property.id.Split('-');
			Vector3 offset = -Vector3.one / 2f;

			ObjectMesh mesh;
			try { mesh = GameManager.singleTextureModelMeshes["-" + tokens[tokens.Length - 1]]; }
			catch { return false; }

			for (int i = 0; i < mesh.vertices.Count; i++) 
			{ objectMesh.Add((matrix.MultiplyPoint3x4(mesh.vertices[i] + offset) - offset) + property.offset); }

			int index = objectMesh.vertexCount - mesh.vertexCount;
			int[] tris = mesh.triangles.Select(x => x + index).ToArray();
			Tools.CullTriangles(ref tris, property.cull);
			objectMesh.Add(tris);

			Vector2 scale = new Vector2(16f / (float)GameSettings.textures.voxelWidth, 16f / (float)GameSettings.textures.voxelHeight);
			Vector2 coords = new Vector2((float)property.textureCoords[0].x * scale.x, (float)(property.textureCoords[0].y + 1) * scale.y);
			coords.y = 1f - coords.y;
			objectMesh.Add(mesh.uvs.Select(x => new Vector2((x.x * scale.x) + coords.x, (x.y * scale.y) + coords.y)).ToArray());

			voxelTriangles[position.x, position.y, position.z] = tris;
			return true;
		}

		private void DrawLiquid(Vector3Int position, VoxelProperty property, Matrix4x4 matrix) 
		{
			int i = 0;
			Vector3 offset = -Vector3.one / 2f;
			List<int> triangles = new List<int>(36);
			for (; i < liquidMeshes.Count; i++) { if (liquidMeshes[i].name == property.id) { break; } }
			if (i >= liquidMeshes.Count) 
			{
				liquidMeshes.Add(new ChunkMesh(gameObject.transform, property.id, 12));
				i = liquidMeshes.Count - 1;
				liquidMeshes[i].renderer.material = GameSettings.materials.liquid;
				liquidMeshes[i].renderer.material.SetTexture("_MainTex", GameSettings.textures.liquid);
				liquidMeshes[i].gameObject.tag = "Liquid";
				liquidMeshes[i].collider.convex = true;
				liquidMeshes[i].collider.isTrigger = true;
			}

			for (int face = 0; face < 6; face++) 
			{
				if (!CanDrawFace(position.x, position.y, position.z, (VoxelFace)face)) { continue; }

				for (int v = 0; v < 4; v++) 
				{ liquidMeshes[i].Add((matrix.MultiplyPoint3x4(ConstMeshData.blockVertices[face][v] + offset) - offset) + property.offset); }

				float uvx = 32f / (float)GameSettings.textures.liquidWidth;
				float uvy = 1f - (32f / (float)GameSettings.textures.liquidHeight);

				liquidMeshes[i].Add(new Vector2[] {
					new Vector2(0f, uvy),
					new Vector2(uvx, uvy),
					new Vector2(uvx, 1f),
					new Vector2(0f, 1f)
				});

				int[] tris = ConstMeshData.blockTriangles(liquidMeshes[i].vertexCount - 1);
				Tools.CullTriangles(ref tris, property.cull);
				liquidMeshes[i].Add(tris);
				triangles.AddRange(tris);
			}

			voxelTriangles[position.x, position.y, position.z] = triangles.ToArray();
		}

		public static int GetVoxelIndex(int x, int y, int z) { return x + (z * ChunkSize) + (y * ChunkSize * ChunkSize); }

		public static Matrix4x4 GetDefaultVoxelMatrix(int x, int y, int z) 
		{
			return Matrix4x4.TRS(
				new Vector3((float)x, (float)y, (float)z),
				Quaternion.Euler(0f, 0f, 0f),
				Vector3.one
			);
		}

		public Chunk GetOutsideChunk(int x, int y, int z) 
		{
			Vector3 position = new Vector3(
				(x < 0) ? -1 : ((x >= ChunkSize) ? 1 : 0),
				(y < 0) ? -1 : ((y >= ChunkSize) ? 1 : 0),
				(z < 0) ? -1 : ((z >= ChunkSize) ? 1 : 0)
			) + this.position;

			return (this.position == position) ? this : TerrainManager.GetChunkFromPosition(position);
		}

		public void SetVoxelType(uint type, int x, int y, int z) 
		{
			try { voxels[GetVoxelIndex(x, y, z)] = (voxels[GetVoxelIndex(x, y, z)] & 0xffff0000) + (type & 0xffff); }
			catch {}
		}

		public void SetVoxelLight(uint light, int x, int y, int z) 
		{
			try { voxels[GetVoxelIndex(x, y, z)] = (voxels[GetVoxelIndex(x, y, z)] & 0xfff0ffff) + ((light & 0xf) << 16); }
			catch {}
		}

		public uint GetVoxelType(int x, int y, int z) 
		{
			try { return voxels[GetVoxelIndex(x, y, z)] & 0xffff; }
			catch { return GameManager.GetVoxelTypeById("air-block"); }
		}

		public uint GetVoxelLight(int x, int y, int z) 
		{
			try { return (voxels[GetVoxelIndex(x, y, z)] >> 16) & 0xf; }
			catch { return 0xf; }
		}

		[System.Obsolete("Might not work.")]
		public ObjectMesh GetMeshFromVoxel(int x, int y, int z) 
		{
			if (IsOutOfBounds(x, y, z)) { return null; }

			ObjectMesh mesh = new ObjectMesh();
			int index = GetTriangleIndexFromPosition(x, y, z);

			for (int i = 0; i < voxelTriangles[x, y, z].Length; i++) 
			{
				mesh.Add(voxelTriangles[x, y, z][i] - index);
				mesh.Add(objectMesh.uvs[voxelTriangles[x, y, z][i]]);
				mesh.Add(new Vector3(
					Math.InverseLerp((float)x, (float)x + 1f, objectMesh.vertices[voxelTriangles[x, y, z][i]].x),
					Math.InverseLerp((float)y, (float)y + 1f, objectMesh.vertices[voxelTriangles[x, y, z][i]].y),
					Math.InverseLerp((float)z, (float)z + 1f, objectMesh.vertices[voxelTriangles[x, y, z][i]].z)
				));
			}

			return mesh;
		}

		public void AddVoxel(uint type, int x, int y, int z) 
		{
			if (IsOutOfBounds(x, y, z)) { return; }

			VoxelProperty property = GameManager.voxelProperties[type];
			if (property.id == "air-block") { return; }

			Vector3Int position = new Vector3Int(x, y, z);
			Matrix4x4 matrix = (matrices[x, y, z] == null) ? GetDefaultVoxelMatrix(x, y, z) : matrices[x, y, z];

			if (property.id.EndsWith("-block")) { DrawBlock(position, property, matrix); SetVoxelType(type, x, y, z); return; }
			if (property.id.EndsWith("-quads")) { DrawQuads(position, property, matrix); SetVoxelType(type, x, y, z); return; }
			if (property.id.EndsWith("-liquid")) { liquidsToGenerate.Add(new dynamic[] { position, property, matrix }); SetVoxelType(type, x, y, z); return; }
			if (property.id.EndsWith("-model") && DrawModel(position, property, matrix)) { SetVoxelType(type, x, y, z); return; }
			if (DrawSingleTexureModel(position, property, matrix)) { SetVoxelType(type, x, y, z); return; }
		}

		public void RemoveVoxel(int x, int y, int z, bool saveType) 
		{
			if (IsOutOfBounds(x, y, z)) { return; }

			VoxelProperty property = GameManager.voxelProperties[GetVoxelType(x, y, z)];

			if (voxelTriangles[x, y, z] == null || 
				voxelTriangles[x, y, z].Length == 0 || 
				property.id.EndsWith("-liquid") || 
				property.id == "air-block"
			) { return; }


			int i = GetTriangleIndexFromPosition(x, y, z);

			objectMesh.triangles.RemoveRange(i, voxelTriangles[x, y, z].Length);
			if (!saveType) { SetVoxelType(GameManager.GetVoxelTypeById("air-block"), x, y, z); matrices[x, y, z] = GetDefaultVoxelMatrix(x, y, z); }
			voxelTriangles[x, y, z] = null;
			return;

			/* if saveType is false remove all vertices and uvs for this voxel */
			/* when adding a new voxel first check if this position already has vertices or uvs, and reuse them */

			int removedVerticesLength = 0;
			List<Vector3> vertices = new List<Vector3>(objectMesh.vertices.Count);
			List<Vector2> uvs = new List<Vector2>(objectMesh.uvs.Count);
			for (int v = 0; v < objectMesh.vertices.Count; v++) 
			{
				if (System.Array.IndexOf(voxelTriangles[x, y, z], v) >= 0) { removedVerticesLength++; continue; }
				vertices.Add((objectMesh.vertices[v]));
				uvs.Add((objectMesh.uvs[v]));
			}

			for (; i < objectMesh.triangles.Count; i++) 
			{ objectMesh.triangles[i] -= removedVerticesLength; }

			matrices[x, y, z] = GetDefaultVoxelMatrix(x, y, z);
			voxelTriangles[x, y, z] = null;
			for (; y < ChunkHeight; y++) 
			{
				for (; z < ChunkSize; z++) 
				{
					for (; x < ChunkSize; x++) 
					{
						if (voxelTriangles[x, y, z] == null) { continue; }
						for (i = 0; i < voxelTriangles[x, y, z].Length; i++) { voxelTriangles[x, y, z][i] -= removedVerticesLength; }
					}

					x = 0;
				}

				z = 0;
			}

			objectMesh.vertices = vertices;
			objectMesh.uvs = uvs;
		}

		[System.Obsolete("It's not working.")]
		public void RemoveFace(int x, int y, int z, VoxelFace face) 
		{
			if (IsOutOfBounds(x, y, z)) { return; }

			VoxelProperty property = GameManager.voxelProperties[GetVoxelType(x, y, z)];

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
				if (objectMesh.triangles[i] != voxelTriangles[x, y, z][0]) { continue; }

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

		public static bool IsOutOfBounds(int x, int y, int z) 
		{
			return
			x < 0 || x >= ChunkSize || 
			y < 0 || y >= ChunkHeight || 
			z < 0 || z >= ChunkSize;
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
					x--;
					break;
				case VoxelFace.Right:
					x++;
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

		public static Vector3 GetNormalFromFace(VoxelFace face) 
		{
			switch (face) 
			{
				case VoxelFace.Up:
					return Vector3.up;
				case VoxelFace.Down:
					return -Vector3.up;
				case VoxelFace.Left:
					return -Vector3.right;
				case VoxelFace.Right:
					return Vector3.right;
				case VoxelFace.Forward:
					return Vector3.forward;
				case VoxelFace.Back:
					return -Vector3.forward;
			}

			return Vector3.up;
		}

		public static VoxelFace GetFaceFromNormal(Vector3 normal) 
		{
			if (Math.Abs(normal.y) > 0.707f) { return (normal.y < 0f) ? VoxelFace.Down : VoxelFace.Up; }
			if (Math.Abs(normal.x) > 0.707f) { return (normal.x < 0f) ? VoxelFace.Left : VoxelFace.Right; }
			if (Math.Abs(normal.z) > 0.707f) { return (normal.z < 0f) ? VoxelFace.Back : VoxelFace.Forward; }

			return VoxelFace.Down;
		}

		public static bool IsInsideVoxel(Vector3 voxel, Vector3 vertex) 
		{
			return 
			vertex.x >= voxel.x && vertex.x <= voxel.x + 1f && 
			vertex.y >= voxel.y && vertex.y <= voxel.y + 1f && 
			vertex.z >= voxel.z && vertex.z <= voxel.z + 1f;
		}

		public uint GetAdjacentVoxelType(int x, int y, int z, VoxelFace face) 
		{
			AddFaceToPosition(ref x, ref y, ref z, face);
			if (IsOutOfBounds(x, y, z)) { return GameManager.GetVoxelTypeById("air-block"); }
			return GetVoxelType(x, y, z);
		}

		public void Update() { GameManager.instance.StartCoroutine(Update(true)); }
		private IEnumerator Update(bool forceUpdate = false) 
		{
			while (!forceUpdate && !canUpdate) { yield return null; }
			while (liquidsToGenerate.Count > 0) 
			{
				DrawLiquid(liquidsToGenerate[0][0], liquidsToGenerate[0][1], liquidsToGenerate[0][2]);
				liquidsToGenerate.RemoveAt(0);
			}

			filter.mesh = objectMesh.mesh;

			SetVoxelLight(15, 0, 0, 0);
			SetVoxelLight(10, 1, 0, 0);
			SetVoxelLight(5, 2, 0, 0);

			ComputeBuffer buffer = new ComputeBuffer(voxels.Length, sizeof(uint));
			buffer.SetData(voxels);
			renderer.material.SetBuffer("voxels", buffer);
			buffer.Release();
			
			for (int i = 0; i < liquidMeshes.Count; i++) { liquidMeshes[i].Update(); }
		}

		public uint GetVoxelTypeFromPoint(Vector3 point) 
		{
			Vector3Int position = GetVoxelPositionFromPoint(point);
			return GetVoxelType(position.x, position.y, position.z);
		}

		public string GetVoxelIdFromPoint(Vector3 point) 
		{
			try { return GameManager.voxelProperties[GetVoxelTypeFromPoint(point)].id; }
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

		public bool ContainsInList(List<Chunk> list) { return this.IndexInList(list) >= 0; }
		public int IndexInList(List<Chunk> list) 
		{
			for (int i = 0; i < list.Count; i++) 
			{ if (this.gameObject.name == list[i].gameObject.name) { return i; } }
			return -1;
		}

		public void PlayerRemoveVoxel(Player player, VoxelHit hit) { GameManager.instance.StartCoroutine(RemovingVoxel(player, hit)); }
		private IEnumerator RemovingVoxel(Player player, VoxelHit hit) 
		{
			if (player.isInteractingWithTerrain) { yield break; }
			player.isInteractingWithTerrain = true;

			bool isHand = false;
			ToolProperty toolProperty;
			try { toolProperty = GameManager.GetItemPropertyById(player.inventory.HandItem.id).toolProperty; }
			catch { toolProperty = null; }
			if (toolProperty == null) 
			{
				toolProperty = new ToolProperty();
				toolProperty.miningSpeed = 1f;
				toolProperty.miningForce = 1f;
				isHand = true;
			}

			if (!ContainsInList(TerrainManager.modifiedChunks)) { TerrainManager.modifiedChunks.Add(this); }
			Vector3Int position = GetVoxelPositionFromPoint(hit.point);
			player.destroyStage.SetPosition(this.position + position);

			bool exit = false;
			float time = toolProperty.miningSpeed * hit.property.hardness;

			for (uint stage = 0; stage < GameSettings.player.destroyStageLength; stage++) 
			{
				if (VoxelHit.Check(player.armature.head.transform.position, -player.armature.head.transform.right * GameSettings.player.reachingDistance, player, out VoxelHit newHit)) 
				{ if (GetVoxelPositionFromPoint(newHit.point) != position) { exit = true; break; } }

				if (Input.GetKeyUp(PlayerSettings.controlls.keyCodes.Attack)) 
				{ exit = true; break; }

				player.destroyStage.SetStage(stage);
				yield return new WaitForSeconds(time / (float)GameSettings.player.destroyStageLength);
			}

			player.destroyStage.Clear();
			if (exit) { player.isInteractingWithTerrain = false; yield break; }

			if (!isHand) 
			{
				if (player.inventory.HandSlot.item.durability > 1) { player.inventory.HandSlot.item.durability--; }
				else { player.inventory.HandSlot.item = Item.EmptyItem; }
				player.inventory.HandSlot.Update();
			}

			RemoveVoxel(position.x, position.y, position.z, false);
			for (int face = 0; face < 6; face++) 
			{
				int x = position.x;
				int y = position.y;
				int z = position.z;
				AddFaceToPosition(ref x, ref y, ref z, (VoxelFace)face);
				RemoveVoxel(x, y, z, true);
				AddVoxel(GetVoxelType(x, y, z), x, y, z);
			}

			hit.chunk.Update();

			Vector3 offset = (Vector3.one * 0.5f) + gameObject.transform.position;
			player.DropItem(new Item(hit.property.dropItem, 1), new Vector3((float)position.x, (float)position.y, (float)position.z) + offset);

			player.isInteractingWithTerrain = false;
		}

		public void PlayerPickVoxel(Player player, VoxelHit hit) 
		{
			if (player.isInteractingWithTerrain) { return; }
			player.isInteractingWithTerrain = true;

			int index = -1;
			for (int i = 0; i < player.inventory.slots.Count; i++) 
			{ if (player.inventory.slots[i].item.id == hit.property.id) { index = i; break; } }

			if (index >= 0) 
			{
				Item handItem = new Item(player.inventory.HandItem);
				player.inventory.HandItem = new Item(player.inventory.slots[index].item);
				player.inventory.slots[index].item = handItem;
				player.inventory.slots[index].Update();
			}

			player.isInteractingWithTerrain = false;
		}

		/* Check for avaliable rotations from the property. */
		/* Check if it is overlaping with other entities / players. */
		public void PlayerPlaceVoxel(Player player, VoxelHit hit) 
		{
			if (player.isInteractingWithTerrain) { return; }
			player.isInteractingWithTerrain = true;

			if (!ContainsInList(TerrainManager.modifiedChunks)) { TerrainManager.modifiedChunks.Add(this); }

			Vector3Int position = GetVoxelPositionFromPoint(hit.previousHit.point);
			InventorySlot slot = player.inventory.HandSlot;

			if (slot.IsEmpty || IsOutOfBounds(position.x, position.y, position.z)) 
			{ player.isInteractingWithTerrain = false; return; }

			uint type = GameManager.GetVoxelTypeById(slot.item.id);
			slot.item.ammount--;
			slot.Update();



			/* Rotate voxel. */
			VoxelProperty property = GameManager.voxelProperties[type];

			TestObject.position = hit.normal + position;

			matrices[position.x, position.y, position.z] = GetDefaultVoxelMatrix(position.x, position.y, position.z);






			AddVoxel(type, position.x, position.y, position.z);
			for (int face = 0; face < 6; face++) 
			{
				int x = position.x;
				int y = position.y;
				int z = position.z;
				AddFaceToPosition(ref x, ref y, ref z, (VoxelFace)face);
				RemoveVoxel(x, y, z, true);
				AddVoxel(GetVoxelType(x, y, z), x, y, z);
			}

			hit.chunk.Update();
			player.isInteractingWithTerrain = false;
		}

		public Chunk(Vector3 position, Transform parent, uint[] voxels = null, Matrix4x4[,,] matrices = null) 
		{
			this.liquidMeshes = new List<ChunkMesh>();
			this.liquidsToGenerate = new List<dynamic[]>();
			this.voxelTriangles = new int[ChunkSize, ChunkHeight, ChunkSize][];
			this.voxels = (voxels == null) ? (new uint[ChunkSize * ChunkHeight * ChunkSize]) : voxels;
			this.matrices = (matrices == null) ? (new Matrix4x4[ChunkSize, ChunkHeight, ChunkSize]) : matrices;
			this.objectMesh = new ObjectMesh();
			this.canUpdate = false;
			
			this.gameObject = new GameObject(position.ToString());
			this.gameObject.transform.SetParent(parent);
			this.gameObject.transform.position = position;
			this.gameObject.transform.eulerAngles = Vector3.zero;
			this.gameObject.transform.localScale = Vector3.one;
			this.gameObject.tag = "Chunk";
			this.gameObject.isStatic = true;
			this.position = this.gameObject.transform.position;
			this.name = this.gameObject.name;

			this.filter = this.gameObject.AddComponent<MeshFilter>();
			this.renderer = this.gameObject.AddComponent<MeshRenderer>();
			this.renderer.material = GameSettings.materials.chunk;
			this.renderer.material.SetTexture("_MainTex", GameSettings.textures.voxel);
			this.renderer.material.SetInt("chunkSize", ChunkSize);
			this.renderer.material.SetInt("chunkHeight", ChunkHeight);

			new Thread(new ThreadStart(() => 
				{
					for (int y = 0; y < ChunkHeight && matrices == null; y++) 
					{
						for (int z = 0; z < ChunkSize; z++) 
						{
							for (int x = 0; x < ChunkSize; x++) 
							{
								this.matrices[x, y, z] = GetDefaultVoxelMatrix(x, y, z);
							}
						}
					}

					this.GenerateVoxels();
					this.GenerateMesh();
					this.canUpdate = true;
				}
			)).Start();
			GameManager.instance.StartCoroutine(this.Update(false));
		}

		// public string ToJson() 
		// {
		// 	SavedData data = new SavedData(this.position, this.voxels, this.matrices);
		// 	string json = JsonUtility.ToJson(data, true);
		// 	json.Remove(json.IndexOf("{"), 1);
		// 	json.Remove(json.LastIndexOf("}"), 1);
		// 	return json;
		// }

		// public static void Save(string[] jsons) 
		// {
		// 	string output = "{\n\t\"content\":\n\t[\n";
		// 	for (int i = 0; i < jsons.Length; i++) { output += "\t\t" + jsons[i] + ((i < jsons.Length - 1) ? ",\n" : "\n"); }
		// 	output += "\t]\n}";
		// 	File.WriteAllText(GameManager.FormatPath(GameSettings.path.savedChunks), output);
		// }

		// public static void Load() 
		// {
		// 	SavedData[] data = JsonUtility.FromJson<ArrayWrapper<SavedData>>(File.ReadAllText(GameSettings.path.savedChunks)).content;
		// 	for (int i = 0; i < data.Length; i++) 
		// 	{
		// 		Chunk obj = new Chunk(data[i].position, TerrainManager.instance.gameObject.transform, data[i].voxels, SavedData.DeserializeMatrices(data[i].matrices));
		// 		TerrainManager.modifiedChunks.Add(obj);
		// 		TerrainManager.chunks.Add(obj);
		// 	}
		// }

		// public class SavedData 
		// {
		// 	public Vector3 position;
		// 	public uint[] voxels;
		// 	public Matrix4x4[] matrices;

		// 	public static Matrix4x4[] SerializeMatrices(Matrix4x4[,,] matrices) 
		// 	{
		// 		int index = 0;
		// 		Matrix4x4[] newMatrices = new Matrix4x4[ChunkSize * ChunkHeight * ChunkSize];
		// 		for (int y = 0; y < ChunkHeight; y++) 
		// 		{
		// 			for (int z = 0; z < ChunkSize; z++) 
		// 			{
		// 				for (int x = 0; x < ChunkSize; x++) 
		// 				{
		// 					newMatrices[index++] = matrices[x, y, z];
		// 				}
		// 			}
		// 		}

		// 		return newMatrices;
		// 	}

		// 	public static Matrix4x4[,,] DeserializeMatrices(Matrix4x4[] matrices) 
		// 	{
		// 		int index = 0;
		// 		Matrix4x4[,,] newMatrices = new Matrix4x4[ChunkSize, ChunkHeight, ChunkSize];
		// 		for (int y = 0; y < ChunkHeight; y++) 
		// 		{
		// 			for (int z = 0; z < ChunkSize; z++) 
		// 			{
		// 				for (int x = 0; x < ChunkSize; x++) 
		// 				{
		// 					newMatrices[x, y, z] = matrices[index++];
		// 				}
		// 			}
		// 		}

		// 		return newMatrices;
		// 	}

		// 	public SavedData(Vector3 position, uint[] voxels, Matrix4x4[,,] matrices) 
		// 	{
		// 		this.position = position;
		// 		this.voxels = voxels;
		// 		this.matrices = SavedData.SerializeMatrices(matrices);
		// 	}
		// }
	}
}