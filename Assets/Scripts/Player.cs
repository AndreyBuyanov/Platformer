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

    private Rigidbody2D _body;
    private Sensor[] _sensors;
    private GroundSensor _groundSensor;
    private double _move;
    private double _jump;
    private System.Random _random = new System.Random();
    private bool _facingLeft = false;
    private bool _isAlive = false;
    private Timer _aliveTimer;

    private bool _initialized = false;

    public NN.NeuralNetwork nn;

    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (_initialized)
        {
            double[] sensorOutput = new double[_sensors.Length];
            for (int i = 0; i < _sensors.Length; i++)
            {
                sensorOutput[i] = _sensors[i].Output;
            }
            Math.Vector controls = nn.Forward(new Math.Vector(sensorOutput));
            _move = (controls[0] * 2 - 1) * speed;
            _jump = controls[1] * 2 - 1;

            _body.velocity = new Vector2((float)_move, _body.velocity.y);

            if (_groundSensor.Grounded && (float)_jump > jumpThreshold)
            {
                _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }

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
    }

    public void Init(uint[] nnTopology)
    {
        if (_sensors == null)
        {
            _sensors = GetComponentsInChildren<Sensor>();
            _groundSensor = GetComponentInChildren<GroundSensor>();
        }
        NN.LayerConfig[] nnConfig = new NN.LayerConfig[nnTopology.Length + 1];
        for (int i = 0; i < nnTopology.Length; i++)
        {
            nnConfig[i].neurons = nnTopology[i];
            nnConfig[i].fn = NN.ActivationFunction.Sigmoid;
        }
        nnConfig[nnConfig.Length - 1].neurons = 2;
        nnConfig[nnConfig.Length - 1].fn = NN.ActivationFunction.Sigmoid;
        nn = new NN.NeuralNetwork((uint)_sensors.Length, nnConfig);
        nn.SetRandomWeights(-1.0, 1.0, _random);

        _aliveTimer = new Timer(aliveTime);
        _aliveTimer.Elapsed += OnTimedEvent;
        _aliveTimer.AutoReset = true;
        _aliveTimer.Enabled = true;
        _aliveTimer.Start();

        _initialized = true;
    }

    public bool IsAlive
    {
        get
        {
            return _isAlive;
        }
        set
        {
            if (_isAlive == value) {
                return;
            }
            _isAlive = value;
            if (_isAlive)
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

    private void OnTimedEvent(System.Object source, ElapsedEventArgs e)
    {
        IsAlive = false;
    }

}
