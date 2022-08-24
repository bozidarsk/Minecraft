using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

public class TerrainManager : MonoBehaviour
{
	[HideInInspector] public GameManager gameManager;
	private Chunk[,] chunks;

	void Start() 
	{
		gameManager = gameObject.GetComponent<GameManager>();
		chunks = new Chunk[10000, 10000];

		// StartCoroutine(GenerateChunks());
		Vector2Int index = GetIndexFromPosition(new Vector3(0f, 0f, 0f));
		chunks[index.x, index.y] = new Chunk(gameManager, new Vector3(0f, 0f, 0f), gameObject.transform);
	}

	public Vector2Int GetIndexFromPosition(Vector3 position) 
	{
		return new Vector2Int(
			Mathf.FloorToInt(position.x / (float)gameManager.gameSettings.terrain.chunkSize) + (chunks.GetLength(0) / 2),
			Mathf.FloorToInt(position.z / (float)gameManager.gameSettings.terrain.chunkSize) + (chunks.GetLength(1) / 2)
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
		float size = (float)gameManager.gameSettings.terrain.chunkSize;
		Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z), true);

		Vector3Int voxelPosition = new Vector3Int(
			(int)(Math2.GetDecimal(x) * size),
			y,
			(int)(Math2.GetDecimal(z) * size)
		);

		currentChunk.AddVoxel(type, voxelPosition.x, voxelPosition.y, voxelPosition.z);
		currentChunk.Update();
	}

	public void RemoveVoxel(int x, int y, int z, bool saveType) 
	{
		float size = (float)gameManager.gameSettings.terrain.chunkSize;
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
		float size = gameManager.gameSettings.terrain.chunkSize;
		for (int z = 0; z < 5; z++) 
		{
			for (int x = 0; x < 5; x++) 
			{
				Vector2Int index = GetIndexFromPosition(new Vector3((float)x, 0f, (float)z));
				chunks[index.x, index.y] = new Chunk(
					gameManager,
					new Vector3((float)x * size, 0f, (float)z * size),
					gameObject.transform
				);

				yield return null;
			}
		}
	}
}