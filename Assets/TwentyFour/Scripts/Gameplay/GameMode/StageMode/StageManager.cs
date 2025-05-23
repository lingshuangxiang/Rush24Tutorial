using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.UOS.TwentyFour.Model;
using UnityEngine;
using Logger = TwentyFour.Scripts.Utilities.Logger;
using TwentyFour.Scripts.Features.Save;

namespace Unity.UOS.TwentyFour
{
    public static class StageManager
    {
        private const string KEY_SCORES = "Scores";

        private const string URL_CDN_PREFIX_LATEST =
            @"https://a.unity.cn/client_api/v1/buckets/569a2f81-b315-487f-be07-aadbe4e145f6/release_by_badge/latest/content/";
        private const string URL_STAGE_INFO = URL_CDN_PREFIX_LATEST + @"questions_1.csv";
        
        private static List<Stage> _allStages = new();
        private static List<Stage> _battleStages = new List<Stage>();
        
        public static List<int> playerStageScores = new(_allStages.Count);
        public static int? selectedStage;

        
        public static void SetAllStages(List<Stage> stages, GameMode mode)
        {
            if (mode == GameMode.Stage)
            {
                _allStages = stages;
            }
            else
            {
                _battleStages = stages;
            }
        }

        public static List<Stage> GetAllStages(GameMode mode)
        {
            if (mode == GameMode.Stage)
            {
                return _allStages;
            }

            return _battleStages;
        }

        public static void LoadStageScores(List<int> scores)
        {
            try
            {
                playerStageScores = scores;
            }
            catch (Exception e)
            {
                Logger.LogError(e.Message);
                playerStageScores = new List<int>(_allStages.Count);
                for (int i = 0; i < _allStages.Count; i++)
                {
                    playerStageScores.Add(i < scores.Count ? scores[i] : 0);
                }
            }

            //题库数量大于分数数量，需要补全
            if (_allStages.Count > playerStageScores.Count)
            {
                var temp = new List<int>();
                for (int i = 0; i < _allStages.Count - playerStageScores.Count; i++)
                {
                    temp.Add(0);
                }
                playerStageScores.AddRange(temp);

            }
            

            Logger.Log($"stage clear count: {GetClearCount()}");
        }

        public static void LoadEmptyStageScore()
        {
            LoadStageScores(new List<int>());
        }

        private static string GetFileContent()
        {
            return "";
        }

        private static void HandleFileContentWithSpace(string text)
        {
            _allStages = new List<Stage>();
            string fileString = text.Replace("\r", ""); 
            string[] lines = fileString.Split(" ");
            foreach (var line in lines)
            {
                string[] numStrings = line.Split(",");
                // var values = line.Split(',');  
                if (numStrings.Length < 4)
                {
                    continue;
                }
                            
                var intValues = numStrings.Select(int.Parse).ToList();

                if (intValues.Count == 4)
                {
                    _allStages.Add(new Stage(){
                        index = _allStages.Count,
                        question = new Question() {
                            cards = new List<Card>()
                            {
                                new(number: intValues[0], index: 0),
                                new(number: intValues[1], index: 1),
                                new(number: intValues[2], index: 2),
                                new(number: intValues[3], index: 3),
                            }
                        }
                    });
                }
            }
        }
        private static void HandleFileContent(string text)
        {
            _allStages = new List<Stage>();
            string fileString = text.Replace("\r", ""); 
            string[] lines = fileString.Split("\n");
            foreach (var line in lines)
            {
                string[] numStrings = line.Split(",");
                // var values = line.Split(',');  
                if (numStrings.Length < 4)
                {
                    continue;
                }
                            
                var intValues = numStrings.Select(int.Parse).ToList();

                if (intValues.Count == 4)
                {
                    _allStages.Add(new Stage(){
                        index = _allStages.Count,
                        question = new Question() {
                            cards = new List<Card>()
                            {
                                new(number: intValues[0], index: 0),
                                new(number: intValues[1], index: 1),
                                new(number: intValues[2], index: 2),
                                new(number: intValues[3], index: 3),
                            }
                        }
                    });
                }
            }
        }
        
        
        
        
        
        public static Task LoadAllStagesFromCSV()
        {
            var tcs = new TaskCompletionSource<object>();
            // 将 csv 文件托管到 CDN 则使用以下代码： 
            // Http.Get(URL_STAGE_INFO)
            //     .OnSuccess(result => {
            //         if (!string.IsNullOrEmpty(result.text))
            //         {
            //             HandleFileContent(result.text);
            //         }
            //         else
            //         {
            //             Debug.Log("error fetching game data");
            //         }
            //         tcs.SetResult(null);
            //     })
            //     .Send();
            
            // 使用本地 csv 文件
            var file = Resources.Load<TextAsset>("levels");
            HandleFileContent(file.text);
            tcs.SetResult(null);
            return tcs.Task;
        }

        public static void LoadAllStagesFromRemoteConfig()
        {
            var file = Resources.Load<TextAsset>("levels").text;
            HandleFileContent(file);
        }
        
        public static List<Stage> ReturnAllStages()
        {
            // 使用本地 csv 文件
            string text = Resources.Load<TextAsset>("levels").text; 
            _allStages = new List<Stage>();
            string fileString = text.Replace("\r", "");
            string[] lines = fileString.Split("\n");
            foreach (var line in lines)
            {
                string[] numStrings = line.Split(",");
                // var values = line.Split(',');  
                if (numStrings.Length < 4)
                {
                    continue;
                }

                var intValues = numStrings.Select(int.Parse).ToList();

                if (intValues.Count == 4)
                {
                    _allStages.Add(new Stage()
                    {
                        index = _allStages.Count,
                        question = new Question()
                        {
                            cards = new List<Card>()
                            {
                                new(number: intValues[0], index: 0),
                                new(number: intValues[1], index: 1),
                                new(number: intValues[2], index: 2),
                                new(number: intValues[3], index: 3),
                            }
                        }
                    });
                }
            }

            return _allStages;
        }
        
        
        private static int GetCurrentStageIndex()
        {
            if (!selectedStage.HasValue)
            {
                for (int i = 0; i < playerStageScores.Count; i++)
                {
                    if (playerStageScores[i] == 0)
                    {
                        selectedStage = i==0?0:i-1;
                    }
                }
            }

            return selectedStage.Value;
        }
        
        public static Stage NextStage(int? score = null)
        {
            int currentIndex = GetCurrentStageIndex();

            //set score
            if (score.HasValue)
            {
                // SetStageScore(score.Value);
                
                //goto next stage
                if (currentIndex < _allStages.Count - 1)
                {
                    currentIndex += 1;
                }
                else
                {
                    return null;
                }
                
                selectedStage = currentIndex;
            }
            
            if (currentIndex >= 0 && currentIndex < _allStages.Count)
            {
                return _allStages[currentIndex];
            }

            return null;
        }

        public static void SetStageScore(int score)
        {
            int currentIndex = GetCurrentStageIndex();
            if (currentIndex >= 0)
            {
                playerStageScores[currentIndex] = score;
                UOSSave.SavePlayerProgress(playerStageScores);
            }
        }

        public static int GetClearCount()
        {
            return playerStageScores.Count(score => score > 0);
        }
        
    }
}