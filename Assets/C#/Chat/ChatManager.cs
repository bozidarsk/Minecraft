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
		public GameObject messagePrefab;
		public GameObject chatQueue;

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

		public static void Push(Player player, string request, bool sendToAll = true) 
		{
			if (request[0] == '/') { ExecuteCommand(player, request); return; }
			string message = ((sendToAll) ? player.FormatedName + " " : "") + request;

			Console.Log(message);

			GameObject.Instantiate(instance.messagePrefab, instance.chatQueue.transform).name = message;
			ChatManager.allMessages.Add(message);
		}

		public static void ExecuteCommand(Player player, string request) 
		{
			string[] tokens = request.Split(' ');
			Command command = Commands.Get(tokens[0]);

			if (command.requireAdmin && !player.isAdmin) { Push(player, "You need to be admin to use this command.", false); }
			if (command == null) { Push(player, "Command not found: " + tokens[0].Substring(1, tokens[0].Length - 1), false); return; }

			dynamic[] args = new dynamic[command.argTypes.Length];
			string[] formattedArgs = Commands.GetArgs(String2.GetStringAt(request, request.IndexOf(" ") + 1, request.Length - 1));

			if (command.argTypes.Length != formattedArgs.Length) { Push(player, "Invalid arguments, expected: " + command.args, false); return; }
			for (int i = 0; i < args.Length; i++) 
			{
				if (!Commands.TryParse(formattedArgs[i], command.argTypes[i], out dynamic result)) 
				{ Push(player, "Invalid arguments, expected: " + command.args, false); return; }

				args[i] = result;
			}

			allMessages.Add(player.FormatedName + " " + request);
			dynamic output = command.Execute(args);
			if (output != null && command.outputMessage != null) { Push(player, command.outputMessage.Replace("$(result)", output.ToString()), false); }
		}
	}
}