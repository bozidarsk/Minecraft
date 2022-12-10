#if !UNITY_EDITOR
#define ENABLE_CONSOLE
#endif

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
		[DllImport("kernel32.dll")] private static extern bool SetConsoleTitle(byte* lpConsoleTitle);
		[DllImport("kernel32.dll")] private static extern bool SetConsoleScreenBufferSize(void* hConsoleOutput, COORD dwSize);
		[DllImport("kernel32.dll")] private static extern bool SetConsoleActiveScreenBuffer(void* hConsoleOutput);
		[DllImport("kernel32.dll")] private static extern bool AllocConsole();
		[DllImport("kernel32.dll")] private static extern bool FreeConsole();

		[DllImport("kernel32.dll")] private static extern void* CreateFileA(
			byte* lpFileName,
			uint dwDesiredAccess,
			uint dwShareMode,
			SecurityAttributes* lpSecurityAttributes,
			uint dwCreationDisposition,
			uint dwFlagsAndAttributes,
			void* hTemplateFile
		);

		[DllImport("kernel32.dll")] private static extern void* CreateConsoleScreenBuffer(
			uint dwDesiredAccess,
			uint dwShareMode,
			SecurityAttributes* lpSecurityAttributes,
			uint dwFlags,
			void* lpScreenBufferData
		);

		[DllImport("kernel32.dll")] private static extern bool WriteConsole(
			void* hConsoleOutput,
			byte* lpBuffer,
			uint nNumberOfCharsToWrite,
			out uint* lpNumberOfCharsWritten,
			void* lpReserved
		);

		[DllImport("kernel32.dll")] private static extern bool ReadConsole(
			void* hConsoleInput,
			void* lpBuffer,
			uint nNumberOfCharsToRead,
			out uint* lpNumberOfCharsRead,
			void* pInputControl
		);

		public static void* Handle { private set; get; }
		public static void* ScreenBuffer { private set; get; }
		public static void* InputBuffer { private set; get; }

		private static string title;
		public static string Title 
		{
			set { title = value; fixed (byte* chars = &Encoding.ASCII.GetBytes(value + "\0")[0]) { SetConsoleTitle(chars); } }
			get { return title; }
		}

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
			#endif

			Detach();
			Attach();

			Console.Title = "Minecraft - Debug Console";

			#if ENABLE_CONSOLE
			Application.logMessageReceived += RedirectedLogs;
			SetActive(true);
			#else
			SetActive(false);
			#endif

			Write("How: ");
			// Debug.Log(ReadLine());
		}

		public static void SetActive(bool state) { ShowWindow(Handle, (state) ? 5 : 0); }

		public static void Detach() 
		{
			Handle = null;
			ScreenBuffer = null;
			InputBuffer = null;
			FreeConsole();
		}

		public static void Attach() 
		{
			AllocConsole();

			COORD size;
			size.x = 120;
			size.y = 9001;

			ScreenBuffer = CreateConsoleScreenBuffer(0x80000000 | 0x40000000, 0, null, 1, null);
			SetConsoleScreenBufferSize(ScreenBuffer, size);
			SetConsoleActiveScreenBuffer(ScreenBuffer);

			fixed (byte* name = &Encoding.ASCII.GetBytes("CONIN$\0")[0]) 
			{ InputBuffer = CreateFileA(name, 0x80000000 | 0x40000000, 0x00000001 | 0x00000002, null, 3, 0x80, null); }

			Handle = GetConsoleWindow();
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
					Exception(input, stackTrace);
					break;
				default:
					Log(input);
					break;
			}
		}

		public static string ReadLine() 
		{
			string output = "";
			for (char c = Read(); c != '\n'; c = Read()) { output += c.ToString(); }
			return output;
		}

		public static void Log(dynamic input) { Log(input.ToString()); }
		private static void Log(string input) 
		{
			#if ENABLE_CONSOLE
			Write(input + "\n");
			#endif

			#if !ENABLE_CONSOLE && UNITY_EDITOR
			Debug.Log(input);
			#endif
		}

		public static void Warning(dynamic input) { Warning(input.ToString()); }
		private static void Warning(string input) 
		{
			#if ENABLE_CONSOLE
			SetConsoleTextAttribute(ScreenBuffer, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_INTENSITY);
			Write(input + "\n");
			SetConsoleTextAttribute(ScreenBuffer, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
			#endif

			#if !ENABLE_CONSOLE && UNITY_EDITOR
			Debug.LogWarning(input);
			#endif
		}

		public static void Error(dynamic input) { Error(input.ToString()); }
		private static void Error(string input) 
		{
			#if ENABLE_CONSOLE
			SetConsoleTextAttribute(ScreenBuffer, FOREGROUND_RED | FOREGROUND_INTENSITY);
			Write(input + "\n");
			SetConsoleTextAttribute(ScreenBuffer, FOREGROUND_RED | FOREGROUND_GREEN | FOREGROUND_BLUE);
			#endif

			#if !ENABLE_CONSOLE && UNITY_EDITOR
			Debug.LogError(input);
			#endif
		}

		private static void Exception(string input, string stackTrace) 
		{
			#if ENABLE_CONSOLE
			Error(input);
			Error(stackTrace);
			#endif
		}

		private static char Read() 
		{
			fixed (byte* output = &(new byte[1])[0]) 
			{
				ReadConsole(InputBuffer, output, 1, out uint* charsRead, null);
				return Encoding.ASCII.GetString(new byte[] { *output })[0];
			}
		}

		private static void Write(string input) 
		{
			fixed (byte* chars = &Encoding.ASCII.GetBytes(input + "\0")[0]) 
			{
				WriteConsole(ScreenBuffer, chars, (uint)input.Length, out uint* charsWritten, null);
			}
		}
	}
}