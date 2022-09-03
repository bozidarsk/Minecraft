using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Minecraft 
{
	[System.Serializable]
	public struct GameSettingsObject 
	{
		public Settings.Player player;
		public Settings.PostProcessing postProcessing;
		public Settings.World world;
		public Settings.Terrain terrain;
		public Settings.Textures textures;
		public Settings.Materials materials;
		public Settings.Path path;
	}

	public static class GameSettings 
	{
		public static Settings.Player player;
		public static Settings.PostProcessing postProcessing;
		public static Settings.World world;
		public static Settings.Terrain terrain;
		public static Settings.Textures textures;
		public static Settings.Materials materials;
		public static Settings.Path path;

		public static void Initialize(GameSettingsObject gameSettings) 
		{
			GameSettings.player = gameSettings.player;
			GameSettings.postProcessing = gameSettings.postProcessing;
			GameSettings.world = gameSettings.world;
			GameSettings.terrain = gameSettings.terrain;
			GameSettings.path = gameSettings.path;

			GameSettings.textures.voxel = new Texture2D(1, 1);
			ImageConversion.LoadImage(GameSettings.textures.voxel, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.voxelTextures)), false);
			GameManager.InitializeTexture(ref GameSettings.textures.voxel);
			GameSettings.textures.voxelWidth = GameSettings.textures.voxel.width;
			GameSettings.textures.voxelHeight = GameSettings.textures.voxel.height;

			GameSettings.textures.item = new Texture2D(1, 1);
			ImageConversion.LoadImage(GameSettings.textures.item, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.itemTextures)), false);
			GameManager.InitializeTexture(ref GameSettings.textures.item);
			GameSettings.textures.itemWidth = GameSettings.textures.item.width;
			GameSettings.textures.itemHeight = GameSettings.textures.item.height;

			GameSettings.textures.liquid = new Texture2D(1, 1);
			ImageConversion.LoadImage(GameSettings.textures.liquid, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.liquidTextures)), false);
			GameManager.InitializeTexture(ref GameSettings.textures.liquid);
			GameSettings.textures.liquidWidth = GameSettings.textures.liquid.width;
			GameSettings.textures.liquidHeight = GameSettings.textures.liquid.height;

			GameSettings.materials.chunk = new Material(Shader.Find(GameSettings.path.chunkShader));
			GameSettings.materials.postProcessing = new Material(Shader.Find(GameSettings.path.postProcessingShader));
			GameSettings.materials.droppedItem = new Material(Shader.Find(GameSettings.path.droppedItemShader));
			GameSettings.materials.player = new Material(Shader.Find(GameSettings.path.playerShader));
			GameSettings.materials.liquid = new Material(Shader.Find(GameSettings.path.liquidShader));
		}
	}
}