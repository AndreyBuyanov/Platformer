using System;

namespace Math
{
    public class Matrix
    {
        private Vector[] _data;

        public Matrix(uint rows, uint cols)
        {
            _data = new Vector[rows];
            for (int r = 0; r < rows; r++)
            {
                _data[r] = new Vector(cols);
            }
        }

        public Vector this[int index]
        {
            get
            {
                return _data[index];
            }
            set
            {
                if (value.Length != Cols) {
                    throw new IndexOutOfRangeException("");
                }
                _data[index] = value;
            }
        }

        public uint Rows
        {
            get
            {
                return (uint)_data.Length;
            }
        }

        public uint Cols
        {
            get
            {
                return (uint)_data[0].Length;
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
