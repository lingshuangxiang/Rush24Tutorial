using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Unity.UOS.TwentyFour.Editor
{
    public class TutorialWindow : EditorWindow
    {
        private string _markdownText;
        private Vector2 scrollPosition = Vector2.zero;
        
        [MenuItem("Tutorial/Show Tutorial")]
        public static void ShowTutorial()
        {
            TutorialWindow window = GetWindow<TutorialWindow>(false, "Tutorial", true);
            window.minSize = new Vector2(300, 200);
            window.Show();
        }
        
        // [InitializeOnLoadMethod]
        // private static void OnEditorLaunch()
        // {
        //     // 代码重新编译时该方法也会重新执行
        //     // 使用时间判断避免重复执行
        //     if (EditorApplication.timeSinceStartup < 300)
        //     {
        //         EditorApplication.delayCall += () =>
        //         {
        //             ShowTutorial();
        //         };
        //     }
        // }

        private void OnEnable()
        {
            // 加载 Markdown 文件内容
            _markdownText = System.IO.File.ReadAllText("Assets/README.md");
        }

        private void CreateGUI()
        {
            var tree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/TwentyFour/Editor/TutorialTree.uxml");
            rootVisualElement.Add(tree.Instantiate());
        }
    }
}
