using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	public class DestroyStage : MonoBehaviour
	{
		private Mesh blockMesh;
		private Mesh quadsMesh;

		private MeshFilter filter;
		new private MeshRenderer renderer;

		void Start() 
		{
			filter = gameObject.GetComponent<MeshFilter>();
			renderer = gameObject.GetComponent<MeshRenderer>();
			renderer.material = new Material(Shader.Find("Minecraft/DefaultTriplanar"));
			Clear();

			ObjectMesh tmp = new ObjectMesh();
			for (int f = 0; f < 6; f++) 
			{
				tmp.Add(new Vector3[] 
					{
						ConstMeshData.blockVertices[f][0],
						ConstMeshData.blockVertices[f][1],
						ConstMeshData.blockVertices[f][2],
						ConstMeshData.blockVertices[f][3]
					}
				);

				tmp.Add(new Vector2[] 
					{
						new Vector2(0f, 0f),
						new Vector2(1f, 0f),
						new Vector2(1f, 1f),
						new Vector2(0f, 1f)
					}
				);

				int index = tmp.vertexCount - 4;
				tmp.Add(new int[] { index + 0, index + 3, index + 1, index + 1, index + 3, index + 2 });
			}

			blockMesh = tmp.mesh;

			tmp = new ObjectMesh();
			tmp.Add(ConstMeshData.quadsVertices);
			tmp.Add(ConstMeshData.quadsTriangles);
			tmp.Add(new Vector2[] 
				{
					new Vector2(0f, 0f),
					new Vector2(1f, 0f),
					new Vector2(1f, 1f),
					new Vector2(0f, 1f),
					new Vector2(1f, 0f),
					new Vector2(1f, 1f),
					new Vector2(0f, 1f),
					new Vector2(0f, 0f)
				}
			);

			quadsMesh = tmp.mesh;
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
			Vector3Int position = new Vector3Int(
				(int)gameObject.transform.position.x,
				(int)gameObject.transform.position.y,
				(int)gameObject.transform.position.z
			);
			
			Chunk chunk = TerrainManager.GetChunkFromPosition(gameObject.transform.position);
			VoxelProperty property = GameManager.voxelProperties[chunk.GetVoxelType(position.x, position.y, position.z)];
			bool useUvs = true;

			if (property.id.EndsWith("-block")) { filter.mesh = blockMesh; }
			else if (property.id.EndsWith("-quads")) { filter.mesh = quadsMesh; }
			else if (property.id.EndsWith("-liquid")) { filter.mesh = blockMesh; }
			else { filter.mesh = blockMesh; }
			// else { filter.mesh = chunk.GetMeshFromVoxel(position.x, position.y, position.z).mesh16; useUvs = false; }

			Texture2D texture = new Texture2D(1, 1);
			try { ImageConversion.LoadImage(texture, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.destroyStageTextures + "/stage" + stage.ToString() + ".png")), false); }
			catch { ImageConversion.LoadImage(texture, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.destroyStageTextures + "/stage0.png")), false); }
			GameManager.InitializeTexture(ref texture);
			renderer.material.SetInt("_UseUvs", (useUvs) ? 1 : 0);
			renderer.material.SetTexture("_MainTex", texture);
		}
	}
}