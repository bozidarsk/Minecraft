using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace Minecraft 
{
	public static class Commands 
	{
		private static List<Command> commands = new List<Command>() 
		{
			new Command("/clear", Collections.Clear, new string[] { "player" }, new Type[] { typeof(string) }, null, true),
			new Command("/give", Collections.Give, new string[] { "player", "item", "ammount" }, new Type[] { typeof(string), typeof(string), typeof(uint) }, null, true),
			new Command("/remove", Collections.Remove, new string[] { "player", "item", "ammount" }, new Type[] { typeof(string), typeof(string), typeof(uint) }, null, true),
			new Command("/place", Collections.Place, new string[] { "voxel", "x", "y", "z" }, new Type[] { typeof(string), typeof(int), typeof(int), typeof(int) }, null, true),
			new Command("/setTextureEffect", Collections.SetTextureEffect, new string[] { "player", "effect" }, new Type[] { typeof(string), typeof(string) }, null, false),
			new Command("/removeTextureEffect", Collections.RemoveTextureEffect, new string[] { "player" }, new Type[] { typeof(string) }, null, false),
			new Command("/sizeof", Collections.SizeOf, new string[] { "type" }, new Type[] { typeof(string) }, "$(result)", false),
			new Command("/save", Collections.SaveGame, new string[] {}, new Type[] {}, "Game saved.", false),
			new Command("/calc", Collections.Calc, new string[] { "expression" }, new Type[] { typeof(string) }, "$(result)", false)
		};

		public static void Add(Command command) { commands.Add(command); }
		public static Command Get(string command) 
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

		public static string[] GetArgs(string input) 
		{
			List<string> args = new List<string>();
			string currentArg = "";
			bool escape = false;
			bool isInside = false;
			int i = 0;

			input = input.TrimStart(' ').TrimEnd(' ');

			while (i < input.Length) 
			{
				try { escape = input[i - 1] == '\\'; }
				catch { escape = false; }

				if (!escape && input[i] == '"') 
				{
					int t = i + 1;
					while (input[t] != '"' && t < input.Length) 
					{
						if (input[t] == '\\') { t++; currentArg += input[t++]; continue; }
						currentArg += input[t];
						t++;
					}

					args.Add(currentArg);
					currentArg = "";
					i = t + 1;
					continue;
				}

				if (input[i] != '\\' && !(!escape && input[i] == ' ')) { currentArg += input[i]; isInside = true; }
				if (!escape && input[i] == ' ') { isInside = false; }

				if ((!isInside && currentArg.TrimStart(' ') != "") || i >= input.Length - 1) 
				{
					args.Add(currentArg);
					currentArg = "";
					isInside = false;
					i++;
					continue;
				}

				i++;
			}

			return args.ToArray();
		}

		public static class Collections 
		{
			public static dynamic Clear(dynamic[] args) { GameManager.GetPlayerByName(args[0]).inventory.Clear(); return null; }
			public static dynamic Remove(dynamic[] args) { GameManager.GetPlayerByName(args[0]).inventory.RemoveItemAll(new Item(args[1], args[2])); return null; }
			public static dynamic Give(dynamic[] args) { GameManager.GetPlayerByName(args[0]).inventory.AddItemAll(new Item(args[1], args[2])); return null; }
			public static dynamic Place(dynamic[] args) { TerrainManager.AddVoxel(GameManager.GetVoxelTypeById(args[0]), args[1], args[2], args[3]); return null; }
			public static dynamic SetTextureEffect(dynamic[] args) { GameManager.GetPlayerByName(args[0]).postProcessing.SetTextureEffect(GameManager.textureEffects[args[1]]); return null; }
			public static dynamic RemoveTextureEffect(dynamic[] args) { GameManager.GetPlayerByName(args[0]).postProcessing.RemoveTextureEffect(); return null; }
			public static dynamic SizeOf(dynamic[] args) { return Tools.SizeOf(Type.GetType(args[0])); }
			public static dynamic SaveGame(dynamic[] args) { GameManager.SaveGame(); return null; }
			public static dynamic Calc(dynamic[] args) { return Utils.Calculators.BasicCalculator.Solve(args[0]); }
		}
	}
}