using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils;

public class ChatManager : MonoBehaviour
{
	public GameObject chatQueue;
	public GameObject messagePrefab;

	private List<string> allMessages;
	private GameManager gameManager;

	void Start() 
	{
		gameManager = gameObject.GetComponent<GameManager>();
		allMessages = new List<string>();
	}

	public int MessagesCount { get { return allMessages.Count; } }
	public string AllMessages 
	{
		get 
		{
			string output = "";
			for (int i = 0; i < allMessages.Count; i++) 
			{ output += allMessages[i] + ((i < allMessages.Count - 1) ? "\n" : ""); }
			return output;
		}
	}

	public void Push(Player player, string request) 
	{
		if (request[0] == '/') { ExecuteCommand(player, request); return; }
		// string message = "<<color=\"#" + Hex(player.color, false) + "\">" + player.name + "</color>> " + request;
		string message = "<" + player.name + "> " + request;

		Debug.Log(message);

		GameObject.Instantiate(messagePrefab, chatQueue.transform).name = message;
		allMessages.Add(message);
	}

	public void ExecuteCommand(Player player, string request) 
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

	public static string Hex(uint x, bool trimZeroes) 
	{
		string hexChar = "0123456789abcdef";
		string output = "";

		for (int i = 0; i < sizeof(uint) * 2; i++) 
		{ output = Convert.ToString(hexChar[(int)((x >> (i * 4)) & 0xf)]) + output; }

		return (trimZeroes) ? output.TrimStart('0') : output;
	}
}