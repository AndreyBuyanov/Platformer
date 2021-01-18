using System;

namespace Math
{

    public class Vector
    {
        private double[] _data;
        public delegate double Functor(double value);

        public Vector(double[] data)
        {
            _data = data;
        }

        public Vector(uint size)
        {
            _data = new double[size];
        }

        public double this[int i]
        {
            get
            {
                return _data[i];
            }
            set
            {
                _data[i] = value;
            }
        }

        public uint Length
        {
            get
            {
                return (uint)_data.Length;
            }
        }

        public Vector ApplyFunction(Functor functor)
        {
            Vector result = new Vector(Length);
            for (int i = 0; i < Length; i++)
            {
                result[i] = functor(this[i]);
            }
            return result;
        }

        public static double Dot(Vector v1, Vector v2)
        {
            if (v1.Length != v2.Length)
            {
                throw new IndexOutOfRangeException("Vectors must be the same size");
            }
            double result = 0f;
            for (int i = 0; i < v1.Length; i++)
            {
                result += v1[i] * v2[i];
            }
            return result;
        }
    }
}
