using System;

namespace Math
{
    public class Matrix
    {
        private double[] _data;
        private uint _rows;
        private uint _cols;

        public Matrix(uint rows, uint cols)
        {
            _rows = rows;
            _cols = cols;
            _data = new double[rows * cols];
        }

        public Vector this[int index]
        {
            get
            {
                return new Vector(new ArraySegment<double>(_data, (int)(index * _cols), (int)_cols));
            }
            set
            {
                Vector vector = value;
                if (vector.Length != _cols) {
                    throw new IndexOutOfRangeException("");
                }
                for (int i = 0; i < _cols; i++)
                {
                    _data[index * _cols + i] = vector[i];
                }
            }
        }

        public uint Rows
        {
            get
            {
                return _rows;
            }
        }

        public uint Cols
        {
            get
            {
                return _cols;
            }
        }

        public double[] Data
        {
            get
            {
                return _data;
            }
            set
            {
                double[] array = value;
                if (array.Length != _data.Length) {
                    throw new IndexOutOfRangeException("");
                }
                _data = array;
            }
        }

        public static Vector operator *(Matrix matrix, Vector vector)
        {
            if (matrix.Cols != vector.Length)
            {
                throw new IndexOutOfRangeException("Number of columns of matrix must be equal to the size of vector");
            }
            Vector result = new Vector(matrix.Rows);
            for (int i = 0; i < matrix.Rows; i++)
            {
                result[i] = Vector.Dot(matrix[i], vector);
            }
            return result;
        }
    }
}
