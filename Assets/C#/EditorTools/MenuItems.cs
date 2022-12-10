#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Compilation;

namespace Minecraft.UnityEditor 
{
	public static class MenuItems 
	{
		[MenuItem("Tools/Reload")]
		public static void Reload() { AssetDatabase.Refresh(); CompilationPipeline.RequestScriptCompilation(); }
	}
}
#endif