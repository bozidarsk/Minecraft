using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Minecraft 
{
	public class DestroyStage : MonoBehaviour
	{
		private MeshFilter filter;
		new private MeshRenderer renderer;

		void Start() 
		{
			filter = gameObject.GetComponent<MeshFilter>();
			renderer = gameObject.GetComponent<MeshRenderer>();
			renderer.material = new Material(Shader.Find("Minecraft/Default"));

			Mesh mesh = new Mesh();
			List<Vector3> vertices = new List<Vector3>(6 * 4);
			List<int> triangles = new List<int>(6 * 6);
			List<Vector2> uvs = new List<Vector2>(6 * 4);

			for (int f = 0; f < 6; f++) 
			{
				vertices.AddRange(new Vector3[] 
					{
						Chunk.blockVertices[f, 0],
						Chunk.blockVertices[f, 1],
						Chunk.blockVertices[f, 2],
						Chunk.blockVertices[f, 3]
					}
				);

				uvs.AddRange(new Vector2[] 
					{
						new Vector2(0f, 0f),
						new Vector2(1f, 0f),
						new Vector2(1f, 1f),
						new Vector2(0f, 1f)
					}
				);

				int index = vertices.Count - 4;
				triangles.AddRange(new int[] { index + 0, index + 3, index + 1, index + 1, index + 3, index + 2 });
			}

			mesh.vertices = vertices.ToArray();
			mesh.triangles = triangles.ToArray();
			mesh.uv = uvs.ToArray();
			mesh.RecalculateNormals();
			filter.mesh = mesh;

			SetStage(5);
		}

		public void SetPosition(Vector3 position) { gameObject.transform.position = position; }

		public void Clear() 
		{
			Texture2D texture = new Texture2D(16, 16);
			Color[] colors = new Color[16 * 16];
			for (int i = 0; i < colors.Length; i++) { colors[i] = new Color(0f, 0f, 0f, 0f); }
			texture.SetPixels(colors);
			texture.Apply();
			renderer.material.SetTexture("_MainTex", texture);
		}

		public void SetStage(uint stage) 
		{
			Texture2D texture = new Texture2D(1, 1);
			try { ImageConversion.LoadImage(texture, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.destroyStageTextures + "/stage" + stage.ToString() + ".png")), false); }
			catch { ImageConversion.LoadImage(texture, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.destroyStageTextures + "/stage0.png")), false); }
			GameManager.InitializeTexture(ref texture);
			renderer.material.SetTexture("_MainTex", texture);
		}
	}
}