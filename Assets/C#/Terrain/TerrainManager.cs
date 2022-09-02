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
		public static TerrainManager instance { private set; get; }
		public static bool DoneGenerating { private set; get; }
		public static List<Chunk> chunks;

		void Awake() { TerrainManager.Initialize(this); }
		public static void Initialize(TerrainManager instance) 
		{
			TerrainManager.instance = instance;
			TerrainManager.DoneGenerating = false;
			TerrainManager.modifiedChunks = new List<Chunk>();
			TerrainManager.chunks = new List<Chunk>();
		}

		void Start() 
		{
			StartCoroutine(TerrainManager.GenerateChunks());
			// chunks.Add(new Chunk(new Vector3(0f, 0f, 0f), TerrainManager.instance.gameObject.transform));
		}

		public static Chunk GetChunkFromPosition(Vector3 position, bool createIfNull = false) 
		{
			position /= GameSettings.world.chunkSize;
			position.x = Math2.Floor(position.x);
			position.z = Math2.Floor(position.z);
			position *= GameSettings.world.chunkSize;
			position.y = 0f;

			for (int i = 0; i < chunks.Count; i++) { if (chunks[i].position == position) { return chunks[i]; } }
			if (createIfNull) { chunks.Add(new Chunk(position, TerrainManager.instance.gameObject.transform)); }
			return (createIfNull) ? chunks[chunks.Count - 1] : null;
		}

		public static void AddVoxel(uint type, int x, int y, int z) 
		{
			float size = (float)GameSettings.world.chunkSize;
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z));
			if (currentChunk == null) { return; }

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
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z));
			if (currentChunk == null) { return; }

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

			for (int z = 0; z < 2; z++) 
			{
				for (int x = 0; x < 2; x++) 
				{
					Vector3 position = new Vector3((float)x * size, 0f, (float)z * size);
					TerrainManager.chunks.Add(new Chunk(position, TerrainManager.instance.gameObject.transform));
					yield return null;
				}
			}

			TerrainManager.DoneGenerating = true;
		}
	}
}