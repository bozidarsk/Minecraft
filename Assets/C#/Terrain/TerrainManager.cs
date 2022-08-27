using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	public class TerrainManager : MonoBehaviour
	{
		[HideInInspector] public GameManager gameManager;
		[HideInInspector] public List<Chunk> modifiedChunks;
		[HideInInspector] public bool DoneGenerating { private set; get; }
		private Chunk[,] chunks;

		void Awake() { DoneGenerating = false; }
		void Start() 
		{
			gameManager = gameObject.GetComponent<GameManager>();
			modifiedChunks = new List<Chunk>();
			chunks = new Chunk[10000, 10000];

			StartCoroutine(GenerateChunks());
			// Vector2Int index = GetIndexFromPosition(new Vector3(0f, 0f, 0f));
			// chunks[index.x, index.y] = new Chunk(gameManager, new Vector3(0f, 0f, 0f), gameObject.transform);
		}

		public Vector2Int GetIndexFromPosition(Vector3 position) 
		{
			return new Vector2Int(
				Mathf.FloorToInt(position.x / (float)GameSettings.terrain.chunkSize) + (chunks.GetLength(0) / 2),
				Mathf.FloorToInt(position.z / (float)GameSettings.terrain.chunkSize) + (chunks.GetLength(1) / 2)
			);
		}

		public Chunk GetChunkFromPosition(Vector3 position, bool createIfNull = false) 
		{
			Vector2Int index = GetIndexFromPosition(position);

			if (chunks[index.x, index.y] == null && createIfNull) 
			{ chunks[index.x, index.y] = new Chunk(gameManager, position, gameObject.transform); }

			return chunks[index.x, index.y];
		}

		public void AddVoxel(ushort type, int x, int y, int z) 
		{
			float size = (float)GameSettings.terrain.chunkSize;
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z), true);

			Vector3Int voxelPosition = new Vector3Int(
				(int)(Math2.GetDecimal(x) * size),
				y,
				(int)(Math2.GetDecimal(z) * size)
			);

			currentChunk.AddVoxel(type, voxelPosition.x, voxelPosition.y, voxelPosition.z, Matrix4x4.identity);
			currentChunk.Update();
		}

		public void RemoveVoxel(int x, int y, int z, bool saveType) 
		{
			float size = (float)GameSettings.terrain.chunkSize;
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z), true);

			Vector3Int voxelPosition = new Vector3Int(
				(int)(Math2.GetDecimal(x) * size),
				y,
				(int)(Math2.GetDecimal(z) * size)
			);

			currentChunk.RemoveVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, saveType);
			currentChunk.Update();
		}

		private IEnumerator GenerateChunks() 
		{
			float size = GameSettings.terrain.chunkSize;
			for (int z = 0; z < 5; z++) 
			{
				for (int x = 0; x < 5; x++) 
				{
					Vector3 position = new Vector3((float)x * size, 0f, (float)z * size);
					Vector2Int index = GetIndexFromPosition(position);
					chunks[index.x, index.y] = new Chunk(gameManager, position, gameObject.transform);
					yield return null;
				}
			}

			DoneGenerating = true;
		}
	}
}