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