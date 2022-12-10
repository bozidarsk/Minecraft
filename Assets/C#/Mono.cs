using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public class Mono : MonoBehaviour
	{
		public static event System.Action onAwake;
		public static event System.Action onStart;
		public static event System.Action onUpdate;
		public static event System.Action onFixedUpdate;
		public static event System.Action onLateUpdate;
		public static event System.Action onGUI;
		public static event System.Action onConnectedToServer;
		public static event System.Action onPlayerConnected;
		public static event System.Action onPlayerDisconnected;
		public static event System.Action onPostRender;
		public static event System.Action onPreCull;
		public static event System.Action onPreRender;
		public static event System.Action<RenderTexture, RenderTexture> onRenderImage;
		public static event System.Action onSerializeNetworkView;
		public static event System.Action onServerInitialized;

		new public static void StartCoroutine(IEnumerator routine) { instance.StartCoroutine(routine); }

		private static MonoBehaviour instance;

		void Awake() { instance = this; if (Mono.onAwake != null) { Mono.onAwake(); } }
		void Start() { if (Mono.onStart != null) { Mono.onStart(); } }
		void Update() { if (Mono.onUpdate != null) { Mono.onUpdate(); } }
		void FixedUpdate() { if (Mono.onFixedUpdate != null) { Mono.onFixedUpdate(); } }
		void LateUpdate() { if (Mono.onLateUpdate != null) { Mono.onLateUpdate(); } }

		void OnGUI() { if (Mono.onGUI != null) { Mono.onGUI(); } }
		void OnConnectedToServer() { if (Mono.onConnectedToServer != null) { Mono.onConnectedToServer(); } }
		void OnPlayerConnected() { if (Mono.onPlayerConnected != null) { Mono.onPlayerConnected(); } }
		void OnPlayerDisconnected() { if (Mono.onPlayerDisconnected != null) { Mono.onPlayerDisconnected(); } }
		void OnPostRender() { if (Mono.onPostRender != null) { Mono.onPostRender(); } }
		void OnPreCull() { if (Mono.onPreCull != null) { Mono.onPreCull(); } }
		void OnPreRender() { if (Mono.onPreRender != null) { Mono.onPreRender(); } }
		void OnRenderImage(RenderTexture src, RenderTexture dest) { if (Mono.onRenderImage != null) { Mono.onRenderImage(src, dest); } }
		void OnSerializeNetworkView() { if (Mono.onSerializeNetworkView != null) { Mono.onSerializeNetworkView(); } }
		void OnServerInitialized() { if (Mono.onServerInitialized != null) { Mono.onServerInitialized(); } }
	}
}