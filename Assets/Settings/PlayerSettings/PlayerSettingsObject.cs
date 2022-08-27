using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	[CreateAssetMenu(fileName = "New Player Settings", menuName = "Player Settings")]
	public class PlayerSettingsObject : ScriptableObject
	{
		public Settings.Sound sound;
		public Settings.Graphics graphics;
		public Settings.Controlls controlls;

		void OnValidate() 
		{
			if (controlls.scrollSensitivity < 1f) { controlls.scrollSensitivity = 1f; }
			if (controlls.sensitivity.x < 0f) { controlls.sensitivity.x = 1f; }
			if (controlls.sensitivity.y < 0f) { controlls.sensitivity.y = 1f; }
			if (sound.soundVolume < 0f) { sound.soundVolume = 0f; }
			if (sound.musicVolume < 0f) { sound.musicVolume = 0f; }
			if (graphics.FOV < 1f) { graphics.FOV = 1f; }
			if (graphics.GUIScale < 1f) { graphics.GUIScale = 1f; }
			if (graphics.renderDistance < 1) { graphics.renderDistance = 1; }
			if (graphics.maxFPS < 1) { graphics.maxFPS = 1; }
		}
	}
}