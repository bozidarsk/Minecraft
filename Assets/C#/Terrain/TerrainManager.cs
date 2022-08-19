using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Terrain;

public class TerrainManager : MonoBehaviour
{
	[HideInInspector] public GameManager gameManager;
	[HideInInspector] public List<Chunk> generatedChunks;

	void Start() 
	{
		gameManager = gameObject.GetComponent<GameManager>();
		generatedChunks = new List<Chunk>();

		StartCoroutine(GenerateChunks());
		// generatedChunks.Add(new Chunk(gameManager, new Vector3(0f, 0f, 0f), gameObject.transform));
	}

	public void AddVoxel(ushort type, int x, int y, int z) 
	{
		float size = (float)gameManager.gameSettings.terrain.chunkSize;
		Vector3 chunkPosition = new Vector3(
			(float)Mathf.FloorToInt((float)x / size),
			0f,
			(float)Mathf.FloorToInt((float)z / size)
		);

		Chunk currentChunk;
		try { currentChunk = generatedChunks.Where(c => c.position == chunkPosition).ToArray()[0]; }
		catch { generatedChunks.Add(new Chunk(gameManager, chunkPosition, gameObject.transform)); currentChunk = generatedChunks[generatedChunks.Count - 1]; }

		Vector3Int voxelPosition = new Vector3Int(
			(int)(Math2.GetDecimal(x) * size),
			y,
			(int)(Math2.GetDecimal(z) * size)
		);

		currentChunk.AddVoxel(type, voxelPosition.x, voxelPosition.y, voxelPosition.z);
		currentChunk.Update();
	}

	private IEnumerator GenerateChunks() 
	{
		for (int z = 0; z < 5 * gameManager.gameSettings.terrain.chunkSize; z += gameManager.gameSettings.terrain.chunkSize) 
		{
			for (int x = 0; x < 5 * gameManager.gameSettings.terrain.chunkSize; x += gameManager.gameSettings.terrain.chunkSize) 
			{
				generatedChunks.Add(new Chunk(gameManager, new Vector3((float)x, 0f, (float)z), gameObject.transform));
				yield return null;
			}
		}
	}
}