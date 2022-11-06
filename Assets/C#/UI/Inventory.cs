using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace Minecraft.UI 
{
	public interface IInventory 
	{
		void Toggle();
		void InitializeSlots();
	}

	public abstract class Inventory : MonoBehaviour
	{
		public RectTransform gui;
		public GameObject cursor;
		public bool IsOpen { protected set; get; }

		[HideInInspector] public List<InventorySlot> slots;
		[HideInInspector] public int buttonIndex = 0;

		protected List<SlotGroup> groups;
		protected InventorySlot cursorSlot;
		protected RectTransform description;
		protected TextMeshProUGUI descriptionText;
		protected Player player;

		protected virtual void Start() 
		{
			IsOpen = false; 
			gui.gameObject.SetActive(false);
			player = gameObject.GetComponent<Player>();
			slots = new List<InventorySlot>();
			groups = new List<SlotGroup>();
			cursorSlot = new InventorySlot(cursor, this, null);
			description = cursorSlot.gameObject.GetComponentsInChildren<RectTransform>().Where(x => x.name == "Description").ToArray()[0];
			descriptionText = description.gameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().ToArray()[0];
		}

		protected virtual void Update() 
		{
			if (!IsOpen) { cursorSlot.gameObject.SetActive(false); description.gameObject.SetActive(false); return; }
			cursorSlot.gameObject.SetActive(true);

			if (!cursorSlot.IsEmpty && Input.GetKey(PlayerSettings.controlls.keyCodes.DropSelectedItem)) 
			{ DropFromSlot(ref cursorSlot); }

			Vector2 mousePosition = Input.mousePosition;
			Vector3[] corners = new Vector3[4];
			gui.GetWorldCorners(corners);

			description.gameObject.SetActive(
				(mousePosition.x > corners[0].x && mousePosition.y > corners[0].y) && 
				(mousePosition.x > corners[1].x && mousePosition.y < corners[1].y) && 
				(mousePosition.x < corners[2].x && mousePosition.y < corners[2].y) && 
				(mousePosition.x < corners[3].x && mousePosition.y > corners[3].y)
			);

			cursorSlot.gameObject.transform.position = mousePosition;
			gui.transform.localScale = Vector3.one * PlayerSettings.graphics.GUIScale;
		}

		protected void DropFromSlot(int index, bool all = false) 
		{
			if (slots[index].IsEmpty) { return; }

			if (Input.GetKey(KeyCode.LeftControl) || all) 
			{
				player.DropItem(slots[index].item);
				slots[index].item = Item.EmptyItem;
				slots[index].Update();
			}
			else 
			{
				player.DropItem(new Item(slots[index].item.id, 1));
				if (slots[index].item.ammount == 0) { slots[index].item.ammount++; }
				slots[index].item.ammount--;
				if (slots[index].item.ammount == 0) { slots[index].item = Item.EmptyItem; }
				slots[index].Update();
			}
		}

		protected void DropFromSlot(ref InventorySlot slot, bool all = false) 
		{
			if (slot.IsEmpty) { return; }

			if (Input.GetKey(KeyCode.LeftControl) || all) 
			{
				player.DropItem(slot.item);
				slot.item = Item.EmptyItem;
				slot.Update();
			}
			else 
			{
				player.DropItem(new Item(slot.item.id, 1));
				if (slot.item.ammount == 0) { slot.item.ammount++; }
				slot.item.ammount--;
				if (slot.item.ammount == 0) { slot.item = Item.EmptyItem; }
				slot.Update();
			}
		}

		public SlotGroup GetGroupByName(string name) 
		{
			try { return groups.Where(x => x.name == name).ToArray()[0]; }
			catch { return null; }
		}

		public void SlotLeftClicked(int index) 
		{
			if (slots[index].IsEmpty && cursorSlot.IsEmpty) { return; }

			if (slots[index].item.id != cursorSlot.item.id || slots[index].item.durability != cursorSlot.item.durability) 
			{
				Item tmp = new Item(slots[index].item);
				slots[index].item = new Item(cursorSlot.item);
				cursorSlot.item = tmp;
			}
			else 
			{
				uint stackSize = GameManager.GetItemPropertyById(cursorSlot.item.id).stackSize;
				if (slots[index].item.ammount + cursorSlot.item.ammount <= stackSize) 
				{
					slots[index].item.ammount += cursorSlot.item.ammount;
					cursorSlot.item = Item.EmptyItem;
				}
				else 
				{
					cursorSlot.item.ammount -= stackSize - slots[index].item.ammount;
					slots[index].item.ammount = stackSize;
				}
			}

			slots[index].Update();
			cursorSlot.Update();
		}

		public void SlotRightClicked(int index) 
		{
			if (slots[index].IsEmpty && cursorSlot.IsEmpty) { return; }

			if (cursorSlot.IsEmpty) 
			{
				if (slots[index].IsEmpty) { return; }
				cursorSlot.item = new Item(slots[index].item);
				slots[index].item.ammount /= 2;
				cursorSlot.item.ammount /= 2;
				cursorSlot.item.ammount += (cursorSlot.item.ammount % 2 != 0) ? (uint)1 : (uint)0;
			}
			else 
			{
				if (slots[index].IsEmpty) 
				{
					slots[index].item = new Item(cursorSlot.item);
					slots[index].item.ammount = 1;
					cursorSlot.item.ammount--;
				}
				else if (!slots[index].IsEmpty && (slots[index].item.id != cursorSlot.item.id || slots[index].item.durability != cursorSlot.item.durability)) 
				{
					Item tmp = new Item(slots[index].item);
					slots[index].item = new Item(cursorSlot.item);
					cursorSlot.item = tmp;
				}
				else 
				{
					uint stackSize = GameManager.GetItemPropertyById(cursorSlot.item.id).stackSize;
					if (slots[index].item.ammount >= stackSize) { return; }
					slots[index].item.ammount++;
					cursorSlot.item.ammount--;
				}
			}

			slots[index].Update();
			cursorSlot.Update();
		}

		public void SlotDoubleClicked(int index) 
		{
			throw new System.NotImplementedException("When double clicked the first click is registerd as a leftclick, then the second as doubleclick.");
		}

		public void SlotHighlighted(int index) 
		{
			slots[index].gameObject.GetComponent<UnityEngine.UI.RawImage>().color = Color.white;
		}

		public uint AddItem(Item item, string group) { return AddItem(item, GetGroupByName(group)); }
		public uint AddItem(Item item, SlotGroup group) 
		{
			if (item.IsEmpty) { return 0; }
			uint stackSize = GameManager.GetItemPropertyById(item.id).stackSize;

			for (int i = group.index; i < group.length; i++) 
			{
				if (slots[i].item.id == item.id && slots[i].item.durability == item.durability && slots[i].IsInFilter(item.id)) 
				{
					if (item.ammount + slots[i].item.ammount <= stackSize) 
					{
						slots[i].item = new Item(item);
						slots[i].Update();
						return 0;
					}

					item.ammount = (item.ammount + slots[i].item.ammount) - stackSize;
					slots[i].item.ammount = stackSize;
					slots[i].Update();
					// return AddItem(item, group);
					continue;
				}

				if (slots[i].IsEmpty && slots[i].IsInFilter(item.id)) 
				{
					slots[i].item = new Item(item);

					if (item.ammount <= stackSize) 
					{
						slots[i].Update();
						return 0;
					}

					item.ammount -= stackSize;
					slots[i].item.ammount = stackSize;
					slots[i].Update();
					// return AddItem(item, group);
					continue;
				}
			}

			return item.ammount;
		}

		public uint RemoveItem(Item item, string group) { return RemoveItem(item, GetGroupByName(group)); }
		public uint RemoveItem(Item item, SlotGroup group) 
		{
			if (item.IsEmpty) { return 0; }
			uint stackSize = GameManager.GetItemPropertyById(item.id).stackSize;

			for (int i = group.index; i < group.length; i++) 
			{
				if (slots[i].IsEmpty || slots[i].item.id != item.id || slots[i].item.durability != item.durability) { continue; }
				if (slots[i].item.ammount >= item.ammount) { slots[i].item.ammount -= item.ammount; slots[i].Update(); return 0; }

				item.ammount -= slots[i].item.ammount;
				slots[i].item = Item.EmptyItem;
				slots[i].Update();
				// return RemoveItem(item, group);
				continue;
			}

			return item.ammount;
		}

		public void DropAll() 
		{
			for (int i = 0; i < slots.Count; i++) 
			{
				DropFromSlot(i, true);
			}
		}

		public void Clear() 
		{
			for (int i = 0; i < slots.Count; i++) 
			{
				slots[i].item = Item.EmptyItem;
				slots[i].Update();
			}
		}
	}
}