using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace Minecraft 
{
	public class SlotController : MonoBehaviour, IPointerClickHandler
	{
		[HideInInspector] public event System.Action<int> onLeftClick;
		[HideInInspector] public event System.Action<int> onRightClick;
		// [HideInInspector] public event System.Action<int> onDoubleClick;
		[HideInInspector] public event System.Action<int> onHighlight;
		[HideInInspector] public int index = -1;
		private bool isHighlighted = false;
		private RectTransform rt;

		void Awake() { rt = gameObject.GetComponent<RectTransform>(); }
		void Update() 
		{
			Vector2 mousePosition = Input.mousePosition;
			Vector3[] corners = new Vector3[4];
			rt.GetWorldCorners(corners);

			isHighlighted = 
			(mousePosition.x > corners[0].x && mousePosition.y > corners[0].y) && 
			(mousePosition.x > corners[1].x && mousePosition.y < corners[1].y) && 
			(mousePosition.x < corners[2].x && mousePosition.y < corners[2].y) && 
			(mousePosition.x < corners[3].x && mousePosition.y > corners[3].y);

			if (isHighlighted && onHighlight != null) { onHighlight(index); }
		}

		public void OnPointerClick(PointerEventData data) 
		{
			// if (data.clickCount == 2 && onDoubleClick != null) { onDoubleClick(index); return; }

			switch (data.button) 
			{
				case PointerEventData.InputButton.Left:
					if (onLeftClick != null) { onLeftClick(index); }
					break;
				case PointerEventData.InputButton.Right:
					if (onLeftClick != null) { onRightClick(index); }
					break;
			}
		}
	}
}