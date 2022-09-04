using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Minecraft 
{
	public interface IInventory 
	{
		void Toggle();
		bool IsOpen { get; }
		void InitializeSlots();
		void SlotLeftClicked(int index);
		void SlotRightClicked(int index);
		void SlotDoubleClicked(int index);
		void SlotHighlighted(int index);
	}

	public class Inventory : MonoBehaviour
	{
		public GameObject gui;
		public List<InventorySlot> slots;
		protected Dictionary<string, int> slotStartIndex;
		protected InventorySlot cursorSlot;
		protected RectTransform description;
		protected TextMeshProUGUI descriptionText;
		protected Player player;

		protected void DropFromSlot(in InventorySlot slot) 
		{
			if (slot.item.IsEmpty) { return; }

			if (Input.GetKey(KeyCode.LeftControl)) 
			{
				player.DropItem(slot.item);
				slot.item = new Item("air-block", 0);
				slot.Update();
			}
			else 
			{
				player.DropItem(new Item(slot.item.id, 1));
				if (slot.item.ammount == 0) { slot.item.ammount++; }
				slot.item.ammount--;
				if (slot.item.ammount == 0) { slot.item = new Item("air-block", 0); }
				slot.Update();
			}
		}

		public void SlotLeftClicked(int index) 
		{
			if (Input.GetKey(KeyCode.LeftShift)) 
			{
				if (slots[index].item.IsEmpty) { return; }

				uint ammount = 0;
				switch (slots[index].opositeSlot) 
				{
					case "Inventory":
						ammount = TryAddItemInventory(slots[index].item);
						break;
					case "Hotbar":
						ammount = TryAddItemHotbar(slots[index].item);
						break;
				}

				if (ammount == 0) { slots[index].item = new Item("air-block", 0); }
				else { slots[index].item.ammount = ammount; }
				slots[index].Update();
				return;
			}

			if (slots[index].item.IsEmpty && cursorSlot.item.IsEmpty) 
			{ return; }

			if (slots[index].item.id == cursorSlot.item.id) 
			{
				uint stackSize = GameManager.itemProperties[GameManager.GetItemTypeById(slots[index].item.id)].stackSize;
				if (slots[index].item.ammount + cursorSlot.item.ammount <= stackSize) 
				{
					slots[index].item.ammount += cursorSlot.item.ammount;
					cursorSlot.item = new Item("air-block", 0);
				}
				else 
				{
					cursorSlot.item.ammount -= stackSize - slots[index].item.ammount;
					slots[index].item.ammount += stackSize - slots[index].item.ammount;
				}

				slots[index].Update();
				cursorSlot.Update();
				return;
			}

			if (slots[index].item.IsEmpty && !cursorSlot.item.IsEmpty) 
			{
				if (slots[index].IsInFilter(cursorSlot.item.id)) 
				{
					slots[index].item = cursorSlot.item;
					cursorSlot.item = new Item("air-block", 0);

					slots[index].Update();
					cursorSlot.Update();
					return;
				}
			}

			if (!slots[index].item.IsEmpty && cursorSlot.item.IsEmpty) 
			{
				if (slots[index].IsInFilter(cursorSlot.item.id)) 
				{
					cursorSlot.item = slots[index].item;
					slots[index].item = new Item("air-block", 0);

					slots[index].Update();
					cursorSlot.Update();
					return;
				}
			}

			if (!slots[index].item.IsEmpty && !cursorSlot.item.IsEmpty) 
			{
				if (slots[index].IsInFilter(cursorSlot.item.id)) 
				{
					Item tmp = cursorSlot.item;
					cursorSlot.item = slots[index].item;
					slots[index].item = tmp;

					slots[index].Update();
					cursorSlot.Update();
					return;
				}
			}
		}

		public void SlotRightClicked(int index) 
		{
			if (cursorSlot.item.IsEmpty) 
			{
				cursorSlot.item = new Item(slots[index].item.id, slots[index].item.ammount / 2);
				if (slots[index].item.ammount % 2 != 0) { cursorSlot.item.ammount++; }

				slots[index].item.ammount -= cursorSlot.item.ammount;
				if (slots[index].item.ammount == 0) { slots[index].item.id = "air-block"; }
			}
			else 
			{
				if ((slots[index].item.id != cursorSlot.item.id && !slots[index].item.IsEmpty) || 
					slots[index].item.ammount >= GameManager.itemProperties[GameManager.GetItemTypeById(cursorSlot.item.id)].stackSize || 
					!slots[index].IsInFilter(cursorSlot.item.id)
				) { return; }

				slots[index].item.id = cursorSlot.item.id;
				slots[index].item.ammount++;
				cursorSlot.item.ammount--;
				if (cursorSlot.item.ammount == 0) { cursorSlot.item.id = "air-block"; }
			}

			slots[index].Update();
			cursorSlot.Update();
		}

		[System.Obsolete("When double clicked the first click is registerd as a leftclick, then the second as doubleclick.")]
		public void SlotDoubleClicked(int index) 
		{
			if (slots[index].item.IsEmpty || 
				(slots[index].item.id != cursorSlot.item.id && !cursorSlot.item.IsEmpty)
			) { return; }

			uint stackSize = GameManager.itemProperties[GameManager.GetItemTypeById(slots[index].item.id)].stackSize;
			uint leftover = TryRemoveItem(new Item(slots[index].item.id, stackSize - cursorSlot.item.ammount));
			cursorSlot.item = new Item(slots[index].item.id, stackSize - cursorSlot.item.ammount - leftover);

			UpdateAllUI();
			cursorSlot.Update();
		}

		public void SlotHighlighted(int index) 
		{
			if (!slots[index].item.IsEmpty) 
			{
				description.gameObject.SetActive(true);
				ItemProperty property = GameManager.itemProperties[GameManager.GetItemTypeById(slots[index].item.id)];
				description.offsetMax = new Vector2(property.descriptionSize.x, description.offsetMax.y);
				description.offsetMin = new Vector2(description.offsetMin.x, property.descriptionSize.y);
				descriptionText.text = property.description + "\n"/*"\n<color=\"#575757\">"*/ + slots[index].item.id/* + "</color>"*/;
			}
			else { description.gameObject.SetActive(false); }

			if (Input.GetKey(PlayerSettings.controlls.keyCodes.DropSelectedItem)) 
			{
				InventorySlot slot = slots[index];
				DropFromSlot(in slot);
				slots[index] = slot;
			}
		}

		public uint TryAddItemInventory(Item item) 
		{
			uint stackSize = GameManager.itemProperties[GameManager.GetItemTypeById(item.id)].stackSize;

			for (int i = slotStartIndex["Inventory"]; i < slotStartIndex["Inventory"] + (9*3); i++) 
			{
				if (slots[i].item.id == item.id && slots[i].item.ammount < stackSize && slots[i].IsInFilter(item.id)) 
				{
					if (slots[i].item.ammount + item.ammount <= stackSize) 
					{ slots[i].item.ammount += item.ammount; slots[i].Update(); return 0; }

					item.ammount -= stackSize - slots[i].item.ammount;
					slots[i].item.ammount = stackSize;
					slots[i].Update();
					return TryAddItemInventory(new Item(item.id, item.ammount));
				}

				if (slots[i].item.IsEmpty && slots[i].IsInFilter(item.id)) 
				{
					slots[i].item.id = item.id;

					if (item.ammount <= stackSize) 
					{ slots[i].item.ammount = item.ammount; slots[i].Update(); return 0; }

					return TryAddItemInventory(new Item(item.id, item.ammount - stackSize));
				}
			}

			return item.ammount;
		}

		public uint TryAddItem(Item item) 
		{
			uint leftover = 0;

			leftover += TryAddItemHotbar(item);
			if (leftover == 0) { return leftover; }

			item.ammount = leftover;
			leftover += TryAddItemInventory(item);
			if (leftover == 0) { return leftover; }

			return leftover;
		}

		public uint TryAddItemHotbar(Item item) 
		{
			uint stackSize = GameManager.itemProperties[GameManager.GetItemTypeById(item.id)].stackSize;

			for (int i = slotStartIndex["Hotbar"]; i < slotStartIndex["Hotbar"] + 9; i++) 
			{
				if (slots[i].item.id == item.id && slots[i].item.ammount < stackSize && slots[i].IsInFilter(item.id)) 
				{
					if (slots[i].item.ammount + item.ammount <= stackSize) 
					{ slots[i].item.ammount += item.ammount; slots[i].Update(); return 0; }

					item.ammount -= stackSize - slots[i].item.ammount;
					slots[i].item.ammount = stackSize;
					slots[i].Update();
					return TryAddItemHotbar(new Item(item.id, item.ammount));
				}

				if (slots[i].item.IsEmpty && slots[i].IsInFilter(item.id)) 
				{
					slots[i].item.id = item.id;

					if (item.ammount <= stackSize) 
					{ slots[i].item.ammount = item.ammount; slots[i].Update(); return 0; }

					return TryAddItemHotbar(new Item(item.id, item.ammount - stackSize));
				}
			}

			return item.ammount;
		}

		public uint TryRemoveItem(Item item) 
		{
			for (int i = slotStartIndex["Inventory"]; i < slotStartIndex["Inventory"] + (9*3); i++) 
			{
				if (slots[i].item.id == item.id) 
				{
					if (slots[i].item.ammount > item.ammount) { slots[i].item.ammount -= item.ammount; slots[i].Update(); return 0; }
					else 
					{
						slots[i].item.id = "air-block";
						if (slots[i].item.ammount == item.ammount) { slots[i].item.ammount = 0; slots[i].Update(); return 0; }
						return TryRemoveItem(new Item(item.id, item.ammount - slots[i].item.ammount));
					}
				}
			}

			for (int i = slotStartIndex["Hotbar"]; i < slotStartIndex["Hotbar"] + 9; i++) 
			{
				if (slots[i].item.id == item.id) 
				{
					if (slots[i].item.ammount > item.ammount) { slots[i].item.ammount -= item.ammount; slots[i].Update(); return 0; }
					else 
					{
						slots[i].item.id = "air-block";
						if (slots[i].item.ammount == item.ammount) { slots[i].item.ammount = 0; slots[i].Update(); return 0; }
						return TryRemoveItem(new Item(item.id, item.ammount - slots[i].item.ammount));
					}
				}
			}

			return item.ammount;
		}

		public void Clear() 
		{
			for (int i = 0; i < slots.Count; i++) { slots[i].item = new Item("air-block", 1); slots[i].Update(); }
			cursorSlot.item = new Item("air-block", 1);
			cursorSlot.Update();
		}

		public int IndexOfItem(Item item) 
		{
			for (int i = slotStartIndex["Inventory"]; i < slotStartIndex["Inventory"] + (9*3); i++) 
			{ if (slots[i].item.id == item.id && slots[i].item.ammount >= item.ammount) { return i; } }

			for (int i = slotStartIndex["Hotbar"]; i < slotStartIndex["Hotbar"] + 9; i++) 
			{ if (slots[i].item.id == item.id && slots[i].item.ammount >= item.ammount) { return i; } }

			return -1;
		}

		public bool ContainsItem(Item item) 
		{
			uint ammount = 0;

			for (int i = slotStartIndex["Inventory"]; i < slotStartIndex["Inventory"] + (9*3); i++) 
			{ if (slots[i].item.id == item.id) { ammount += slots[i].item.ammount; } }

			for (int i = slotStartIndex["Hotbar"]; i < slotStartIndex["Hotbar"] + 9; i++) 
			{ if (slots[i].item.id == item.id) { ammount += slots[i].item.ammount; } }

			return ammount >= item.ammount;
		}

		protected void UpdateAllUI() { for (int i = 0; i < slots.Count; i++) { slots[i].Update(); } }
	}
}