using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DroppedItemController : MonoBehaviour
{
	[HideInInspector] public bool useCooldown;
	[HideInInspector] public GameManager gameManager;
	private float timeMax = 3f;

	void Start() { StartCoroutine(Timer()); }

	private IEnumerator Timer() 
	{
		float time = 0f;
		while (time < timeMax && useCooldown) 
		{
			time += 1f;
			yield return new WaitForSeconds(1f);
		}

		MeshCollider collider = gameObject.GetComponent<MeshCollider>();
		collider.enabled = true;
		collider.convex = true;
		collider.isTrigger = true;
	}
}