using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	[CreateAssetMenu(fileName = "New Game Settings", menuName = "Game Settings")]
	public class GameSettingsObject : ScriptableObject
	{
		public Settings.Player player;
		public Settings.PostProcessing postProcessing;
		public Settings.World world;
		public Settings.Terrain terrain;
		public Settings.Path path;
		public Settings.Textures textures;
		public Settings.Materials materials;

		void OnValidate() 
		{
			if (world.worldSize < 1) { world.worldSize = 1; }
			if (world.chunkSize < 1) { world.chunkSize = 1; }
			if (world.chunkHeight < 1) { world.chunkHeight = 1; }
			if (terrain.dirtDepth < 1) { terrain.dirtDepth = 1; }
			if (player.walkingSpeed < 0f) { player.walkingSpeed = 0f; }
			if (player.sprintingSpeed < 0f) { player.sprintingSpeed = 0f; }
			if (player.sneakingSpeed < 0f) { player.sneakingSpeed = 0f; }
			if (player.jumpSpeed < 0f) { player.jumpSpeed = 0f; }
			if (player.jumpHeight < 0f) { player.jumpHeight = 0f; }
			if (player.reachingDistance < 0f) { player.reachingDistance = 0f; }
			if (player.rotationLimit.x < 10f) { player.rotationLimit.x = 10f; }
			if (player.rotationLimit.x > 90f) { player.rotationLimit.x = 90f; }
			if (player.rotationLimit.y < 10f) { player.rotationLimit.y = 10f; }
			if (player.rotationLimit.y > 180f) { player.rotationLimit.y = 180f; }
		}
	}
}