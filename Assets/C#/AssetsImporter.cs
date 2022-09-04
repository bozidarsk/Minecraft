#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor; 

internal sealed class AssetImporter : AssetPostprocessor
{
    // This event is raised when a texture asset is imported
    private void OnPreprocessTexture()
    {
        var importer = assetImporter as TextureImporter;
        importer.textureType = TextureImporterType.Default;
        importer.npotScale = TextureImporterNPOTScale.None;
        importer.mipmapEnabled = false;
        importer.streamingMipmaps = false;
        importer.isReadable = false;
        importer.sRGBTexture = true;
        importer.wrapMode = TextureWrapMode.Clamp;
        importer.filterMode = FilterMode.Point;
        importer.anisoLevel = 1;
        importer.spritePixelsPerUnit = 1;
        importer.compressionQuality = 100;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.alphaIsTransparency = true;
    }
}

#endif