using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Minecraft 
{
	[System.Serializable]
	public struct PlayerSettingsObject 
	{
		public Settings.Sound sound;
		public Settings.Graphics graphics;
		public Settings.Controlls controlls;
	}

	public static class PlayerSettings 
	{
		public static Settings.Sound sound;
		public static Settings.Graphics graphics;
		public static Settings.Controlls controlls;

		public static void Initialize(PlayerSettingsObject playerSettings) 
		{
			PlayerSettings.sound = playerSettings.sound;
			PlayerSettings.graphics = playerSettings.graphics;
			PlayerSettings.controlls = playerSettings.controlls;
		}
	}
}