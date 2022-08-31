using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Minecraft 
{
	public class ChatController : MonoBehaviour
	{
		public TextMeshProUGUI chatBox;
		public TMP_InputField inputField;

		private RectTransform chatBoxTransform;
		private Player player;

		public bool IsOpen 
		{
			set { chatBox.gameObject.transform.parent.gameObject.SetActive(value); inputField.gameObject.SetActive(value); inputField.text = ""; }
			get { return inputField.gameObject.activeSelf; }
		}

		void Start() 
		{
			chatBoxTransform = chatBox.gameObject.GetComponent<RectTransform>();
			player = gameObject.GetComponent<Player>();
			IsOpen = false;
		}

		void Update() 
		{
			if (!IsOpen) { return; }

			chatBox.text = ChatManager.AllMessages;
			inputField.ActivateInputField();

			Vector2 mouse = Input.mousePosition;
			Vector3[] corners = new Vector3[4];
			chatBoxTransform.GetWorldCorners(corners);

			if ((mouse.x > corners[0].x && mouse.y > corners[0].y) && 
				(mouse.x > corners[1].x && mouse.y < corners[1].y) && 
				(mouse.x < corners[2].x && mouse.y < corners[2].y) && 
				(mouse.x < corners[3].x && mouse.y > corners[3].y)
			) {
				float maxScroll = ChatManager.MessagesCount * 18.5185185f;
				chatBoxTransform.offsetMin += new Vector2(0f, -Input.mouseScrollDelta.y * player.playerSettings.controlls.scrollSensitivity);
				if (chatBoxTransform.offsetMin.y > 0f) { chatBoxTransform.offsetMin = Vector2.zero; }
				if (chatBoxTransform.offsetMin.y < -maxScroll) { chatBoxTransform.offsetMin = Vector2.up * -maxScroll; }
			}

			if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) 
			{
				if (inputField.text != "") { ChatManager.Push(player, inputField.text); }
				inputField.text = "";
				IsOpen = false;
			}
		}
	}
}