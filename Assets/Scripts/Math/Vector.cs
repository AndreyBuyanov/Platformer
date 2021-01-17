using System;

namespace Math
{

    internal class ArrayView
    {
        private ArraySegment<double> _slice;

        public ArrayView(ArraySegment<double> slice)
        {
            _slice = slice;
        }
        public double this[int i]
        {
            get
            {
                return _slice.Array[_slice.Offset + i];
            }
            set
            {
                _slice.Array[_slice.Offset + i] = value;
            }
        }

        public uint Length
        {
            get
            {
                return (uint)_slice.Count;
            }
        }
    }

    public class Vector
    {
        private double[] _data;
        private ArrayView _view;
        public delegate double Functor(double value);

        public Vector(double[] data)
        {
            _data = data;
            _view = new ArrayView(new ArraySegment<double>(_data, 0, _data.Length));
        }
        public Vector(uint size)
        {
            _data = new double[size];
            _view = new ArrayView(new ArraySegment<double>(_data, 0, _data.Length));
        }

        public Vector(ArraySegment<double> slice)
        {
            _view = new ArrayView(slice);
        }

        public double this[int i]
        {
            get
            {
                return _view[i];
            }
            set
            {
                _view[i] = value;
            }
        }

        public uint Length
        {
            get
            {
                return _view.Length;
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
