using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	[Serializable]
	public class CraftingProperty 
	{
		public Item resultItem;
		public Item[,] recipe;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public class VoxelProperty 
	{
		public string id;
		public string dropItem;
		public string preferedTool;
		public byte light;
		public Cull cull;
		public int dropMultiplier;
		public float hardness;
		public float movementSpeedMultiplier;
		public float xp;
		public Vector3 offset;
		public Vector3 scale;
		public bool useCollision;
		public bool indestructible;
		public Vector3bool enableRotation;
		public bool[] usingFullFace;
		public Vector2byte[] textureCoords;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public class ToolProperty 
	{
		public float damage;
		public uint durability;
		public float miningSpeed;
		public float miningForce;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public class ArmourProperty 
	{
		public float protection;
		public float blastProtection;
		public float projectileProtection;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public class ItemProperty 
	{
		public string id;
		public string description;
		public Vector2 descriptionSize;
		public uint stackSize;
		public string[] allowedEnchantments;
		public Enchantment[] enchantments;
		public ToolProperty toolProperty;
		public ArmourProperty armourProperty;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public class EnchantmentProperty 
	{
		public string id;
		public string shader;
		public uint maxLevel;
		public string[] incompatibleEnchantments;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}
}