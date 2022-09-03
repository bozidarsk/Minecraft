using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Minecraft 
{
	public class PlayerInventory : Inventory, IInventory
	{
		public GameObject hotbarSelector;
		public GameObject offhand;
		[HideInInspector] public int buttonIndex = 0;
		public bool IsOpen { private set; get; }

		private InventorySlot[] hotbarSlots;
		private InventorySlot offhandSlot;
		private int handOnHotbar;

		public void InitializeSlots() 
		{
			RectTransform[] transforms = hotbarSelector.transform.parent.gameObject.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("Hotbar Slot")).ToArray();
			for (int i = 0; i < transforms.Length; i++) { hotbarSlots[i] = new InventorySlot(transforms[i].gameObject, null, null, this, null); }

			GameObject inventoryButtons, hotbarButtons, craftingButtons, armourButtons;
			transforms = gui.GetComponentsInChildren<RectTransform>();
			offhandSlot = new InventorySlot(offhand, null, null, this, null);

			inventoryButtons = transforms.Where(x => x.name == "InventorySlots").ToArray()[0].gameObject;
			hotbarButtons = transforms.Where(x => x.name == "HotbarSlots").ToArray()[0].gameObject;
			craftingButtons = transforms.Where(x => x.name == "CraftingSlots").ToArray()[0].gameObject;
			armourButtons = transforms.Where(x => x.name == "ArmourSlots").ToArray()[0].gameObject;

			cursorSlot = new InventorySlot(transforms.Where(x => x.name == "CursorSlot").ToArray()[0].gameObject, "Cursor", null, this, null);
			cursorSlot.item = new Item("air-block", 0);

			slotStartIndex.Add("Offhand", buttonIndex);
			slots.Add(new InventorySlot(transforms.Where(x => x.name == "OffhandSlot").ToArray()[0].gameObject, "Offhand", "Inventory", this, null));

			slotStartIndex.Add("CraftingResult", buttonIndex);
			slots.Add(new InventorySlot(transforms.Where(x => x.name == "CraftingResultSlot").ToArray()[0].gameObject, "CraftingResult", "Inventory", this, "notplaceable"));

			slotStartIndex.Add("Inventory", buttonIndex);
			transforms = inventoryButtons.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("Button")).ToArray();
			for (int i = 0; i < 9 * 3; i++) { slots.Add(new InventorySlot(transforms[i].gameObject, "Inventory", "Hotbar", this, null)); }

			slotStartIndex.Add("Hotbar", buttonIndex);
			transforms = hotbarButtons.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("Button")).ToArray();
			for (int i = 0; i < 9; i++) { slots.Add(new InventorySlot(transforms[i].gameObject, "Hotbar", "Inventory", this, null)); }

			slotStartIndex.Add("Crafting", buttonIndex);
			transforms = craftingButtons.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("Button")).ToArray();
			for (int i = 0; i < 4; i++) { slots.Add(new InventorySlot(transforms[i].gameObject, "Crafting", "Inventory", this, null)); }

			slotStartIndex.Add("Armour", buttonIndex);
			string[] filters = { "helmet", "chestplate", "leggings", "boots" };
			transforms = armourButtons.GetComponentsInChildren<RectTransform>().Where(x => x.name.Contains("Button")).ToArray();
			for (int i = 0; i < 4; i++) { slots.Add(new InventorySlot(transforms[i].gameObject, "Armour", "Inventory", this, filters[i])); }
		}

		void Start() 
		{
			slotStartIndex = new Dictionary<string, int>(6);
			player = gameObject.GetComponent<Player>();
			slots = new List<InventorySlot>(46);
			hotbarSlots = new InventorySlot[9];

			InitializeSlots();
			gui.SetActive(IsOpen);

			description = cursorSlot.gameObject.GetComponentsInChildren<RectTransform>().Where(x => x.name == "Description").ToArray()[0];
			descriptionText = description.gameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>().ToArray()[0];
		}

		void Update() 
		{
			if (Input.mouseScrollDelta.y * 0.05f < 0) { handOnHotbar--; }
			if (Input.mouseScrollDelta.y * 0.05f > 0) { handOnHotbar++; }
			if (handOnHotbar < 0) { handOnHotbar = 8; }
			if (handOnHotbar > 8) { handOnHotbar = 0; }

			for (int i = 0; i < hotbarSlots.Length; i++) 
			{ 
				hotbarSlots[i].item = slots[slotStartIndex["Hotbar"] + i].item; 
				hotbarSlots[i].Update();
			}

			offhandSlot.item = slots[slotStartIndex["Offhand"]].item;
			if (!offhandSlot.item.IsEmpty) { offhandSlot.gameObject.SetActive(true); offhandSlot.Update(); }
			else { offhandSlot.gameObject.SetActive(false); }

			if (IsOpen) 
			{
				Vector2 mousePosition = Input.mousePosition;
				Vector3[] corners = new Vector3[4];
				description.GetWorldCorners(corners);

				description.gameObject.SetActive(
					(mousePosition.x > corners[0].x && mousePosition.y > corners[0].y) && 
					(mousePosition.x > corners[1].x && mousePosition.y < corners[1].y) && 
					(mousePosition.x < corners[2].x && mousePosition.y < corners[2].y) && 
					(mousePosition.x < corners[3].x && mousePosition.y > corners[3].y)
				);
			}

			if (!IsOpen && !player.chat.IsOpen) 
			{
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot1)) { handOnHotbar = 0; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot2)) { handOnHotbar = 1; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot3)) { handOnHotbar = 2; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot4)) { handOnHotbar = 3; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot5)) { handOnHotbar = 4; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot6)) { handOnHotbar = 5; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot7)) { handOnHotbar = 6; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot8)) { handOnHotbar = 7; }
				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot9)) { handOnHotbar = 8; }

				if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.SwapItemWithOffhand)) 
				{
					Item tmp = offhandSlot.item;
					offhandSlot.item = slots[slotStartIndex["Hotbar"] + handOnHotbar].item;
					slots[slotStartIndex["Hotbar"] + handOnHotbar].item = tmp;
					slots[slotStartIndex["Hotbar"] + handOnHotbar].Update();
					slots[slotStartIndex["Offhand"]].item = offhandSlot.item;
					slots[slotStartIndex["Offhand"]].Update();
				}

				if (Input.GetKey(PlayerSettings.controlls.keyCodes.DropSelectedItem)) 
				{
					InventorySlot slot = slots[slotStartIndex["Hotbar"] + handOnHotbar];
					DropFromSlot(in slot);
					slots[slotStartIndex["Hotbar"] + handOnHotbar] = slot;
				}

				hotbarSelector.transform.localPosition = new Vector3(handOnHotbar * 14f - 64f, 0f, 0f);
				hotbarSelector.transform.parent.localScale = Vector3.one * (PlayerSettings.graphics.GUIScale + 1);
				return;
			}

			if (Input.GetKey(PlayerSettings.controlls.keyCodes.DropSelectedItem)) { DropFromSlot(in cursorSlot); }
			cursorSlot.gameObject.transform.position = Input.mousePosition;

			gui.transform.localScale = Vector3.one * PlayerSettings.graphics.GUIScale;
		}

		public InventorySlot GetHandSlot() { return slots[slotStartIndex["Hotbar"] + handOnHotbar]; }

		public void Toggle() 
		{
			IsOpen = !IsOpen;
			gui.SetActive(IsOpen);
			if (!IsOpen) { return; }

			player.DropItem(slots[slotStartIndex["CraftingResult"]].item);
			slots[slotStartIndex["CraftingResult"]].item = new Item("air-block", 0);
			slots[slotStartIndex["CraftingResult"]].Update();

			player.DropItem(cursorSlot.item);
			cursorSlot.item = new Item("air-block", 0);
			cursorSlot.Update();

			for (int i = slotStartIndex["Crafting"]; i < slotStartIndex["Crafting"] + 4; i++) 
			{
				player.DropItem(slots[i].item);
				slots[i].item = new Item("air-block", 0);
				slots[i].Update();
			}
		}
	}
}