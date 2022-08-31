using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Minecraft 
{
	public class GameSettings 
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
			PNG image;

			image = new PNG(File.ReadAllBytes(GameSettings.path.voxelTextures));
			GameSettings.textures.voxel = new Texture2D(image.Width, image.Height);
			ImageConversion.LoadImage(GameSettings.textures.voxel, File.ReadAllBytes(GameSettings.path.voxelTextures), false);
			GameManager.InitializeTexture(ref GameSettings.textures.voxel);
			GameSettings.textures.voxelWidth = GameSettings.textures.voxel.width;
			GameSettings.textures.voxelHeight = GameSettings.textures.voxel.height;

			image = new PNG(File.ReadAllBytes(GameSettings.path.itemTextures));
			GameSettings.textures.item = new Texture2D(image.Width, image.Height);
			ImageConversion.LoadImage(GameSettings.textures.item, File.ReadAllBytes(GameSettings.path.itemTextures), false);
			GameManager.InitializeTexture(ref GameSettings.textures.item);
			GameSettings.textures.itemWidth = GameSettings.textures.item.width;
			GameSettings.textures.itemHeight = GameSettings.textures.item.height;

			image = new PNG(File.ReadAllBytes(GameSettings.path.liquidTextures));
			GameSettings.textures.liquid = new Texture2D(image.Width, image.Height);
			ImageConversion.LoadImage(GameSettings.textures.liquid, File.ReadAllBytes(GameSettings.path.liquidTextures), false);
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