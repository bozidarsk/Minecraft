using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	public static class Noise 
	{
		public static System.Random random;

        /*
		public static float Perlin2D(Vector2 point, Settings.Noise noise) 
		{
		    float maxNoiseHeight = float.MinValue;
		    float minNoiseHeight = float.MaxValue;
		    Vector2[] octaveOffset = new Vector2[noise.octaves];

		    for (int o = 0; o < noise.octaves; o++)
		    {
		        float offsetX = random.Next(-100000, 100000) + noise.offset.x;
		        float offsetY = random.Next(-100000, 100000) + noise.offset.y;
		        octaveOffset[o] = new Vector2(offsetX, offsetY);
		    }

            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;
            
            for (int o = 0; o < noise.octaves; o++)
            {
                float pointX = point.x / noise.scale * frequency + octaveOffset[o].x;
                float pointY = point.y / noise.scale * frequency + octaveOffset[o].y;

                float noiseValue = Mathf.PerlinNoise(pointX, pointY) * 2 - 1;
                noiseHeight += noiseValue * amplitude;

                amplitude *= noise.persistance;
                frequency *= noise.lacunarity;
            }

            if (noiseHeight > maxNoiseHeight) { maxNoiseHeight = noiseHeight; }
            else if (noiseHeight < minNoiseHeight) { minNoiseHeight = noiseHeight; }

            return Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseHeight);
		}
        */

        public static float Perlin2D(Vector2 point, Settings.Noise noise) 
        {
            point = (point * noise.scale) + noise.offset;
            float result = Mathf.PerlinNoise(point.x, point.x);

            if (result < 0f) { result = 0f; }
            if (result > 1f) { result = 1f; }
            return result;
        }

		public static float Perlin3D(Vector3 point, Settings.Noise noise) 
		{
			float ab = Perlin2D(new Vector2(point.x, point.y), noise);
			float bc = Perlin2D(new Vector2(point.y, point.z), noise);
			float ac = Perlin2D(new Vector2(point.x, point.z), noise);

			float ba = Perlin2D(new Vector2(point.y, point.x), noise);
			float cb = Perlin2D(new Vector2(point.z, point.y), noise);
			float ca = Perlin2D(new Vector2(point.z, point.x), noise);

			float abc = ab + bc + ac + ba + cb + ca;
			return abc / 6f;
		}

		public static void Initialize(System.Random random) 
		{
			Noise.random = random;
		}
	}
}

/*

public float[,] GenerateNoiseMap(int width, int height, int seed, float scale, int octaves, float persistance, float lacunarity, Vector2 offset)
{
    float[,] noiseMap = new float[width, height];
    float maxNoiseHeight = float.MinValue;
    float minNoiseHeight = float.MaxValue;
    System.Random random = new System.Random(seed);
    Vector2[] octaveOffset = new Vector2[octaves];
    float halfWidth = width / 2f;
    float halfHeight = height / 2f;

    for (int o = 0; o < octaves; o++)
    {
        float offsetX = random.Next(-100000, 100000) + offset.x;
        float offsetY = random.Next(-100000, 100000) + offset.y;
        octaveOffset[o] = new Vector2(offsetX, offsetY);
    }

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            float amplitude = 1;
            float frequency = 1;
            float noiseHeight = 0;
            
            for (int o = 0; o < octaves; o++)
            {
                float pointX = (x - halfWidth) / scale * frequency + octaveOffset[o].x;
                float pointY = (y - halfHeight) / scale * frequency + octaveOffset[o].y;

                float noiseValue = Mathf.PerlinNoise(pointX, pointY) * 2 - 1;
                noiseHeight += noiseValue * amplitude;

                amplitude *= persistance;
                frequency *= lacunarity;
            }

            if (noiseHeight > maxNoiseHeight)
            {
                maxNoiseHeight = noiseHeight;
            }
            else if (noiseHeight < minNoiseHeight) 
            {
                minNoiseHeight = noiseHeight;
            }

            noiseMap[x, y] = noiseHeight;
        }
    }

    for (int y = 0; y < height; y++)
    {
        for (int x = 0; x < width; x++)
        {
            noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
        }
    }

    return noiseMap;
}

*/