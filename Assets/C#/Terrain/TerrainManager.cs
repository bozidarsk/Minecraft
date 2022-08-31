using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	public class TerrainManager : MonoBehaviour
	{
		public static List<Chunk> modifiedChunks;
		public static bool DoneGenerating { private set; get; }
		public static TerrainManager instance { private set; get; }
		private static Chunk[,] chunks;

		void Awake() { TerrainManager.Initialize(this); }
		public static void Initialize(TerrainManager instance) 
		{
			TerrainManager.instance = instance;
			TerrainManager.DoneGenerating = false;
			TerrainManager.modifiedChunks = new List<Chunk>();
			TerrainManager.chunks = new Chunk[10000, 10000];
		}

		void Start() 
		{
			StartCoroutine(TerrainManager.GenerateChunks());
			// Vector2Int index = GetIndexFromPosition(new Vector3(0f, 0f, 0f));
			// chunks[index.x, index.y] = new Chunk(new Vector3(0f, 0f, 0f), gameObject.transform);
		}

		public static Vector2Int GetIndexFromPosition(Vector3 position) 
		{
			return new Vector2Int(
				Mathf.FloorToInt(position.x / (float)GameSettings.world.chunkSize) + (TerrainManager.chunks.GetLength(0) / 2),
				Mathf.FloorToInt(position.z / (float)GameSettings.world.chunkSize) + (TerrainManager.chunks.GetLength(1) / 2)
			);
		}

		public static Chunk GetChunkFromPosition(Vector3 position, bool createIfNull = false) 
		{
			Vector2Int index = GetIndexFromPosition(position);

			if (TerrainManager.chunks[index.x, index.y] == null && createIfNull) 
			{ TerrainManager.chunks[index.x, index.y] = new Chunk(position, TerrainManager.instance.gameObject.transform); }

			return TerrainManager.chunks[index.x, index.y];
		}

		public static void AddVoxel(uint type, int x, int y, int z) 
		{
			float size = (float)GameSettings.world.chunkSize;
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z), true);

			Vector3Int voxelPosition = new Vector3Int(
				(int)(Math2.GetDecimal(x) * size),
				y,
				(int)(Math2.GetDecimal(z) * size)
			);

			currentChunk.AddVoxel(type, voxelPosition.x, voxelPosition.y, voxelPosition.z, Matrix4x4.identity);
			currentChunk.Update();
		}

		public static void RemoveVoxel(int x, int y, int z, bool saveType) 
		{
			float size = (float)GameSettings.world.chunkSize;
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z), true);

			Vector3Int voxelPosition = new Vector3Int(
				(int)(Math2.GetDecimal(x) * size),
				y,
				(int)(Math2.GetDecimal(z) * size)
			);

			currentChunk.RemoveVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, saveType);
			currentChunk.Update();
		}

		private static IEnumerator GenerateChunks() 
		{
			float size = GameSettings.world.chunkSize;
			TerrainManager.DoneGenerating = false;

			for (int z = 0; z < 5; z++) 
			{
				for (int x = 0; x < 5; x++) 
				{
					Vector3 position = new Vector3((float)x * size, 0f, (float)z * size);
					Vector2Int index = GetIndexFromPosition(position);
					TerrainManager.chunks[index.x, index.y] = new Chunk(position, TerrainManager.instance.gameObject.transform);
					yield return null;
				}
			}

			TerrainManager.DoneGenerating = true;
		}
	}
}