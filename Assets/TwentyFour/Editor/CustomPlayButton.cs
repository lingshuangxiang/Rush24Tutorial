#if UNITY_EDITOR
using System.Reflection;
using Unity.UOS.TwentyFour;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TwentyFour.Scripts.Gameplay.HomePage;

[InitializeOnLoad]
public static class CustomPlayButton
{
    static CustomPlayButton()
    {
        EditorApplication.update += OnUpdate;
    }

    private static ScriptableObject toolbar;
    static void OnUpdate()
    {
        if (toolbar == null)
        {
            var toolbarType = typeof(Editor).Assembly.GetType("UnityEditor.Toolbar");
            var toolbars = Resources.FindObjectsOfTypeAll(toolbarType);
            toolbar = (ScriptableObject)toolbars[0];
            var root = toolbar.GetType().GetField("m_Root", BindingFlags.NonPublic | BindingFlags.Instance);
            var visualRoot = root.GetValue(toolbar) as VisualElement;
            var rightZone = visualRoot.Q("ToolbarZoneRightAlign");

            var container = new IMGUIContainer(() =>
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("登出账号"))
                {
                    // EditorPrefs.SetBool("START_GAME_OPTION", true);
                    // EditorApplication.ExecuteMenuItem("Edit/Play");
                    var m = GameObject.FindObjectOfType<MainSceneManager>();
                    m?.Logout();
                }
                GUILayout.EndHorizontal();
            });
            rightZone.Add(container);
        }
    }
}
#endif