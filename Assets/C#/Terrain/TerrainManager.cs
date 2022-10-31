using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

		void Update() 
		{
			for (int z = -(int)PlayerSettings.graphics.renderDistance / 2; z <= PlayerSettings.graphics.renderDistance / 2; z++) 
			{
				for (int x = -(int)PlayerSettings.graphics.renderDistance / 2; x <= PlayerSettings.graphics.renderDistance / 2; x++) 
				{
					Vector3 position = Player.instance.transform.position + new Vector3((float)x * Chunk.ChunkSize, 0f, (float)z * Chunk.ChunkSize);
					Chunk chunk = TerrainManager.GetChunkFromPosition(position, true);
					chunk.IsActive = true;
				}
			}

			for (int i = 0; i < TerrainManager.chunks.Count; i++) 
			{
				Vector3 position = new Vector3(Player.instance.transform.position.x, 0f, Player.instance.transform.position.z);
				Vector3 offset = new Vector3(Chunk.ChunkSize / 2f, 0f, Chunk.ChunkSize / 2f);

				if (Math.Distance(position, TerrainManager.chunks[i].position + offset) > PlayerSettings.graphics.renderDistance * Chunk.ChunkSize) 
				{ TerrainManager.chunks[i].IsActive = false; }

				if (Math.Distance(Player.instance.transform.position, TerrainManager.chunks[i].position + offset) > PlayerSettings.graphics.discardDistance * Chunk.ChunkSize) 
				{ GameObject.Destroy(TerrainManager.chunks[i].gameObject); TerrainManager.chunks.RemoveAt(TerrainManager.chunks[i].IndexInList(TerrainManager.chunks)); }
			}
		}

		public static Chunk GetChunkFromPosition(Vector3 position, bool createIfNull = false) 
		{
			position /= Chunk.ChunkSize;
			position.x = Math.Floor(position.x);
			position.z = Math.Floor(position.z);
			position *= Chunk.ChunkSize;
			position.y = 0f;

			for (int i = 0; i < chunks.Count; i++) { if (chunks[i].position == position) { return chunks[i]; } }
			if (createIfNull) { chunks.Add(new Chunk(position, TerrainManager.instance.gameObject.transform)); }
			return (createIfNull) ? chunks[chunks.Count - 1] : null;
		}

		public static void AddVoxel(uint type, int x, int y, int z) 
		{
			float size = (float)Chunk.ChunkSize;
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z));
			if (currentChunk == null) { return; }

			Vector3Int voxelPosition = new Vector3Int(
				(int)(Math.GetDecimal(x) * size),
				y,
				(int)(Math.GetDecimal(z) * size)
			);

			currentChunk.AddVoxel(type, voxelPosition.x, voxelPosition.y, voxelPosition.z);
			currentChunk.Update();
		}

		public static void RemoveVoxel(int x, int y, int z, bool saveType) 
		{
			float size = (float)Chunk.ChunkSize;
			Chunk currentChunk = GetChunkFromPosition(new Vector3((float)x, (float)y, (float)z));
			if (currentChunk == null) { return; }

			Vector3Int voxelPosition = new Vector3Int(
				(int)(Math.GetDecimal(x) * size),
				y,
				(int)(Math.GetDecimal(z) * size)
			);

			currentChunk.RemoveVoxel(voxelPosition.x, voxelPosition.y, voxelPosition.z, saveType);
			currentChunk.Update();
		}

		public static Settings.Biome GetBiomeFromPosition(Vector3 position) 
		{
			float noise = Noise.Perlin.Value2D(new Vector2(position.x, position.z), GameSettings.terrain.biomeMapNoise);
			for (int i = 0; i < GameSettings.terrain.biomes.Length; i++) 
			{ if (noise >= GameSettings.terrain.biomes[i].range.min && noise <= GameSettings.terrain.biomes[i].range.max) 
				{ return GameSettings.terrain.biomes[i]; } }
			return null;
		}

		public static Settings.Biome GetBiomeById(string id) 
		{ for (uint i = 0; i < GameSettings.terrain.biomes.Length; i++) 
			{ if (GameSettings.terrain.biomes[i].id == id) { return GameSettings.terrain.biomes[i]; } 
		} return null; }

		private static IEnumerator GenerateChunks() 
		{
			float size = Chunk.ChunkSize;
			TerrainManager.DoneGenerating = false;

			for (int z = 0; z < 5; z++) 
			{
				for (int x = 0; x < 5; x++) 
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