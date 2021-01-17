using System;
using System.Linq;

namespace NN
{
    public struct LayerConfig
    {
        public uint neurons;
        public ActivationFunction fn;
    }

    public class NeuralNetwork
    {
        private Math.Matrix[] _weights;
        private LayerConfig[] _layers;

        public NeuralNetwork(uint inputs, LayerConfig[] layers)
        {
            _layers = layers;
            _weights = new Math.Matrix[_layers.Length];
            _weights[0] = new Math.Matrix(_layers[0].neurons, inputs);
            for (int i = 1; i < _layers.Length; i++)
            {
                _weights[i] = new Math.Matrix(_layers[i].neurons, _layers[i - 1].neurons);
            }
        }

        public uint LayersCount
        {
            get
            {
                return (uint)_layers.Length;
            }
        }

        public uint WeightsCount
        {
            get
            {
                uint weightsCount = 0;
                for (int i = 0; i < _weights.Length; i++)
                {
                    weightsCount += _weights[i].Rows * _weights[i].Cols;
                }
                return weightsCount;
            }
        }

        public Math.Matrix this[int i]
        {
            get
            {
                return _weights[i];
            }
            set
            {
                _weights[i] = value;
            }
        }

        public void SetRandomWeights(double minValue, double maxValue, Random random)
        {
            double range = maxValue - minValue;
            for (int layer = 0; layer < _weights.Length; layer++)
            {
                for (int row = 0; row < _weights[layer].Rows; row++)
                {
                    for (int col = 0; col < _weights[layer].Cols; col++)
                    {
                        _weights[layer][row][col] = (random.NextDouble() * range) + minValue;
                    }
                }
            }
        }

        public Math.Vector Forward(Math.Vector input)
        {
            Math.Vector output = Forward(input, 0);
            for (int i = 1; i < _weights.Length; i++)
            {
                output = Forward(output, i);
            }
            return output;
        }

        private Math.Vector Forward(Math.Vector input, int layer)
        {
            return (_weights[layer] * input).ApplyFunction(ActivationFunctions.Get(_layers[layer].fn));
        }
    }

}
