using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementController : MonoBehaviour
{
	[HideInInspector] public GameManager gameManager;
	[HideInInspector] public Transform center;

	public float t { get { return Time.fixedDeltaTime; } }
	private Vector3 gravity = Vector3.zero;

	public bool ApplyGravity(Vector3 gravity) 
	{
		this.gravity += gravity * t * t;
		if (!Move(this.gravity)) { this.gravity *= 0f; return false; }
		return true;
	}

	public bool CanApplyGravity(Vector3 gravity) 
	{
		this.gravity += gravity * t * t;
		if (!CanMove(this.gravity)) { this.gravity *= 0f; return false; }
		return true;
	}

	public bool Move(Vector3 movement) 
	{
		VoxelProperty property;
		Chunk chunk;

		try 
		{
			chunk = gameManager.terrainManager.GetChunkFromPosition(center.position + movement);
			property = gameManager.voxelProperties[chunk.GetVoxelTypeFromPoint(center.position + movement)];
		} catch { gameObject.transform.position += movement; return true; }

		gameObject.transform.position += (!property.useCollision) ? movement : Vector3.zero;
		return !property.useCollision;
	}

	public bool CanMove(Vector3 movement) 
	{
		VoxelProperty property;
		Chunk chunk;

		try 
		{
			chunk = gameManager.terrainManager.GetChunkFromPosition(center.position + movement);
			property = gameManager.voxelProperties[chunk.GetVoxelTypeFromPoint(center.position + movement)];
		} catch { return true; }

		return !property.useCollision;
	}

	public void Initialize(GameManager gameManager, Transform center) 
	{
		this.gameManager = gameManager;
		this.center = center;
	}
}