using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public static class ConstMeshData 
	{
		public static int[] blockTriangles(int lastVertIndex) { lastVertIndex -= 3; return new int[] { lastVertIndex + 0, lastVertIndex + 3, lastVertIndex + 1, lastVertIndex + 1, lastVertIndex + 3, lastVertIndex + 2 }; }
		public static readonly Vector3[][] blockVertices = 
		{
			new Vector3[] { new Vector3(0f, 1f, 0f), new Vector3(1f, 1f, 0f), new Vector3(1f, 1f, 1f), new Vector3(0f, 1f, 1f) },
			new Vector3[] { new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, 0f) },
			new Vector3[] { new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 1f, 1f) },
			new Vector3[] { new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, 1f), new Vector3(1f, 1f, 1f), new Vector3(1f, 1f, 0f) },
			new Vector3[] { new Vector3(1f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f), new Vector3(1f, 1f, 1f) },
			new Vector3[] { new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(1f, 1f, 0f), new Vector3(0f, 1f, 0f) }
		};



		public static readonly Vector3[] quadsVertices = 
		{
			new Vector3(0f, 0f, 0f),
			new Vector3(1f, 0f, 1f),
			new Vector3(1f, 1f, 1f),
			new Vector3(0f, 1f, 0f),
			new Vector3(1f, 0f, 0f),
			new Vector3(1f, 1f, 0f),
			new Vector3(0f, 1f, 1f),
			new Vector3(0f, 0f, 1f)
		};

		public static readonly int[] quadsTriangles = 
		{
			0, 1, 2, 2, 3, 0,
			4, 5, 6, 6, 7, 4
		};



		public static readonly Vector3[] stairsVertices = 
		{
			new Vector3(0f, 1f, 0f), new Vector3(0.5f, 1f, 0f), new Vector3(0.5f, 1f, 1f), new Vector3(0f, 1f, 1f),
			new Vector3(0.5f, 0.5f, 0f), new Vector3(1f, 0.5f, 0f), new Vector3(1f, 0.5f, 1f), new Vector3(0.5f, 0.5f, 1f),

			new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, 0f),

			new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), new Vector3(0f, 1f, 1f),

			new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, 1f), new Vector3(1f, 0.5f, 1f), new Vector3(1f, 0.5f, 0f),
			new Vector3(0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 1f), new Vector3(0.5f, 1f, 1f), new Vector3(0.5f, 1f, 0f),

			new Vector3(1f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 1f, 1f), 
			new Vector3(0.5f, 1f, 1f), new Vector3(0.5f, 0.5f, 1f), new Vector3(1f, 0.5f, 1f),
			new Vector3(0f, 0.5f, 1f),

			new Vector3(1f, 0f, 0f), new Vector3(0f, 0f, 0f), new Vector3(0f, 1f, 0f), 
			new Vector3(0.5f, 1f, 0f), new Vector3(0.5f, 0.5f, 0f), new Vector3(1f, 0.5f, 0f),
			new Vector3(0f, 0.5f, 0f)
		};

		public static readonly int[] stairsTriangles = 
		{
			0, 2, 1, 0, 3, 2,
			4, 6, 5, 4, 7, 6,

			8, 10, 9, 8, 11, 10,

			12, 15, 14, 12, 14, 13,

			16, 19, 18, 16, 18, 17,
			20, 23, 22, 20, 22, 21,

			1+24, 0+24, 6+24, 6+24, 0+24, 5+24, 6+24, 4+24, 2+24, 2+24, 4+24, 3+24,

			1+31, 6+31, 0+31, 6+31, 5+31, 0+31, 6+31, 2+31, 4+31, 2+31, 3+31, 4+31
		};

		public static readonly Vector2[] stairsUvs = 
		{
			new Vector2(0f, 1f), new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 1f),
			new Vector2(0f, 0.5f), new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f),

			new Vector2(1f, 1f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(1f, 0f),

			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f),

			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f),
			new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(1f, 1f), new Vector2(0f, 1f),

			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f),
			new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(0f, 0.5f),
			new Vector2(1f, 0.5f),

			new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 1f),
			new Vector2(0.5f, 1f), new Vector2(0.5f, 0.5f), new Vector2(1f, 0.5f),
			new Vector2(0f, 0.5f)
		};



		public static readonly Vector3[] slabVertices = 
		{
			new Vector3(0f, 0.5f, 0f), new Vector3(1f, 0.5f, 0f), new Vector3(1f, 0.5f, 1f), new Vector3(0f, 0.5f, 1f),
			new Vector3(0f, 0f, 0f), new Vector3(0f, 0f, 1f), new Vector3(1f, 0f, 1f), new Vector3(1f, 0f, 0f),
			new Vector3(0f, 0f, 1f), new Vector3(0f, 0f, 0f), new Vector3(0f, 0.5f, 0f), new Vector3(0f, 0.5f, 1f),
			new Vector3(1f, 0f, 0f), new Vector3(1f, 0f, 1f), new Vector3(1f, 0.5f, 1f), new Vector3(1f, 0.5f, 0f),
			new Vector3(1f, 0f, 1f), new Vector3(0f, 0f, 1f), new Vector3(0f, 0.5f, 1f), new Vector3(1f, 0.5f, 1f),
			new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(1f, 0.5f, 0f), new Vector3(0f, 0.5f, 0f)
		};

		public static readonly int[] slabTriangles = 
		{
			0, 3, 1, 1, 3, 2,
			4, 7, 5, 5, 7, 6,
			8, 11, 9, 9, 11, 10,
			12, 15, 13, 13, 15, 14,
			16, 19, 17, 17, 19, 18,
			20, 23, 21, 21, 23, 22
		};

		public static readonly Vector2[] slabUvs = 
		{
			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f),
			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 1f),
			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f),
			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f),
			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f),
			new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(1f, 0.5f), new Vector2(0f, 0.5f)
		};
	}
}