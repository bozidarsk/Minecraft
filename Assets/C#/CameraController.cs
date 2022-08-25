using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Minecraft 
{
	[RequireComponent(typeof(Camera))]
	public class CameraController : MonoBehaviour
	{

		private Player player;
		private GameSettings gameSettings;
		private PlayerSettings playerSettings;
		new private Camera camera;

		void Awake() { camera = gameObject.GetComponent<Camera>(); }

		void Initialize() 
		{
			player = gameObject.transform.parent.gameObject.GetComponent<Player>();
			gameSettings = player.gameManager.gameSettings;
			// gameSettings = ((GameManager)GameObject.FindObjectOfType(typeof(GameManager))).gameSettings;
			playerSettings = player.playerSettings;

			gameObject.transform.SetParent(player.armature.head.transform);
		}

		void FixedUpdate() 
		{
			if (gameObject.transform.parent.name != "Head") { Initialize(); }
			if (player.inventory.IsOpen || player.chat.IsOpen) { return; }

			Vector3 direction = new Vector3(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"), 0f);
			if (playerSettings.controlls.invertMouse) { direction *= -1f; }
			direction.x *= playerSettings.controlls.sensitivity.x;
			direction.y *= playerSettings.controlls.sensitivity.y;

			float z = -direction.y + player.armature.head.transform.localEulerAngles.z;
			if ((z < 360f - (gameSettings.player.rotationLimit.y / 2f) && z > 180f) || 
				(z > (gameSettings.player.rotationLimit.y / 2f) && z < 180f)
			) { z += direction.y; }

			float y = direction.x + player.armature.head.transform.localEulerAngles.y;
			if ((y < 360f - gameSettings.player.rotationLimit.x && y > 180f) || 
				(y > gameSettings.player.rotationLimit.x && y < 180f)
			) { player.gameObject.transform.localEulerAngles = new Vector3(0f, direction.x + player.gameObject.transform.localEulerAngles.y, 0f);
				y -= direction.x; }

			player.armature.head.transform.localEulerAngles = new Vector3(0f, y, z);

			if (Input.GetKey(playerSettings.controlls.keyCodes.MoveForward) || 
				Input.GetKey(playerSettings.controlls.keyCodes.MoveBackwards) || 
				Input.GetKey(playerSettings.controlls.keyCodes.MoveLeft) || 
				Input.GetKey(playerSettings.controlls.keyCodes.MoveRight)
			) 
			{
				float angle = 90f * Math2.Dot(player.armature.head.transform.forward, player.gameObject.transform.forward);
				player.gameObject.transform.eulerAngles += new Vector3(0f, -angle, 0f);
				player.armature.head.transform.eulerAngles += new Vector3(0f, +angle, 0f);
			}
		}
	}
}