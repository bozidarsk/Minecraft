using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Minecraft 
{
    [RequireComponent(typeof(Camera))]
    public class PostProcessing : MonoBehaviour
    {
        private Material material;

        void Start() 
        {
            material = GameSettings.materials.postProcessing;
            material.SetInt("_UseEffect", 0);
        }

        void OnRenderImage(RenderTexture src, RenderTexture dest) 
        {
            if (material == null) { Start(); }

            material.SetColor("_fogColor", GameSettings.postProcessing.fogColor);
            material.SetFloat("_fogDensity", GameSettings.postProcessing.fogDensity);
            material.SetFloat("_fogOffset", GameSettings.postProcessing.fogOffset);
            material.SetFloat("_exposure", GameSettings.postProcessing.exposure);
            material.SetFloat("_temperature", GameSettings.postProcessing.temperature);
            material.SetFloat("_tint", GameSettings.postProcessing.tint);
            material.SetFloat("_contrast", GameSettings.postProcessing.contrast);
            material.SetFloat("_brightness", GameSettings.postProcessing.brightness);
            material.SetFloat("_colorFiltering", GameSettings.postProcessing.colorFiltering);
            material.SetFloat("_saturation", GameSettings.postProcessing.saturation);
            material.SetFloat("_gamma", GameSettings.postProcessing.gamma);

            Graphics.Blit(src, dest, material);
        }

        public void RemoveTextureEffect() { material.SetInt("_UseEffect", 0); }
        public void SetTextureEffect(Texture2D texture) 
        {
            material.SetTexture("_Effect", texture);
            material.SetInt("_UseEffect", 1);
        }
    }
}