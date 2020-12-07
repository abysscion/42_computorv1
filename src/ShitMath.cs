using System;

namespace computorv1
{
    public static class ShitMath
    {
        public static int Abs(int value)
        {
            return value < 0 ? -value : value;
        }
        public static float Abs(float value)
        {
            return value < 0 ? -value : value;
        }
        public static double Abs(double value)
        {
            return value < 0 ? -value : value;
        }

        public static double Min(double a, double b)
        {
            return a <= b ? a : b;
        }
        
        public static double Max(double a, double b)
        {
            return a >= b ? a : b;
        }

        public static double Sqrt(double x)
        {
            if (x < 0) 
                return -1;
            if (x == 0 || Math.Abs(x - 1) < 0.000000001)
                return x;
            
            double res = x;
            while (res * res > x)
                res *= 0.5;
            while (res * res < x)
                res *= 2;
            double half = res * 0.5;
            while (half >= (x / 1000000000000 > 1 ? 1 : x / 1000000000000))
            {
                res += half * (res * res - x > 0 ? -1 : 1);
                half *= 0.5;
            }

            if (res - (int)(res + 0.000000001) < 0.000000001)
                return (int)(res + 0.000000001);
            return res;
        }

        public static double Pow(double baseValue, int power)
        {
            if (baseValue > int.MaxValue)
                throw new Exception("can't work with numbers bigger than INT_MAX: " + baseValue + "^" + power);
            if (power == 0)
                return 1;
            if (power == 1)
                return baseValue;

            double retValue = 1;
            var isNegativePower = power < 0;
            var startPower = power;
            var currentBase = baseValue;
            
            power = isNegativePower ? -power : power;
            while (power != 0)
            {
                if ((power & 1) == 1)
                    retValue *= currentBase;
                currentBase *= currentBase;
                if (retValue > int.MaxValue)
                    throw new Exception("can't work with numbers bigger than INT_MAX: " + baseValue + "^" + startPower);
                power >>= 1;
            }

            if (isNegativePower)
                retValue = 1 / retValue;
            return retValue;
        }
    }
}
