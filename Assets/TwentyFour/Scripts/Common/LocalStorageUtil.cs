using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Utils
{
    public class LocalStorageUtil
    {
        public static void SetInt(string k ,int v)
        {
            PlayerPrefs.SetInt($"{k.ToUpper()}",v);
        }

        public static int GetInt(string k)
        {
            return PlayerPrefs.GetInt($"{k.ToUpper()}");
        }
        
        public static void SetString(string k ,string v)
        {
            PlayerPrefs.SetString($"{k.ToUpper()}",v);
        }

        public static string GetString(string k)
        {
            return PlayerPrefs.GetString($"{k.ToUpper()}");
        }
        
        public static void SetFloat(string k ,float v)
        {
            PlayerPrefs.SetFloat($"{k.ToUpper()}",v);
        }

        public static float GetFloat(string k)
        {
            return PlayerPrefs.GetFloat($"{k.ToUpper()}");
        }

        public static void Delete(string k)
        {
            PlayerPrefs.DeleteKey($"{k.ToUpper()}");
        }
        public static void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public static bool HasKey(string k)
        {
            return PlayerPrefs.HasKey($"{k.ToUpper()}");
        }
        
        public static string SaveLocal(Texture2D screenShot,string name,string localPath = null)
        {
            string folder = localPath ?? Application.persistentDataPath;
            if(string.IsNullOrEmpty(folder))
                return null;
            byte[] byts = screenShot.EncodeToPNG();
            string path = Path.Combine(folder , $"{name}.png");
            FileStream fs = new FileStream(path, FileMode.Create);
            fs.Write(byts,0,byts.Length);
            fs.Close();
            return path;
        }
       
        public static Texture2D GetAstcTexture(byte[] bytes ,TextureFormat format,int width,int height)
        {
            var loadTexture = new Texture2D(width, height, format, false, false);
            loadTexture.LoadRawTextureData(bytes);
            loadTexture.Apply(false);
            return loadTexture;
        }
    }

    public class LocalStorageKeys
    {
        public const string BGM_ON = nameof(BGM_ON);
        public const string AFX_ON = nameof(AFX_ON);
        public const string DEFAULT_CATEGORY_UPDATED = nameof(DEFAULT_CATEGORY_UPDATED);
    }
}