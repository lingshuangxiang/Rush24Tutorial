using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Logger = Unity.UOS.TwentyFour.Common.Logger;

namespace Unity.UOS.TwentyFour
{
//to manage scene switching
    public static class GameRouter
    {
        // public static GameRouter instance;
        public const string FolderPath = "TwentyFour/Scenes/";
        
        public const string LoadingScene = "LoadingScene";
        public const string StartScene = "FirstInitScene";
        public const string MainScene = "MainScene";
        public const string BattleScene = "BattleScene";
        public const string StageScene = "StageScene";

        //flags
        public static bool isLoggingOut = false;

        [RuntimeInitializeOnLoadMethod]
        static void Initialize()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (!scene.name.Equals(StartScene))
            {
                SceneManager.LoadScene(StartScene);
            }

            //Init all SDK
            MatchMakingManager.StartInitializeMatchMaking();
        }

        // Start is called before the first frame update
        // void Start()
        // {
        //     instance = this;
        //     DontDestroyOnLoad(gameObject);
        // }
        //
        // // Update is called once per frame
        // void Update()
        // {
        //
        // }


        public static void LoadLoadingScene()
        {
            SceneManager.LoadScene(FolderPath + LoadingScene);
        }

        public static void LoadStageGameScene()
        {
            Logger.Log("Load Stage Game Scene");
            AysncLoadingScenenEf.LoadScene(SceneManager.GetActiveScene().name, FolderPath + StageScene, false);
        }

        public static void LoadBattleGameScene()
        {
            Logger.Log("Load Battle Game Scene");
            //    SceneManager.LoadScene("TwentyFour/Scenes/BattleScene");
            AysncLoadingScenenEf.LoadScene(SceneManager.GetActiveScene().name, FolderPath + BattleScene, false);
        }

        public static void LoadHomeScene()
        {
            Logger.Log("Load Home Scene");
            AysncLoadingScenenEf.LoadScene(SceneManager.GetActiveScene().name, FolderPath + MainScene);
        }
        public static void LoadHomeSceneFirst()
        {
            Logger.Log("Load Home Scene");
            AysncLoadingScenenEf.LoadScene(SceneManager.GetActiveScene().name, FolderPath + MainScene, false);
        }

        public static void BackAndLogout()
        {
            isLoggingOut = true;
            SceneManager.LoadScene(FolderPath + StartScene);
        }
    }
}