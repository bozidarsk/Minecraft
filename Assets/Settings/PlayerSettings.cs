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
}