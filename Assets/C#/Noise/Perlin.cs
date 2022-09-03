using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public static partial class Noise 
	{
		public static class Perlin 
        {
            public static float[,] Map2D(int width, int height, Settings.PerlinNoise noise) 
            {
                float[,] noiseMap = new float[width, height];
                float maxNoiseHeight = float.MinValue;
                float minNoiseHeight = float.MaxValue;
                float halfWidth = width / 2f;
                float halfHeight = height / 2f;

                Vector2 offset = new Vector2(random.Next(-100000, 100000), random.Next(-100000, 100000)) + noise.offset;

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        float amplitude = 1;
                        float frequency = 1;
                        float noiseHeight = 0;
                        
                        for (int o = 0; o < noise.octaves; o++)
                        {
                            float pointX = (x - halfWidth) / noise.scale * frequency + offset.x;
                            float pointY = (y - halfHeight) / noise.scale * frequency + offset.y;

                            float noiseValue = Mathf.PerlinNoise(pointX, pointY) * 2 - 1;
                            noiseHeight += noiseValue * amplitude;

                            amplitude *= noise.persistance;
                            frequency *= noise.lacunarity;
                        }

                        if (noiseHeight > maxNoiseHeight) { maxNoiseHeight = noiseHeight; }
                        else if (noiseHeight < minNoiseHeight) { minNoiseHeight = noiseHeight; }

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

            public static float Value2D(Vector2 point, Settings.PerlinNoise noise) 
            {
                float amplitude = 1f;
                float frequency = 1f;
                float result = 0f;
                float maxValue = 0f;
                float minValue = 0f;

                for (int o = 0; o < noise.octaves; o++) 
                {
                    Vector2 samplePoint = (point / (noise.scale * frequency)) + noise.offset;
                    result += (Mathf.PerlinNoise(samplePoint.x, samplePoint.y) * 2f - 1f) * amplitude;
                    maxValue += (1f * 2f - 1f) * amplitude;
                    minValue += (-1f * 2f - 1f) * amplitude;
                    amplitude *= noise.persistance;
                    frequency *= noise.lacunarity;
                }

                result = Mathf.InverseLerp(minValue, maxValue, result);
                if (result < 0f) { result = 0f; }
                if (result > 1f) { result = 1f; }
                return result;
            }

            public static float Value3D(Vector3 point, Settings.PerlinNoise noise) 
            {
                float ab = Value2D(new Vector2(point.x, point.y), noise);
                float bc = Value2D(new Vector2(point.y, point.z), noise);
                float ac = Value2D(new Vector2(point.x, point.z), noise);

                float ba = Value2D(new Vector2(point.y, point.x), noise);
                float cb = Value2D(new Vector2(point.z, point.y), noise);
                float ca = Value2D(new Vector2(point.z, point.x), noise);

                float abc = ab + bc + ac + ba + cb + ca;
                return abc / 6f;
            }
        }
	}
}