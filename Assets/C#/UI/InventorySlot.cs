using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Minecraft.UI 
{
	public class InventorySlot 
	{
		public Item item { set; get; }
		public GameObject gameObject { get; }
		public bool IsEmpty { get { return item.IsEmpty; } }

		private Inventory inventory;
		private List<string> filters;
		private TextMeshProUGUI text;
		public RawImage icon;
		private RawImage durabilitySlider;

		public void Update() 
		{
			if (item.durability < 0) { item = Item.EmptyItem; }

			if (item.ammount == 0) { item.id = "air-block"; }
			text.SetText((!IsEmpty && item.ammount > 1) ? item.ammount.ToString() : "");

			Texture2D texture = new Texture2D(1, 1);
			try { ImageConversion.LoadImage(texture, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.itemTextures + "/" + item.id + ".png")), false); }
			catch { ImageConversion.LoadImage(texture, File.ReadAllBytes(GameManager.FormatPath(GameSettings.path.itemTextures + "/undefined-" + (item.id.EndsWith("-block") ? "block" : "item") + ".png")), false); }
			icon.texture = texture;

			ItemProperty property = GameManager.GetItemPropertyById(item.id);

			float percent = -1f;
			if (property != null && property.toolProperty != null) 
			{
				percent = (float)item.durability / (float)property.toolProperty.maxDurability;
				percent = ((uint)item.durability == property.toolProperty.maxDurability) ? -1f : percent;
			}

			durabilitySlider.material.SetColor("fgColor", GameSettings.player.durabilityGradient.Evaluate(percent));
			durabilitySlider.material.SetColor("bgColor", Color.black);
			durabilitySlider.material.SetFloat("value", percent);
		}

		public void ClearFilters() { filters = null; }
		public bool IsInFilter(string id) 
		{
			if (filters == null) { return true; }
			for (int i = 0; i < filters.Count; i++) { if (filters[i].Contains(id)) { return true; } }
			return false;
		}

		public void AddFilters(params string[] id) 
		{
			if (filters == null) { filters = new List<string>(); }
			for (int i = 0; i < id.Length; i++) { filters.Add(id[i]); }
		}

		public InventorySlot(GameObject obj, Inventory inventory, params string[] filters) 
		{
			this.inventory = inventory;
			this.item = Item.EmptyItem;
			this.filters = (filters == null) ? null : filters.ToList();
			this.text = obj.GetComponentsInChildren<TextMeshProUGUI>()[0];
			this.icon = obj.GetComponentsInChildren<RawImage>()[1];
			this.durabilitySlider = obj.GetComponentsInChildren<RawImage>()[2];
			this.gameObject = obj;

			if (obj.TryGetComponent(typeof(SlotController), out Component controller)) 
			{
				((SlotController)controller).index = this.inventory.buttonIndex++;
				((SlotController)controller).onLeftClick += this.inventory.SlotLeftClicked;
				((SlotController)controller).onRightClick += this.inventory.SlotRightClicked;
				((SlotController)controller).onDoubleClick += this.inventory.SlotDoubleClicked;
				((SlotController)controller).onHighlight += this.inventory.SlotHighlighted;
			}

			this.durabilitySlider.material = GameSettings.materials.slider;
			this.Update();
		}
	}
}