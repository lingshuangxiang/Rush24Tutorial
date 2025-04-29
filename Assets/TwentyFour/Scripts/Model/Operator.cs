using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.UOS.TwentyFour.Model
{
    public enum OperatorName
    {
        Plus,
        Minus,
        Multiply,
        Divide
    }

    [Serializable]
    public class Operator
    {
        [SerializeField] public OperatorName name;

        private static readonly Dictionary<OperatorName, string> NameToSymbol
            = new()
            {
                { OperatorName.Plus, "+" },
                { OperatorName.Minus, "-" },
                { OperatorName.Multiply, "×" },
                { OperatorName.Divide, "÷" },
            };

        public string Symble()
        {
            return NameToSymbol[name];
        }

        public float Calculate(float a, float b)
        {
            switch (name)
            {
                case OperatorName.Plus:
                    return a + b;
                case OperatorName.Minus:
                    return a - b;
                case OperatorName.Multiply:
                    return a * b;
                case OperatorName.Divide:
                    if (Math.Abs(b - 0) < 0.0001)
                    {
                        return float.NaN;
                    }

                    return a / b;
            }

            return float.NaN;
        }

        public string GetCalculateResult(float a, float b)
        {
            switch (name)
            {
                case OperatorName.Plus:
                    return (a + b).ToString();
                case OperatorName.Minus:
                    return (a - b).ToString();
                case OperatorName.Multiply:
                    return (a * b).ToString();
                case OperatorName.Divide:
                    if (Math.Abs(b - 0) < 0.0001)
                    {
                        return float.NaN.ToString();
                    }

                    if (a % b != 0)
                    {
                        return "a/b";
                    }

                    return (a / b).ToString();
            }

            return float.NaN.ToString();
        }

        // public string GetCalculateResult(CardGroup a, CardGroup b)
        // {
        //     
        // }
        /// <summary>
        /// 用分数来进行全部的计算
        /// </summary>
        /// <returns></returns>
        public  Fraction CalculateFraction(Fraction a, Fraction b, OperatorName targetName)
        {
            // string[] allUnitA = a.Split('/');
            // string[] allUnitB = b.Split('/');
            Fraction fa = a;
            Fraction fb = b;
            switch (targetName)
            {
                case OperatorName.Divide: //除法相当于乘法的倒数
                    Fraction ff = fa.Divide(fb);
                    return ff;
                case OperatorName.Multiply:
                    Fraction ff1 = fa.Multiply(fb);
                    return ff1;
                case OperatorName.Minus:
                    Fraction ff2 = fa.Subtract(fb);
                    return ff2;
                case OperatorName.Plus:
                    Fraction ff3 = fa.Add(fb);
                    return ff3;
            }

            return null;
        }
        
       [System.Serializable]
        public class Fraction
        {
            public int Numerator;
            public int Denominator;

            public Fraction(int numerator, int denominator)
            {
                if (denominator == 0)
                {
                    throw new ArgumentException("Denominator cannot be zero.");
                }

                // 确保分母为正
                if (denominator < 0)
                {
                    numerator = -numerator;
                    denominator = -denominator;
                }

                // 简化分数
                int gcd = GCD(Math.Abs(numerator), denominator);
                Numerator = numerator / gcd;
                Denominator = denominator / gcd;
            }

            // 计算最大公约数
            private int GCD(int a, int b)
            {
                return b == 0 ? Math.Abs(a) : GCD(b, a % b);
            }

            // 计算最小公倍数
            private int LCM(int a, int b)
            {
                return a / GCD(a, b) * b;
            }

            // 分数相乘
            public Fraction Multiply(Fraction other)
            {
                return new Fraction(this.Numerator * other.Numerator, this.Denominator * other.Denominator);
            }

            // 分数相加
            public Fraction Add(Fraction other)
            {
                int commonDenominator = LCM(this.Denominator, other.Denominator);
                int numerator1 = this.Numerator * (commonDenominator / this.Denominator);
                int numerator2 = other.Numerator * (commonDenominator / other.Denominator);
                return new Fraction(numerator1 + numerator2, commonDenominator);
            }

            // 分数相减
            public Fraction Subtract(Fraction other)
            {
                int commonDenominator = LCM(this.Denominator, other.Denominator);
                int numerator1 = this.Numerator * (commonDenominator / this.Denominator);
                int numerator2 = other.Numerator * (commonDenominator / other.Denominator);
                return new Fraction(numerator1 - numerator2, commonDenominator);
            }

            // 分数相除
            public Fraction Divide(Fraction other)
            {
                //Fraction mucc=new Fraction()
                return new Fraction(this.Numerator * other.Denominator, this.Denominator * other.Numerator);
            }

            public override string ToString()
            {
                return $"{Numerator}/{Denominator}";
            }
        }
    }
}