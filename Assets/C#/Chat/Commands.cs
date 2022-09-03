using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Minecraft 
{
	public static class Commands 
	{
		private static List<Command> commands = new List<Command>() 
		{
			new Command("/clear", Commands.Collections.Clear, new string[] { "player" }, new Type[] { typeof(string) }),
			new Command("/give", Commands.Collections.Give, new string[] { "player", "item", "ammount" }, new Type[] { typeof(string), typeof(string), typeof(uint) }),
			new Command("/remove", Commands.Collections.Remove, new string[] { "player", "item", "ammount" }, new Type[] { typeof(string), typeof(string), typeof(uint) }),
			new Command("/place", Commands.Collections.Place, new string[] { "voxel", "x", "y", "z" }, new Type[] { typeof(string), typeof(int), typeof(int), typeof(int) }),
			new Command("/setTextureEffect", Commands.Collections.SetTextureEffect, new string[] { "player", "effect" }, new Type[] { typeof(string), typeof(string) }),
			new Command("/removeTextureEffect", Commands.Collections.RemoveTextureEffect, new string[] { "player" }, new Type[] { typeof(string) })
		};

		public static void AddCommand(Command command) { commands.Add(command); }
		public static Command GetCommand(string command) 
		{
			try { return commands.Where(x => x.name == command || x.name.TrimStart('/') == command).ToArray()[0]; }
			catch { return null; }
		}

		public static bool TryParse(string input, Type type, out dynamic result) 
		{
			result = null;
			bool o = false;
			if (type == typeof(string)) { result = input; return true; }
			if (type == typeof(char)) { o = char.TryParse(input, out char r); result = r; return o; }
			if (type == typeof(bool)) { o = bool.TryParse(input, out bool r); result = r; return o; }
			if (type == typeof(float)) { o = float.TryParse(input, out float r); result = r; return o; }
			if (type == typeof(decimal)) { o = decimal.TryParse(input, out decimal r); result = r; return o; }
			if (type == typeof(double)) { o = double.TryParse(input, out double r); result = r; return o; }
			if (type == typeof(long)) { o = long.TryParse(input, out long r); result = r; return o; }
			if (type == typeof(ulong)) { o = ulong.TryParse(input, out ulong r); result = r; return o; }
			if (type == typeof(int)) { o = int.TryParse(input, out int r); result = r; return o; }
			if (type == typeof(uint)) { o = uint.TryParse(input, out uint r); result = r; return o; }
			if (type == typeof(short)) { o = short.TryParse(input, out short r); result = r; return o; }
			if (type == typeof(ushort)) { o = ushort.TryParse(input, out ushort r); result = r; return o; }
			if (type == typeof(byte)) { o = byte.TryParse(input, out byte r); result = r; return o; }
			if (type == typeof(sbyte)) { o = sbyte.TryParse(input, out sbyte r); result = r; return o; }
			return o;
		}

		public static string TypeToString(Type type) 
		{
			if (type == typeof(string)) { return "string"; }
			if (type == typeof(char)) { return "char"; }
			if (type == typeof(bool)) { return "bool"; }
			if (type == typeof(float)) { return "float"; }
			if (type == typeof(decimal)) { return "decimal"; }
			if (type == typeof(double)) { return "double"; }
			if (type == typeof(long)) { return "long"; }
			if (type == typeof(ulong)) { return "ulong"; }
			if (type == typeof(int)) { return "int"; }
			if (type == typeof(uint)) { return "uint"; }
			if (type == typeof(short)) { return "short"; }
			if (type == typeof(ushort)) { return "ushort"; }
			if (type == typeof(byte)) { return "byte"; }
			if (type == typeof(sbyte)) { return "sbyte"; }
			return null;
		}

		public static partial class Collections 
		{
			public static void Clear(dynamic[] args) 
			{
				Player.instance.inventory.Clear();
			}

			public static void Remove(dynamic[] args) 
			{
				Player.instance.inventory.TryRemoveItem(new Item(args[1], args[2]));
			}

			public static void Give(dynamic[] args) 
			{
				Player.instance.inventory.TryAddItem(new Item(args[1], args[2]));
			}

			public static void Place(dynamic[] args) 
			{
				TerrainManager.AddVoxel(GameManager.GetVoxelTypeById(args[0]), args[1], args[2], args[3]);
			}

			public static void SetTextureEffect(dynamic[] args) 
			{
				Player player = Player.instance;
				player.postProcessing.SetTextureEffect(GameManager.textureEffects[args[1]]);
			}

			public static void RemoveTextureEffect(dynamic[] args) 
			{
				Player.instance.postProcessing.RemoveTextureEffect();
			}
		}
	}
}