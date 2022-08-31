using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

namespace Minecraft 
{
	public class ChatManager : MonoBehaviour
	{
		public static GameObject chatQueue;
		public static GameObject messagePrefab;
		public static ChatManager instance { private set; get; }

		private static List<string> allMessages;

		void Awake() { ChatManager.Initialize(this); }
		public static void Initialize(ChatManager instance) 
		{
			ChatManager.instance = instance;
			ChatManager.allMessages = new List<string>();
		}

		public static int MessagesCount { get { return ChatManager.allMessages.Count; } }
		public static string AllMessages 
		{
			get 
			{
				string output = "";
				for (int i = 0; i < ChatManager.allMessages.Count; i++) 
				{ output += ChatManager.allMessages[i] + ((i < ChatManager.allMessages.Count - 1) ? "\n" : ""); }
				return output;
			}
		}

		public static void Push(Player player, string request) 
		{
			if (request[0] == '/') { ExecuteCommand(player, request); return; }
			// string message = "<<color=\"#" + Tools.Hex(player.color, false) + "\">" + player.name + "</color>> " + request;
			string message = "<" + player.name + "> " + request;

			Console.Log(message);

			GameObject.Instantiate(messagePrefab, chatQueue.transform).name = message;
			ChatManager.allMessages.Add(message);
		}

		public static void ExecuteCommand(Player player, string request) 
		{
			string[] tokens = request.Split(' ');
			Command command = Commands.GetCommand(tokens[0]);

			if (command == null) { Push(player, "Command not found: " + tokens[0].Substring(1, tokens[0].Length - 1)); return; }
			if (command.argTypes.Length != tokens.Length - 1) { Push(player, "Invalid arguments, expected: " + command.args); return; }

			dynamic[] args = new dynamic[command.argTypes.Length];
			for (int i = 0; i < args.Length; i++) 
			{
				if (!Commands.TryParse(tokens[i + 1], command.argTypes[i], out dynamic result)) 
				{ Push(player, "Invalid arguments, expected: " + command.args); return; }

				args[i] = result;
			}

			command.Execute(args);
		}
	}
}