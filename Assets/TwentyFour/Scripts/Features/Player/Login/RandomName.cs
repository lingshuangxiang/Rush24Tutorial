using System;
using System.Collections;
using System.Collections.Generic;
using Random = UnityEngine.Random;

namespace TwentyFour.Scripts.Features.Player
{
    public class RandomName
    {
        private static List<string> nameList = new List<string>()
        {
            "神秘人"
        };

        private const int Size = 4; // size位数
        
        public static string Get(string fullUserId)
        {
            var index = Random.Range(0, nameList.Count);
            // return nameList[index] + GetRandomNumber(Size);
            return nameList[index] + GetLastCharacters(fullUserId, Size);
        }

        private static int GetRandomNumber(int size)
        {
            var minNumber = (int)Math.Pow(10, size - 1);
            var maxNumber = (int)Math.Pow(10, size) - 1;
            var number = Random.Range(minNumber, maxNumber);
            return number;
        }
        
        public static string GetLastCharacters(string str, int n)
        {
            if (str.Length <= n)
            {
                return str; // 如果字符串长度小于或等于 n，返回整个字符串
            }
            return str.Substring(str.Length - n); // 获取最后 n 位
        }
    }
}