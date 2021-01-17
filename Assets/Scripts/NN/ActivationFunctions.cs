using System;

namespace NN
{
    public enum ActivationFunction
    {
        Sigmoid
    }
    public static class ActivationFunctions
    {
        private static double Sigmoid(double value)
        {
            if (value > 10) return 1.0;
            else if (value < -10) return 0.0;
            else return 1.0 / (1.0 + System.Math.Exp(-value));
        }

        public static Math.Vector.Functor Get(ActivationFunction fn)
        {
            switch (fn)
            {
                case ActivationFunction.Sigmoid:
                    return Sigmoid;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
