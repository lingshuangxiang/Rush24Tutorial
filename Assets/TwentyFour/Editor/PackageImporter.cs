using System.IO;
using UnityEditor;
using UnityEngine;

public class PackageImporter
{
    [MenuItem("Tools/PackageImporter/导入UPR_Package文件")]
    public static void ImportUPRPackage()
    {
        // 替换为你的.unitypackage文件路径
        string packagePath = $"{Directory.GetCurrentDirectory()}\\Tools\\UPRTools_V0.14.2.unitypackage";
        AssetDatabase.ImportPackage(packagePath, false);
        AssetDatabase.Refresh();
    }
}