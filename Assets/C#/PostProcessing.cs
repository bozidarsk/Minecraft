using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class PostProcessing : MonoBehaviour
{
	private Material material;
    private GameManager gameManager;

    void Start() 
    {
    	gameManager = (GameManager)GameObject.FindObjectOfType(typeof(GameManager));
    	material = new Material(Shader.Find("Custom/PostProcessing"));
        material.SetInt("_UseEffect", 0);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest) 
    {
    	if (material == null || gameManager == null) { Start(); }

    	material.SetColor("_fogColor", gameManager.gameSettings.postProcessing.fogColor);
	    material.SetFloat("_fogDensity", gameManager.gameSettings.postProcessing.fogDensity);
	    material.SetFloat("_fogOffset", gameManager.gameSettings.postProcessing.fogOffset);
	    material.SetFloat("_exposure", gameManager.gameSettings.postProcessing.exposure);
	    material.SetFloat("_temperature", gameManager.gameSettings.postProcessing.temperature);
	    material.SetFloat("_tint", gameManager.gameSettings.postProcessing.tint);
	    material.SetFloat("_contrast", gameManager.gameSettings.postProcessing.contrast);
	    material.SetFloat("_brightness", gameManager.gameSettings.postProcessing.brightness);
	    material.SetFloat("_colorFiltering", gameManager.gameSettings.postProcessing.colorFiltering);
	    material.SetFloat("_saturation", gameManager.gameSettings.postProcessing.saturation);
	    material.SetFloat("_gamma", gameManager.gameSettings.postProcessing.gamma);

        Graphics.Blit(src, dest, material);
    }

    public void RemoveTextureEffect() { material.SetInt("_UseEffect", 0); }
    public void SetTextureEffect(Texture2D texture) 
    {
        material.SetTexture("_Effect", texture);
        material.SetInt("_UseEffect", 1);
    }
}