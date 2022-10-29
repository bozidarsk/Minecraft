using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Minecraft 
{
	/* https://docs.microsoft.com/en-us/windows/console/console-functions */
	public unsafe static class Console 
	{
		[DllImport("user32.dll")] private static extern bool ShowWindow(void* hWnd, int nCmdShow);
		[DllImport("kernel32.dll")] private static extern void* GetConsoleWindow();
		[DllImport("kernel32.dll")] private static extern bool SetConsoleTextAttribute(void* hConsoleOutput, ushort wAttributes);
		[DllImport("kernel32.dll")] private static extern bool SetConsoleTitle(void* lpConsoleTitle);
		[DllImport("kernel32.dll")] private static extern bool SetConsoleScreenBufferSize(void* hConsoleOutput, COORD dwSize);
		[DllImport("kernel32.dll")] private static extern bool SetConsoleActiveScreenBuffer(void* hConsoleOutput);
		[DllImport("kernel32.dll")] private static extern bool AllocConsole();
		[DllImport("kernel32.dll")] private static extern bool FreeConsole();

		[DllImport("kernel32.dll")] private static extern void* CreateConsoleScreenBuffer(
			uint dwDesiredAccess,
			uint dwShareMode,
			SecurityAttributes* lpSecurityAttributes,
			uint dwFlags,
			void* lpScreenBufferData
		);

		[DllImport("kernel32.dll")] private static extern bool WriteConsole(
			void* hConsoleOutput,
			void* lpBuffer,
			uint nNumberOfCharsToWrite,
			out uint* lpNumberOfCharsWritten,
			void* lpReserved
		);

		public static void* Handle { private set; get; }
		public static void* Buffer { private set; get; }

		private const ushort FOREGROUND_BLUE      = 0x0001;
		private const ushort FOREGROUND_GREEN     = 0x0002;
		private const ushort FOREGROUND_RED       = 0x0004;
		private const ushort FOREGROUND_INTENSITY = 0x0008;
		private const ushort BACKGROUND_BLUE      = 0x0010;
		private const ushort BACKGROUND_GREEN     = 0x0020;
		private const ushort BACKGROUND_RED       = 0x0040;
		private const ushort BACKGROUND_INTENSITY = 0x0080;

		public static void Initialize() 
		{
			#if UNITY_EDITOR
			EditorApplication.playModeStateChanged += (PlayModeStateChange change) => 
			{ if (change == PlayModeStateChange.ExitingPlayMode) { Detach(); } };
			#else
			Application.logMessageReceived += RedirectedLogs;
			#endif

			Detach();
			Attach();

			fixed (byte* chars = &Encoding.ASCII.GetBytes("Minecraft - Debug Console\x0000")[0]) { SetConsoleTitle(chars); }

			#if UNITY_EDITOR
			SetActive(false);
			#else
			SetActive(true);
			#endif
		}

		public static void SetActive(bool state) { ShowWindow(Handle, (state) ? 5 : 0); }

		public static void Detach() 
		{
			Handle = null;
			Buffer = null;
			FreeConsole();
		}

		public static void Attach() 
		{
			AllocConsole();

			COORD size;
			size.x = 120;
			size.y = 30;

			Buffer = CreateConsoleScreenBuffer(0x80000000 | 0x40000000, 0, null, 1, null);
			SetConsoleScreenBufferSize(Buffer, size);
			SetConsoleActiveScreenBuffer(Buffer);

			Handle = GetConsoleWindow();
		}

		public static void Crash() 
		{
			SetActive(true);
			#if UNITY_EDITOR
			EditorApplication.isPlaying = false;
			#else
			Environment.Exit(1);
			#endif
		}

		private static void RedirectedLogs(string input, string stackTrace, LogType type) 
		{
			switch (type) 
			{
				case LogType.Log:
					Log(input);
					break;
				case LogType.Warning:
					Warning(input);
					break;
				case LogType.Error:
					Error(input);
					break;
				case LogType.Exception:
					Exception(new ErrorException(input));
					break;
				default:
					Log(input);
					break;
			}
		}

		public static void Log(dynamic input) { Log(input.ToString()); }
		private static void Log(string input) 
		{
			#if UNITY_EDITOR
			Debug.Log(input);
			#else
			Write(input + "\n");
			#endif
		}

		public static void Warning(dynamic input) { Warning(input.ToString()); }
		private static void Warning(string input) 
		{
			#if UNITY_EDITOR
			Debug.LogWarning(input);
			#else
			SetConsoleTextAttribute(Buffer, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_INTENSITY);
			Write(input + "\n");
			SetConsoleTextAttribute(Buffer, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
			#endif
		}

		public static void Error(dynamic input) { Error(input.ToString()); }
		private static void Error(string input) 
		{
			#if UNITY_EDITOR
			Debug.LogError(input);
			#else
			SetConsoleTextAttribute(Buffer, FOREGROUND_RED | FOREGROUND_INTENSITY);
			Write(input + "\n");
			SetConsoleTextAttribute(Buffer, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
			#endif
		}

		public static void Exception(System.Exception exception) 
		{
			#if UNITY_EDITOR
			Debug.LogException(exception);
			#else
			string stack = "";
			List<string> list = System.Environment.StackTrace.Split('\x000a').ToList();
			for (int i = 2; i < list.Count; i++) { stack += list[i] + ((i < list.Count - 1) ? "\n" : ""); }

			Error(exception.ToString());
			Error(stack);
			#endif
		}

		private static void Write(string input) 
		{
			fixed (byte* chars = &Encoding.ASCII.GetBytes(input + "\x0000")[0]) 
			{
				WriteConsole(Buffer, chars, (uint)input.Length, out uint* charsWritten, null);
			}
		}
	}
}