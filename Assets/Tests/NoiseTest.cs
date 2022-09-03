using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Minecraft;

namespace Minecraft.Test 
{
	[ExecuteInEditMode]
	public class NoiseTest : MonoBehaviour
	{
		#if UNITY_EDITOR

		public int width;
		public int height;
		public Settings.Biome biome;

		void OnDrawGizmos() 
		{
			for (int y = 0; y < height; y++) 
			{
				for (int x = 0; x < width; x++) 
				{
					float result = Noise.Perlin.Value2D(
						new Vector2((float)x, (float)y),
						biome.heightNoise
					);

					int h = (int)Mathf.Lerp(biome.height.min, biome.height.max, result);

					if (h < biome.seaLevel) { Gizmos.color = new Color(0f, 0f, 1f, 1f); }
					else { Gizmos.color = new Color(0f, 1f, 0f, 1f); }

					Gizmos.DrawCube(new Vector3((float)x, (float)h, (float)y), Vector3.one);
				}
			}
		}

		#endif
	}
}