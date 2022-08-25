using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public static class Console 
	{
		public static void WriteLine(dynamic input) { Log(input.ToString()); }
		public static void Log(dynamic input) { Log(input.ToString()); }
		private static void Log(string input) 
		{
			#if UNITY_EDITOR
			Debug.Log(input);
			#else
			#endif
		}

		public static void Warning(dynamic input) { Warning(input.ToString()); }
		private static void Warning(string input) 
		{
			#if UNITY_EDITOR
			Debug.LogWarning(input);
			#else
			#endif
		}

		public static void Error(dynamic input) { Error(input.ToString()); }
		private static void Error(string input) 
		{
			#if UNITY_EDITOR
			Debug.LogError(input);
			#else
			#endif
		}
	}
}