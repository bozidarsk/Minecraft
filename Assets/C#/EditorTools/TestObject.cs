#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
	public class TestObject : MonoBehaviour
	{
		public static Vector3 position 
		{
			set { instance.transform.position = value; }
			get { return instance.transform.position; }
		}

		public static Vector3 localScale 
		{
			set { instance.transform.localScale = value; }
			get { return instance.transform.localScale; }
		}

		public static Vector3 eulerAngles 
		{
			set { instance.transform.eulerAngles = value; }
			get { return instance.transform.eulerAngles; }
		}

		private static GameObject instance;
		void Awake() { TestObject.instance = gameObject; }
	}
}
#endif