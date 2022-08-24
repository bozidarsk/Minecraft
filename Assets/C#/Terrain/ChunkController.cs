using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkController : MonoBehaviour
{
	[HideInInspector] public GameManager gameManager;
	[HideInInspector] public Chunk chunk;
	[Range(1, 5)] public int lineSpacing = 1;

	/* Add delay based on mining speed, force, etc. */
	/* Drop the item on the ground. */
	public void OnPlayerRemoveVoxel(Player player, VoxelHit hit) 
	{
		if (!chunk.ContainsInList(gameManager.modifiedChunks)) { gameManager.modifiedChunks.Add(chunk); }
		Vector3Int position = chunk.GetVoxelPositionFromPoint(hit.point);

		chunk.RemoveVoxel(position.x, position.y, position.z, false);
		for (int face = 0; face < 6; face++) 
		{
			int x = position.x;
			int y = position.y;
			int z = position.z;
			Chunk.AddFaceToPosition(ref x, ref y, ref z, (VoxelFace)face);
			chunk.RemoveVoxel(x, y, z, true);
			chunk.AddVoxel(chunk.GetVoxelType(x, y, z), x, y, z);
		}

		chunk.Update();

		Vector3 offset = (Vector3.one * 0.5f) + gameObject.transform.position;
		player.DropItem(new Item(hit.property.dropItem, 1), new Vector3((float)position.x, (float)position.y, (float)position.z) + offset);
	}

	/* Return the string id of the block. */
	public void OnPlayerPickVoxel(Player player, VoxelHit hit) 
	{
		Debug.Log("Picking: " + hit.property.id);
	}

	public void OnPlayerPlaceVoxel(Player player, VoxelHit hit) 
	{
		if (!chunk.ContainsInList(gameManager.modifiedChunks)) { gameManager.modifiedChunks.Add(chunk); }
		Vector3Int position = chunk.GetVoxelPositionFromPoint(hit.previousHit.point);

		/* Check if you can place on the hit block. */
		/* Check for avaliable rotations from the property. */
		/* Check if position + hit.normal is out of bounds. */

		chunk.AddVoxel(gameManager.GetVoxelTypeById("grass-block"), position.x, position.y, position.z);

		for (int face = 0; face < 6; face++) 
		{
			int x = position.x;
			int y = position.y;
			int z = position.z;
			Chunk.AddFaceToPosition(ref x, ref y, ref z, (VoxelFace)face);
			chunk.RemoveVoxel(x, y, z, true);
			chunk.AddVoxel(chunk.GetVoxelType(x, y, z), x, y, z);
		}

		chunk.Update();
	}
}