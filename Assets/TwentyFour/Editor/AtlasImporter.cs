using UnityEngine;
using System.IO;
using UnityEditor;

public class AtlasImporter : AssetPostprocessor
{
    const string WebGL_Name = "WebGL";
    const string Stanalone_Name = "Standalone";
    const int Compress_Quality = 100;
    public void OnPreprocessTexture()
    {
        string dirName = Path.GetDirectoryName(assetPath);
        string fileName = Path.GetFileName(assetPath);


        if (dirName == null)
        {
            Debug.LogError("导入资源时，路径出错");
            return;
        }
        
        if (Path.GetDirectoryName(dirName)!.Contains(Path.Combine("TwentyFour","Sprites")) )
        {
            Debug.Log($"导入资源 ："+fileName);
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.maxTextureSize = 2048;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;
            //textureImporter.filterMode = FilterMode.Point;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.alphaIsTransparency = true;
            
            // WebGL设置纹理格式
            TextureImporterPlatformSettings settings = textureImporter.GetPlatformTextureSettings(WebGL_Name);
            
            // 根据不同文件夹设定压缩格式
            if(dirName.Contains("Background"))
                settings.format = TextureImporterFormat.ASTC_8x8;
            else
                settings.format = TextureImporterFormat.ASTC_4x4;
            settings.overridden = true;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.compressionQuality = Compress_Quality;
            textureImporter.SetPlatformTextureSettings(settings);
            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
            //AssetDatabase.Refresh();
        }

        if (Path.GetDirectoryName(dirName)!.Contains(Path.Combine("TwentyFour", "Resources", "Icons")))
        {
            Debug.Log($"导入资源 ："+fileName);
            TextureImporter textureImporter = (TextureImporter)assetImporter;
            textureImporter.maxTextureSize = 2048;
            textureImporter.textureType = TextureImporterType.Sprite;
            textureImporter.mipmapEnabled = false;
            textureImporter.isReadable = false;
            //textureImporter.filterMode = FilterMode.Point;
            textureImporter.npotScale = TextureImporterNPOTScale.None;
            textureImporter.alphaIsTransparency = true;
            
            // WebGL设置纹理格式
            TextureImporterPlatformSettings settings = textureImporter.GetPlatformTextureSettings(WebGL_Name);
            settings.overridden = true;
            settings.textureCompression = TextureImporterCompression.Uncompressed;
            settings.compressionQuality = Compress_Quality;
            textureImporter.SetPlatformTextureSettings(settings);
            EditorUtility.SetDirty(textureImporter);
            textureImporter.SaveAndReimport();
        }
    }
}
