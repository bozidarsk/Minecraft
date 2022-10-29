using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;

namespace Minecraft 
{
	public static class Tools 
	{
		public static bool GetAnyBit(int x) { for (int i = 0; i < sizeof(int) * 8; i++) { if (x >> i == 0x1) { return true; } } return false; }

		public static bool HasMember(Type type, string member) 
		{
			MemberInfo[] infos = type.GetMembers();
			for (int i = 0; i < infos.Length; i++) { if (infos[i].ToString() == member) { return true; } }
			return false;
		}

		public static int SizeOf(Type type) 
		{
			if (type == null) { throw new NullReferenceException("Type can not be null."); }
			var method = new DynamicMethod("SizeOfImpl", typeof(uint), new Type[0], typeof(Tools), false);

		    ILGenerator gen = method.GetILGenerator();

		    gen.Emit(OpCodes.Sizeof, type);
		    gen.Emit(OpCodes.Ret);

		    var func = (Func<uint>)method.CreateDelegate(typeof(Func<uint>));
		    return checked((int)func());
		}

		public static int[] CullTriangles(int[] triangles, Cull mode) { CullTriangles(ref triangles, mode); return triangles; }
		public static void CullTriangles(ref int[] triangles, Cull mode) 
		{
			if (triangles.Length % 3 != 0) { throw new ArgumentException("Triangle array must be multiple of 3."); }

			switch (mode) 
			{
				case Cull.Back:
					return;
				case Cull.Front:
					for (int i = 0; i < triangles.Length; i += 3) 
					{
						int tmp = triangles[i + 1];
						triangles[i + 1] = triangles[i + 2];
						triangles[i + 2] = tmp;
					}

					return;
				case Cull.Off:
					List<int> list = triangles.ToList();
					for (int i = 0; i < list.Count; i += 6) { list.InsertRange(i, new int[] { list[i + 0], list[i + 2], list[i + 1] }); }
					triangles = list.ToArray();
					return;
			}
		}

		public static string Hex(uint x, bool trimZeroes = false) 
		{
			string hexChar = "0123456789abcdef";
			string output = "";

			for (int i = 0; i < sizeof(uint) * 2; i++) 
			{ output = Convert.ToString(hexChar[(int)((x >> (i * 4)) & 0xf)]) + output; }

			return (trimZeroes) ? output.TrimStart('0') : output;
		}

		public static byte[] GetBytesAt(byte[] main, int startPos, int endPos) 
		{
		    if (main == null || startPos < 0 || endPos <= 0 || endPos < startPos || endPos >= main.Length) { return main; }

		    List<byte> output = new List<byte>((endPos - startPos) + 1);
		    int i = startPos;
		    int t = 0;
		    while (t < (endPos - startPos) + 1) 
		    {
		        output.Add(main[i]);
		        i++;
		        t++;
		    }

		    return output.ToArray();
		}
	}
}