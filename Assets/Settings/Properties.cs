using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	[Serializable]
	public struct PropertyArray<T> 
	{ public T[] properties; }

	[Serializable]
	public struct CraftingProperty 
	{
		public Item resultItem;
		public Item[,] recipe;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public struct VoxelProperty 
	{
		public string id;
		public string dropItem;
		public string preferedTool;
		public byte light;
		public Cull cull;
		public int dropMultiplier;
		public int miningForce;
		public float miningSpeed;
		public float speedMultiplier;
		public float xp;
		public bool useCollision;
		public bool indestructible;
		public Vector3bool enableRotation;
		public bool[] usingFullFace;
		public Vector2byte[] textureCoords;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public struct ToolProperty 
	{
		public float damage;
		public float durability;
		public float miningSpeed;
		public float miningForce;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public struct ArmourProperty 
	{
		public float protection;
		public float blastProtection;
		public float projectileProtection;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public struct ItemProperty 
	{
		public string id;
		public string description;
		public Vector2 descriptionSize;
		public Vector2byte textureCoords;
		public uint stackSize;
		public string[] allowedEnchantments;
		public Enchantment[] enchantments;
		public ToolProperty toolProperty;
		public ArmourProperty armourProperty;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}

	[Serializable]
	public struct EnchantmentProperty 
	{
		public string id;
		public string shader;
		public uint maxLevel;
		public string[] incompatibleEnchantments;

		public override string ToString() { return JsonUtility.ToJson(this); }
	}
}