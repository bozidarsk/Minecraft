using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MessageController : MonoBehaviour
{
	private float timeMax = 5f;
	private float alphaFade = 0.1f;
	private CanvasGroup canvasGroup;

	void Start() 
	{
		gameObject.GetComponentsInChildren<TMPro.TextMeshProUGUI>()[0].text = gameObject.name;
		canvasGroup = gameObject.GetComponent<CanvasGroup>();
		StartCoroutine(Timer());
	}

	private IEnumerator Timer() 
	{
		float time = 0f;
		while (time < timeMax) 
		{
			time += 1f;
			yield return new WaitForSeconds(1f);
		}

		StartCoroutine(FadeAlpha());
	}

	private IEnumerator FadeAlpha() 
	{
		while (canvasGroup.alpha > 0f) 
		{
			canvasGroup.alpha -= alphaFade;
			yield return new WaitForSeconds(alphaFade);
		}

		Destroy(gameObject);
	}
}