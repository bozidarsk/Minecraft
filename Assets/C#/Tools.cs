using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft 
{
	public static class Tools 
	{
		public static bool GetAnyBit(int x) { for (int i = 0; i < sizeof(int) * 8; i++) { if (x >> i == 0x1) { return true; } } return false; }

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

	public class PNG 
	{
		public readonly static string signature = "\x89PNG\x0d\x0a\x1a\x0a";
		public Chunk[] Chunks { private set; get; }
		public int Width { private set; get; }       // (4 bytes)
		public int Height { private set; get; }      // (4 bytes)
		public int Depth { private set; get; }       // (1 byte, values 1, 2, 4, 8, or 16)
		public int ColorType { private set; get; }   // (1 byte, values 0, 2, 3, 4, or 6)
		public int Compression { private set; get; } // (1 byte, value 0)
		public int Filter { private set; get; }      // (1 byte, value 0)
		public int Interlace { private set; get; }   // (1 byte, values 0 "no interlace" or 1 "Adam7 interlace") (13 data bytes total).[9]

		public PNG() {}
		public PNG(byte[] content) 
		{
			PNG image = PNG.Decode(content);
			this.Chunks = image.Chunks;
			this.Width = image.Width;
			this.Height = image.Height;
			this.Depth = image.Depth;
			this.ColorType = image.ColorType;
			this.Compression = image.Compression;
			this.Filter = image.Filter;
			this.Interlace = image.Interlace;
		}

		public static PNG Decode(byte[] content) 
		{
			PNG image = new PNG();
			List<Chunk> chunks = new List<Chunk>();

			int i = PNG.signature.Length;
			while (i < content.Length) 
			{
				chunks.Add(Chunk.GetChunk(content, i));
				i += 4 + 4 + chunks[chunks.Count - 1].Length + 4;
			}

			image.Chunks = chunks.ToArray();
			image.Width = (chunks[0].Data[0] << 24) + (chunks[0].Data[1] << 16) + (chunks[0].Data[2] << 8) + (chunks[0].Data[3] << 0);
			image.Height = (chunks[0].Data[4] << 24) + (chunks[0].Data[5] << 16) + (chunks[0].Data[6] << 8) + (chunks[0].Data[7] << 0);
			image.Depth = chunks[0].Data[8];
			image.ColorType = chunks[0].Data[9];
			image.Compression = chunks[0].Data[10];
			image.Filter = chunks[0].Data[11];
			image.Interlace = chunks[0].Data[12];

			return image;
		}

		public static byte[] Encode(PNG image) 
		{
			List<byte> content = new List<byte>();
			for (int b = 0; b < PNG.signature.Length; b++) { content.Add((byte)(PNG.signature[b] & 0xff)); }

			content.Add((byte)0x00);
			content.Add((byte)0x00);
			content.Add((byte)0x00);
			content.Add((byte)0x0d);
			content.Add((byte)0x49);
			content.Add((byte)0x48);
			content.Add((byte)0x44);
			content.Add((byte)0x52);
			for (int b = 3; b >= 0; b--) { content.Add((byte)((image.Width >> (b * 8)) & 0xff)); }
			for (int b = 3; b >= 0; b--) { content.Add((byte)((image.Height >> (b * 8)) & 0xff)); }
			content.Add((byte)image.Depth);
			content.Add((byte)image.ColorType);
			content.Add((byte)image.Compression);
			content.Add((byte)image.Filter);
			content.Add((byte)image.Interlace);
			content.Add((byte)0x8d);
			content.Add((byte)0x32);
			content.Add((byte)0xcf);
			content.Add((byte)0xbd);

			for (int i = 1; i < image.Chunks.Length; i++) 
			{
				for (int b = 3; b >= 0; b--) { content.Add((byte)((image.Chunks[i].Length >> (b * 8)) & 0xff)); }
				for (int b = 0; b < image.Chunks[i].Type.Length; b++) { content.Add(image.Chunks[i].Type[b]); }
				for (int b = 0; b < image.Chunks[i].Data.Length; b++) { content.Add(image.Chunks[i].Data[b]); }
				for (int b = 0; b < image.Chunks[i].Crc.Length; b++) { content.Add(image.Chunks[i].Crc[b]); }
			}

			return content.ToArray();
		}

		public class Chunk 
		{
			public int Length { set; get; }
			public byte[] Type { set; get; }
			public byte[] Data { set; get; }
			public byte[] Crc { set; get; }

			public override string ToString() 
			{
				string output = "";

				output += "Chunk:\n";
				output += "  Length: " + System.Convert.ToString(this.Length) + "\n";
				output += "  Type: " + System.Text.Encoding.UTF8.GetString(this.Type) + "\n";

				output += "  Data: { ";
				for (int i = 0; i < this.Length; i++) { string c = Tools.Hex(this.Data[i], true); output += ((c.Length == 1) ? "0" + c : c) + ((i >= this.Length - 1) ? " " : ", "); }
				output += "}\n";

				output += "  Crc: { ";
				for (int i = 0; i < this.Crc.Length; i++) { string c = Tools.Hex(this.Crc[i], true); output += ((c.Length == 1) ? "0" + c : c) + ((i >= 4 - 1) ? " " : ", "); }
				output += "}\n";

				return output;
			}

			public Chunk() {}
			public Chunk(int Length, byte[] Type, byte[] Data, byte[] Crc) 
			{
				this.Length = Length;
				this.Type = Type;
				this.Data = Data;
				this.Crc = Crc;
			}

			public static Chunk GetChunk(byte[] content, int index) 
			{
				Chunk chunk = new Chunk();
				chunk.Length = (content[index] << 24) + (content[index + 1] << 16) + (content[index + 2] << 8) + (content[index + 3] << 0);
				chunk.Type = Tools.GetBytesAt(content, index + 4, index + 7);
				chunk.Data = (chunk.Length > 0) ? Tools.GetBytesAt(content, index + 8, index + 8 + chunk.Length - 1) : new[] { (byte)0x00 };
				chunk.Crc = Tools.GetBytesAt(content, index + ((chunk.Length > 0) ? 9 : 8) + chunk.Length, index + ((chunk.Length > 0) ? 9 : 8) + chunk.Length + 3);
				return chunk;
			}
		}
	}
}