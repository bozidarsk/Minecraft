using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft.UI 
{
	public class SlotGroup 
	{
		public int index { get; }
		public int length { get; }
		public string name { get; }

		public SlotGroup(int index, int length, string name) 
		{
			this.index = index;
			this.length = length;
			this.name = name;
		}
	}
}