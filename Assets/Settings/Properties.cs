using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils.Collections.Generic;

namespace Minecraft 
{
	public class CraftingProperty 
	{
		public Item resultItem;
		public Item[,] recipe;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	public class VoxelProperty 
	{
		public string id = "undefined-block";
		public string dropItem = "undefined-block";
		public string preferedTool = null;
		public byte light = 0;
		public Cull cull = Cull.Back;
		public int dropMultiplier = 1;
		public float hardness = 1;
		public float movementSpeedMultiplier = 1;
		public float xp = 0;
		public Vector3 offset = Vector3.zero;
		public Vector3 scale = Vector3.one;
		public bool useCollision = true;
		public bool indestructible = false;
		public Vector3<bool> enableRotation = new Vector3<bool>(false, false, false);
		public bool[] usingFullFace = { true, true, true, true, true, true };
		public Vector2<byte>[] textureCoords = 
		{
			new Vector2<byte>(14, 1),
			new Vector2<byte>(14, 1),
			new Vector2<byte>(14, 1),
			new Vector2<byte>(14, 1),
			new Vector2<byte>(14, 1),
			new Vector2<byte>(14, 1)
		};

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	public class ToolProperty 
	{
		public float damage;
		public uint maxDurability;
		public float miningSpeed;
		public float miningForce;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	public class ArmourProperty 
	{
		public float protection;
		public float blastProtection;
		public float projectileProtection;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	public class ItemProperty 
	{
		public string id = "undefined-item";
		public string description = "This is an undefined item.";
		public uint stackSize = 64;
		public string[] allowedEnchantments = {};
		public ToolProperty toolProperty = null;
		public ArmourProperty armourProperty = null;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}
}