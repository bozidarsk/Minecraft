using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public static class GameSettings 
	{
		public static Settings.Player player;
		public static Settings.PostProcessing postProcessing;
		public static Settings.Terrain terrain;
		public static GameManagerTextures textures;
		public static GameManagerMaterials materials;

		public static void Initialize(GameSettingsObject gameSettings, GameManagerTextures textures, GameManagerMaterials materials) 
		{
			GameSettings.player = gameSettings.player;
			GameSettings.postProcessing = gameSettings.postProcessing;
			GameSettings.terrain = gameSettings.terrain;
			GameSettings.textures = textures;
			GameSettings.materials = materials;
		}
	}
}