using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Utils.Collections;

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
		public Vector3<bool> enableRotation;
		public bool[] usingFullFace;
		public Vector2<byte>[] textureCoords;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	public class ToolProperty 
	{
		public float damage;
		public uint durability;
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

	public class EnchantmentProperty 
	{
		public string id;
		public string shader;
		public uint maxLevel;
		public string[] incompatibleEnchantments;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}
}