using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.UOS.TwentyFour
{
    public class Judge
    {
        public static bool HasAnswer(Question q)
        {
            return isSolvable(q.cards[0].number, q.cards[1].number, q.cards[2].number, q.cards[3].number);
        }

        // /// <summary>
        // /// Adds a menu item for this editor window
        // /// </summary>
        // [MenuItem("Tools/GenerateQuestions")]
        // public static void GenerateQuestions()
        // {
        //     List<List<int>> all = new List<List<int>>();
        //     string outputPath = string.Concat(Application.dataPath, Path.DirectorySeparatorChar, "Resources",
        //         Path.DirectorySeparatorChar, "questions-5.csv");
        //     Debug.Log("question file path: " + outputPath);
        //     FileStream fileStream = File.Open(outputPath, FileMode.Create, FileAccess.Write);
        //     StreamWriter streamWriter = new StreamWriter(fileStream);
        //     
        //     for (int i = 1; i <= 13; i++)
        //     {
        //         for (int j = i; j <= 13; j++)
        //         {
        //             for (int k = j; k <= 13; k++)
        //             {
        //                 for (int l = k; l <= 13; l++)
        //                 {
        //                     if (Judge.isSolvable(i, j, k, l))
        //                     {
        //                         all.Add(new List<int>(){i,j,k,l});
        //                         // streamWriter.WriteLine($"{i},{j},{k},{l}");
        //                     }
        //                 }
        //             }
        //         }
        //     }
        //     streamWriter.Flush();
        //     streamWriter.Close();
        //     fileStream.Close();
        // }
        //
        static double cal(double a, double b, int op)
        {
            switch (op)
            {
                case 0:
                    return (a + b);
                case 1:
                    return (a - b);
                case 2:
                    return (a * b);
                case 3:
                    break;
            }

            return (a / b);
        }

        static bool isEqual(double d1, double d2)
        {
            double d = d1 - d2;
            d = Math.Abs(d);
            return (d <= 0.001);
        }

        public static bool isSolvable(int v0, int v1, int v2, int v3)
        {
            int count = 0, f1, f2, f3;
            for (f1 = 0; f1 < 4; f1++)
            for (f2 = 0; f2 < 4; f2++)
            for (f3 = 0; f3 < 4; f3++)
            {
                double t1, t2, t3;
                t1 = cal(v0, v1, f1);
                t2 = cal(t1, v2, f2);
                t3 = cal(t2, v3, f3);
                if (isEqual(t3, 24))
                    count++;
                t1 = cal(v0, v1, f1);
                t2 = cal(v2, v3, f3);
                t3 = cal(t1, t2, f2);
                if (isEqual(t3, 24))
                    count++;
                t1 = cal(v1, v2, f2);
                t2 = cal(v0, t1, f1);
                t3 = cal(t2, v3, f3);
                if (isEqual(t3, 24))
                    count++;
                t1 = cal(v1, v2, f2);
                t2 = cal(t1, v3, f3);
                t3 = cal(v0, t2, f1);
                if (isEqual(t3, 24))
                    count++;
                t1 = cal(v2, v3, f3);
                t2 = cal(v1, t1, f2);
                t3 = cal(v0, t2, f1);
                if (isEqual(t3, 24))
                    count++;
            }

            return count > 0;
        }
    }
}