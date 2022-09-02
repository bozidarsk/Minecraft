using System;
using System.Collections;
using System.Collections.Generic;
using Utils;
using UnityEngine;

namespace Minecraft 
{
	public static class Calculator 
	{
		private static char trigType = 'r';
		private static char[] allowedChars = { '.', ',', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '^', '√', '*', '/', '+', '-', '(', ')', '!', '%' };
		private static char[] operators = { '^', '*', '/', '+', '-', '%', '√' };
		private static string[] functions = { "&(", "sin(", "cos(", "tan(", "asin(", "acos(", "atan(", "sqrt(", "abs(", "ceiling(", "floor(", "min(", "max(", "lerp(", "inverselerp(", "log(", "clamp(" }; // '&' brackets are calculated as function, it just needs a name
		private static string ERRMSG = "Invalid input.";

		private static float itof(int x) { return float.Parse(Convert.ToString(x)); }
		private static int ftoi(float x) { return Convert.ToInt32(x); }
		private static float stof(string x) { return float.Parse(x); }
		private static string ftos(float x) { return Convert.ToString(x); }
		private static int stoi(string x) { return Convert.ToInt32(x); }
		private static string itos(int x) { return Convert.ToString(x); }

		private static string GetStringAt(string main, int startPos, int endPos) 
	    {
	        int i = startPos;
	        string output = "";

	        if (startPos < 0 || endPos <= 0 || endPos < startPos || endPos >= main.Length) { return null; }
	        if (startPos == endPos) { return Convert.ToString(main[startPos]); }
	        if (startPos == -1) { startPos = 0; }
	        if (endPos == -1) { endPos = main.Length - 1; }

	        while (i < main.Length && i <= endPos) 
	        {
	            output += main[i];
	            i++;
	        }

	        return output;
	    }

	    private static int CountChar(string main, char target) 
		{
			int i = 0;
			int count = 0;
			while (i < main.Length)
			{
			    if (main[i] == target) { count++; }
			    i++;
			}

			return count;
		}

		private static string FormatNumber(string number) 
		{
			if (number == ERRMSG) { return ERRMSG; }
			string result = "";

			if (!number.Contains("!") && !number.Contains("√")) { return number; }
	    	else 
	    	{
	    		if (number.Contains("!")) 
	    		{
	    			if (number[number.Length - 1] != '!') { return ERRMSG; }
	    			number = number.Remove(number.Length - 1, 1);
	    			result = Math2.Fact(stoi(number)).ToString();
	    		}

	    		if (number.Contains("√")) 
	    		{
	    			if (number[0] != '√') { return ERRMSG; }
	    			if (number[0] == '-') { return ERRMSG; }
	    			number = number.Remove(0, 1);
	    			result = ftos(Math2.Sqrt(stof(number)));
	    		}
	    	}

	    	return result;
		}

	    private static string Calculate(string content) 
	    {
	    	if (content == ERRMSG) { return ERRMSG; }
	    	if (content.Contains("(")) { content = CheckForFunctions(content); }

	    	List<string> _numbers = new List<string>();
	    	List<char> _operators = new List<char>();
	    	float result = 0f;
	    	string number = "";
	    	int i = 0;

	    	while (i < content.Length) 
	    	{
	    		if (Array.IndexOf(operators, content[i]) < 0 || ((content[i] == '√' || content[i] == '-') && (i == 0 || Array.IndexOf(operators, content[i - 1]) >= 0))) 
	    		{
	    			number += content[i];
	    			if (i + 1 >= content.Length) { _numbers.Add(number); }
				}
	    		else 
	    		{
	    			_numbers.Add(number);
	    			_operators.Add(content[i]);
	    			number = "";
	    		}

	    		i++;
	    	}

	    	if (_numbers.Count - 1 != _operators.Count && _numbers.Count > 1) { return ERRMSG; }

	    	result = stof(FormatNumber(_numbers[0]));

	    	i = 1;
	    	while (i < _numbers.Count) 
	    	{
	    		switch (_operators[i - 1]) 
	    		{
	    			case '^':
	    				_numbers[i] = FormatNumber(_numbers[i]);
	    				result = Math2.Pow(result, stof(_numbers[i]));
	    				break;
	    			case '√':
	    				_numbers[i] = FormatNumber(_numbers[i]);
	    				if(_numbers[i][0] == '-') { return ERRMSG; }
	    				result = Math2.Pow(stof(_numbers[i]), 1f / result);
	    				break;
	    			case '*':
	    				_numbers[i] = FormatNumber(_numbers[i]);
	    				result *= stof(_numbers[i]);
	    				break;
	    			case '/':
	    				_numbers[i] = FormatNumber(_numbers[i]);
	    				if (stof(_numbers[i]) == 0f) { return ERRMSG; }
	    				result /= stof(_numbers[i]);
	    				break;
	    			case '+':
	    				_numbers[i] = FormatNumber(_numbers[i]);
	    				result += stof(_numbers[i]);
	    				break;
	    			case '-':
	    				_numbers[i] = FormatNumber(_numbers[i]);
	    				result -= stof(_numbers[i]);
	    				break;
	    			case '%':
	    				_numbers[i] = FormatNumber(_numbers[i]);
	    				if (stof(_numbers[i]) == 0f) { return ERRMSG; }
	    				result = result % stof(_numbers[i]);
	    				break;
	    		}

	    		i++;
	    	}

	    	return ftos(result);
	    }

		private static string CalculateFunction(string content, string type) 
		{
			if (content == ERRMSG) { return ERRMSG; }
			string[] args = content.Split(',');
			float calculated = stof(Calculate(args[0]));
			switch (type) 
			{
				case "&(":
					if (args.Length != 1) { return ERRMSG; }
					return ftos(calculated);
				case "sin(":
					if (args.Length != 1) { return ERRMSG; }
					if (trigType == 'd') { return ftos(Math2.Sin(calculated * Math2.DEG2RAD) * Math2.RAD2DEG); }
					else { return ftos(Math2.Sin(calculated)); }
				case "cos(":
					if (args.Length != 1) { return ERRMSG; }
					if (trigType == 'd') { return ftos(Math2.Cos(calculated * Math2.DEG2RAD) * Math2.RAD2DEG); }
					else { return ftos(Math2.Cos(calculated)); }
				case "tan(":
					if (args.Length != 1) { return ERRMSG; }
					if (trigType == 'd') { return ftos(Math2.Tan(calculated * Math2.DEG2RAD) * Math2.RAD2DEG); }
					else { return ftos(Math2.Tan(calculated)); }
				case "asin(":
					if (args.Length != 1) { return ERRMSG; }
					if (trigType == 'd') { return ftos(Math2.Asin(calculated * Math2.DEG2RAD) * Math2.RAD2DEG); }
					else { return ftos(Math2.Asin(calculated)); }
				case "acos(":
					if (args.Length != 1) { return ERRMSG; }
					if (trigType == 'd') { return ftos(Math2.Acos(calculated * Math2.DEG2RAD) * Math2.RAD2DEG); }
					else { return ftos(Math2.Acos(calculated)); }
				case "atan(":
					if (args.Length != 1) { return ERRMSG; }
					if (trigType == 'd') { return ftos(Math2.Atan(calculated * Math2.DEG2RAD) * Math2.RAD2DEG); }
					else { return ftos(Math2.Atan(calculated)); }
				case "sqrt(":
					if (args.Length != 1) { return ERRMSG; }
					if (calculated < 0f) { return ERRMSG; }
					return ftos(Math2.Sqrt(calculated));
				case "abs(":
					if (args.Length != 1) { return ERRMSG; }
					return ftos(Math2.Abs(calculated));
				case "ceiling(":
					if (args.Length != 1) { return ERRMSG; }
					return ftos(Math2.Ceiling(calculated));
				case "floor(":
					if (args.Length != 1) { return ERRMSG; }
					return ftos(Math2.Floor(calculated));
				case "min(":
					if (args.Length != 2) { return ERRMSG; }
					return ftos(Math2.Min(calculated, stof(Calculate(args[1]))));
				case "max(":
					if (args.Length != 2) { return ERRMSG; }
					return ftos(Math2.Max(calculated, stof(Calculate(args[1]))));
				case "lerp(":
					if (args.Length != 3) { return ERRMSG; }
					return ftos(Math2.Lerp(calculated, stof(Calculate(args[1])), stof(Calculate(args[2]))));
				case "inverselerp(":
					if (args.Length != 3) { return ERRMSG; }
					return ftos(Math2.InverseLerp(calculated, stof(Calculate(args[1])), stof(Calculate(args[2]))));
				case "clamp(":
					if (args.Length != 3) { return ERRMSG; }
					return ftos(Math2.Clamp(calculated, stof(Calculate(args[1])), stof(Calculate(args[2]))));
				case "log(":
					switch (args.Length) 
					{
						case 1:
							return ftos(Math2.Log(calculated));
						case 2:
							return ftos(Math2.Log(calculated, stof(Calculate(args[1]))));
						default:
							return content;
					}
			}

			return content;
		}

		private static string CheckForFunctions(string main) 
		{
			if (main == ERRMSG) { return ERRMSG; }
			if (!main.Contains("(")) { return main; }

			int i = 0;
			int t = 0;
			int count = 0;
			string content = "";
			while (i < functions.Length) 
			{
				while (main.Contains(functions[i])) 
				{
					count = 1;
					content = "";
					t = main.IndexOf(functions[i]) + functions[i].Length;
					while (t < main.Length) 
					{
						if (main[t] == '(') { count++; }
						if (main[t] == ')') { count--; }
						if (count == 0) { break; }
						content += main[t];
						t++;
					}

					content = (CountChar(content, '(') - CountChar(content, ')') != 0 || (!content.Contains("(") && content[content.Length - 1] == ')')) ? content.Remove(content.Length - 1, 1) : content;
					main = main.Replace(functions[i] + content + ")", CalculateFunction(content, functions[i]));
				}

				i++;
			}

			return main;
		}

		private static string FormatBrackets(string main) 
		{
			int i = 0;
			while (i < main.Length) 
			{
				if (main[i] == '(' && Array.IndexOf(allowedChars, main[(i == 0) ? i : i - 1]) >= 0) 
				{
					main = GetStringAt(main, 0, i - 1) + "&" + GetStringAt(main, i, main.Length - 1);
				}

				if (main[i] == '|') 
				{
					// how?
				}

				i++;
			}

			return main;
		}

		private static string ReplaceConstants(string main) 
		{
			if (main.Contains("π")) { main = main.Replace("π", ftos(Math2.PI)); }
			if (main.Contains("pi")) { main = main.Replace("pi", ftos(Math2.PI)); }

			int i = 0;
			while (i < main.Length) 
			{
				if (main[i] == 'e') 
				{
					if (Array.IndexOf(allowedChars, main[(i - 1 < 0) ? 1 : i - 1]) >= 0 && Array.IndexOf(allowedChars, main[(i + 1 >= main.Length) ? main.Length - 2 : i + 1]) >= 0) 
					{
						main = ((i - 1 < 0) ? "" : GetStringAt(main, 0, i - 1)) + ftos(Math2.E) + ((i + 1 >= main.Length) ? "" : GetStringAt(main, i + 1, main.Length - 1));
					}
				}

				i++;
			}

			return main;
		}

		private static string Format(string main) 
		{
			main = main.Replace(" ", "").ToLower();
			if (main == "" || main == " " || CountChar(main, ' ') == main.Length) { return ERRMSG; }
			if (main.Contains("&")) { return ERRMSG; }
			if (CountChar(main, '(') != CountChar(main, ')')) { return ERRMSG; }
			if (CountChar(main, '|') % 2 != 0) { return ERRMSG; }

			main = ReplaceConstants(main);
			main = FormatBrackets(main);
			main = CheckForFunctions(main);

			int i = 0;
			while (i < main.Length) 
			{
				if (Array.IndexOf(allowedChars, main[i]) < 0) { return ERRMSG; }
				i++;
			}

			return main;
		}

		public static string Solve(string input) 
		{
			string output = Calculate(Format(input));;
			return (output != ERRMSG) ? output : "";
		}
	}
}