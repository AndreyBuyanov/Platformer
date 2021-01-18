using System;
using System.Collections.Generic;
using UnityEngine;
using System.Timers;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float speed = 2.0f;
    [SerializeField]
    private float jumpForce = 5.0f;
    [SerializeField]
    private float jumpThreshold = 0.0f;
    [SerializeField]
    private int aliveTime = 30000;

    private System.Random _random = new System.Random();

    private Rigidbody2D _body;
    private Sensor[] _sensors;
    private GroundSensor _groundSensor;

    private double _move;
    private double _jump;
    
    private bool _facingLeft = false;

    private bool _alive = false;
    private Timer _aliveTimer;

    private bool _initialized = false;

    private GA.Individual _individual;
    private NN.NeuralNetwork _nn;

    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    private double[] GetSensorsOutput()
    {
        double[] sensorsOutput = new double[_sensors.Length];
        for (int i = 0; i < _sensors.Length; i++)
        {
            sensorsOutput[i] = _sensors[i].Output;
        }
        return sensorsOutput;
    }

    private void UpdateControlValues(double[] sensorsOutput)
    {
        Math.Vector controls = _nn.Forward(new Math.Vector(sensorsOutput));
        _move = (controls[0] * 2 - 1) * speed;
        _jump = controls[1] * 2 - 1;
    }

    private void FlipX()
    {
        Vector3 localScale = transform.localScale;
        if (_move < -0.01)
        {
            _facingLeft = false;
        }
        else if (_move > 0.01)
        {
            _facingLeft = true;
        }
        if (_facingLeft && localScale.x < 0 || !_facingLeft && localScale.x > 0)
        {
            localScale.x *= -1;
        }
        transform.localScale = localScale;
    }

    private void InitNN(uint[] nnTopology)
    {
        NN.LayerConfig[] nnConfig = new NN.LayerConfig[nnTopology.Length + 1];
        for (int i = 0; i < nnTopology.Length; i++)
        {
            nnConfig[i].neurons = nnTopology[i];
            nnConfig[i].fn = NN.ActivationFunction.Sigmoid;
        }
        nnConfig[nnConfig.Length - 1].neurons = 2;
        nnConfig[nnConfig.Length - 1].fn = NN.ActivationFunction.Sigmoid;

        _nn = new NN.NeuralNetwork((uint)_sensors.Length, nnConfig);
        _nn.SetRandomWeights(-1.0, 1.0, _random);
    }

    private void CopytWeightsToIndividual()
    {
        int individualPos = 0;
        for (int layer = 0; layer < _nn.LayersCount; layer++)
        {
            for (int row = 0; row < _nn[layer].Rows; row++)
            {
                for (int col = 0; col < _nn[layer].Cols; col++)
                {
                    _individual[individualPos++] = _nn[layer][row][col];
                }
            }
        }
    }

    void FixedUpdate()
    {
        if (_initialized)
        {
            double[] sensorsOutput = GetSensorsOutput();
            UpdateControlValues(sensorsOutput);

            _body.velocity = new Vector2((float)_move, _body.velocity.y);

            if (_groundSensor.Grounded && (float)_jump > jumpThreshold)
            {
                _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }

            FlipX();
        }
    }

    public void Init(uint[] nnTopology)
    {
        if (_sensors == null)
        {
            _sensors = GetComponentsInChildren<Sensor>();
            _groundSensor = GetComponentInChildren<GroundSensor>();
        }

        InitNN(nnTopology);

        _individual = new GA.Individual(_nn.WeightsCount);

        CopytWeightsToIndividual();

        

        _aliveTimer = new Timer(aliveTime);
        _aliveTimer.Elapsed += OnTimedEvent;
        _aliveTimer.AutoReset = true;
        _aliveTimer.Enabled = true;
        _aliveTimer.Start();

        _initialized = true;
    }

    public bool Alive
    {
        get
        {
            return _alive;
        }
        set
        {
            if (_alive == value) {
                return;
            }
            _alive = value;
            if (_alive)
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    _sensors[i].Show();
                }
                _groundSensor.Show();
                _aliveTimer.Start();
            }
            else
            {
                for (int i = 0; i < _sensors.Length; i++)
                {
                    _sensors[i].Hide();
                }
                _groundSensor.Hide();
                _aliveTimer.Stop();
            }
        }
    }

    public GA.Individual Individual
    {
        get
        {
            return _individual;
        }
        set
        {
            _individual = value;
        }
    }

    public void FitWeights()
    {
        int individualPos = 0;
        for (int layer = 0; layer < _nn.LayersCount; layer++)
        {
            for (int row = 0; row < _nn[layer].Rows; row++)
            {
                for (int col = 0; col < _nn[layer].Cols; col++)
                {
                    _nn[layer][row][col] = _individual[individualPos++];
                }
            }
        }
    }

    private void OnTimedEvent(System.Object source, ElapsedEventArgs e)
    {
        Alive = false;
    }

}
