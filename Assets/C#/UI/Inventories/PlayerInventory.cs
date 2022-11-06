using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace Minecraft.UI 
{
	public class PlayerInventory : Inventory, IInventory
	{
		public InventorySlot HandSlot { get { return hotbarSlots[handIndex]; } }
		public Item HandItem 
		{
			set { hotbarSlots[handIndex].item = value; }
			get { return hotbarSlots[handIndex].item; }
		}

		[SerializeField] private GameObject hotbar;
		[SerializeField] private GameObject hotbarSelector;

		private InventorySlot[] hotbarSlots;
		private InventorySlot offhandSlot;
		private int handIndex;

		protected override void Start() { base.Start(); InitializeSlots(); }
		protected override void Update() 
		{
			base.Update();

			int i = 0;
			SlotGroup hotbarGroup = GetGroupByName("hotbar");
			for (int index = hotbarGroup.index; index < hotbarGroup.length + hotbarGroup.index; index++) 
			{
				hotbarSlots[i++].item = new Item(slots[index].item);
				hotbarSlots[i - 1].Update();
			}

			if (IsOpen || player.chat.IsOpen) { return; }

			if (Input.mouseScrollDelta.y * 0.05f < 0) { handIndex--; }
			if (Input.mouseScrollDelta.y * 0.05f > 0) { handIndex++; }
			handIndex = System.Math.Clamp(handIndex, 0, 8);

			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot1)) { handIndex = 0; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot2)) { handIndex = 1; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot3)) { handIndex = 2; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot4)) { handIndex = 3; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot5)) { handIndex = 4; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot6)) { handIndex = 5; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot7)) { handIndex = 6; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot8)) { handIndex = 7; }
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.HotbarSlot9)) { handIndex = 8; }
			hotbarSelector.transform.localPosition = new Vector3(handIndex * 14f - 64f, 0f, 0f);

			SlotGroup offhandGroup = GetGroupByName("offhand");
			if (Input.GetKeyDown(PlayerSettings.controlls.keyCodes.SwapItemWithOffhand)) 
			{
				Item tmp = new Item(slots[offhandGroup.index].item);
				slots[offhandGroup.index].item = new Item(slots[hotbarGroup.index + handIndex].item);
				slots[offhandGroup.index].Update();
				slots[hotbarGroup.index + handIndex].item = tmp;
				slots[hotbarGroup.index + handIndex].Update();
			}

			offhandSlot.item = new Item(slots[offhandGroup.index].item);
			offhandSlot.gameObject.SetActive(!offhandSlot.IsEmpty);
			offhandSlot.Update();
		}

		public void InitializeSlots() 
		{
			RectTransform[] transforms = gui.GetComponentsInChildren<RectTransform>();

			groups.Add(new SlotGroup(buttonIndex, 1, "crafting-result"));
			slots.Add(new InventorySlot(transforms.Where(x => x.name == "CraftingResultSlot").ToArray()[0].gameObject, this, "-"));

			groups.Add(new SlotGroup(buttonIndex, 1, "offhand"));
			slots.Add(new InventorySlot(transforms.Where(x => x.name == "OffhandSlot").ToArray()[0].gameObject, this, null));

			groups.Add(new SlotGroup(buttonIndex, 27, "inventory"));
			slots.AddRange(
				transforms
				.Where(x => x.name == "InventorySlots").ToArray()[0].gameObject
				.GetComponentsInChildren<RectTransform>()
				.Where(x => x.name.StartsWith("SlotButton")).ToArray()
				.Select(x => new InventorySlot(x.gameObject, this, null)).ToArray()
			);

			groups.Add(new SlotGroup(buttonIndex, 9, "hotbar"));
			slots.AddRange(
				transforms
				.Where(x => x.name == "HotbarSlots").ToArray()[0].gameObject
				.GetComponentsInChildren<RectTransform>()
				.Where(x => x.name.StartsWith("SlotButton")).ToArray()
				.Select(x => new InventorySlot(x.gameObject, this, null)).ToArray()
			);

			groups.Add(new SlotGroup(buttonIndex, 4, "crafting"));
			slots.AddRange(
				transforms
				.Where(x => x.name == "CraftingSlots").ToArray()[0].gameObject
				.GetComponentsInChildren<RectTransform>()
				.Where(x => x.name.StartsWith("SlotButton")).ToArray()
				.Select(x => new InventorySlot(x.gameObject, this, null)).ToArray()
			);

			groups.Add(new SlotGroup(buttonIndex, 4, "armour"));
			slots.AddRange(
				transforms
				.Where(x => x.name == "ArmourSlots").ToArray()[0].gameObject
				.GetComponentsInChildren<RectTransform>()
				.Where(x => x.name.StartsWith("SlotButton")).ToArray()
				.Select(x => new InventorySlot(x.gameObject, this, null)).ToArray()
			);

			transforms = hotbar.GetComponentsInChildren<RectTransform>();
			offhandSlot = new InventorySlot(transforms.Where(x => x.name == "Offhand").ToArray()[0].gameObject, this, null);
			hotbarSlots = 
				transforms
				.Where(x => x.name == "Slots").ToArray()[0].gameObject
				.GetComponentsInChildren<RectTransform>()
				.Where(x => x.name.StartsWith("HotbarSlot")).ToArray()
				.Select(x => new InventorySlot(x.gameObject, this, null)).ToArray()
			;
		}

		public void Toggle() 
		{
			IsOpen = !IsOpen;
			gui.gameObject.SetActive(IsOpen);
			if (IsOpen) { return; }

			SlotGroup group = GetGroupByName("crafting");
			for (int i = group.index; i < group.length + group.index; i++) { DropFromSlot(i); }
			DropFromSlot(GetGroupByName("crafting-result").index);
		}

		public uint AddItemAll(Item item) 
		{
			item.ammount = AddItem(item, "hotbar");
			if (item.ammount == 0) { return 0; }
			return AddItem(item, "inventory");
		}

		public uint RemoveItemAll(Item item) 
		{
			item.ammount = RemoveItem(item, "hotbar");
			if (item.ammount == 0) { return 0; }
			return RemoveItem(item, "inventory");
		}
	}
}