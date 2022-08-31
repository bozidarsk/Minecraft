using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public class MovementController : MonoBehaviour
	{
		private Transform center;
		private Vector3 gravity = Vector3.zero;
		private Vector3 offset;

		public float t { get { return Time.fixedDeltaTime; } }

		void Start() { center.position += offset; }

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
				chunk = TerrainManager.GetChunkFromPosition(center.position + movement);
				property = GameManager.voxelProperties[chunk.GetVoxelTypeFromPoint(center.position + movement)];
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
				chunk = TerrainManager.GetChunkFromPosition(center.position + movement);
				property = GameManager.voxelProperties[chunk.GetVoxelTypeFromPoint(center.position + movement)];
			} catch { return true; }

			return !property.useCollision;
		}

		public void Initialize(Transform center, Vector3 offset) 
		{
			this.center = center;
			this.offset = offset;
		}
	}
}