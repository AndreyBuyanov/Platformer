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
    // Сенсоры
    private Sensor[] _sensors;
    // Сенсор для определения того, что находимся на земле
    private GroundSensor _groundSensor;

    // Перемещение
    private double _move;
    // Прыжок
    private double _jump;
    
    // Разворот влево
    private bool _facingLeft = false;

    // Жив ли игрок
    private bool _alive = false;
    // Таймер "жизни" игрока
    private Timer _aliveTimer;

    private bool _initialized = false;

    private GA.Individual _individual;
    private NN.NeuralNetwork _nn;

    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    // Получаем значения сенсоров
    private double[] GetSensorsOutput()
    {
        // Массив с результатами
        double[] sensorsOutput = new double[_sensors.Length];
        for (int i = 0; i < _sensors.Length; i++)
        {
            // Запрашиваем значение сенсора
            sensorsOutput[i] = _sensors[i].Output;
        }
        return sensorsOutput;
    }

    // Обновляем значения переменных управления
    // Подаём значения сенсоров
    private void UpdateControlValues(double[] sensorsOutput)
    {
        // Прогоняем значения сенсоров через нейронную сеть
        Math.Vector controls = _nn.Forward(new Math.Vector(sensorsOutput));
        // Обновляем значения переменных управления
        _move = (controls[0] * 2 - 1) * speed;
        _jump = controls[1] * 2 - 1;
    }

    // Переворот игрока влево\вправо
    private void FlipX()
    {
        // Получаем вектор масштабирования игрока
        Vector3 localScale = transform.localScale;
        // Если значение перемещения отрицательное,
        // то двигаемся назад, те переворачиваем игрока (по оси х)
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

    // Создание и инициализация нейронной сети
    // На входе массив с количеством нейронов в скрытых слоях
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

    // Копирование весов в особь
    private void CopytWeightsToIndividual()
    {
        int individualPos = 0;
        // Проходим по всем слоям, строкам, столбцам и добавляем веса в особъ
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
            // 1. Получаем значения сенсоров
            double[] sensorsOutput = GetSensorsOutput();
            // 2. Прогоняем значения сенсоров через нейронную сеть
            // и обновляем значения управляющих переменных
            UpdateControlValues(sensorsOutput);

            // 3. Перемещаем персонажа
            _body.velocity = new Vector2((float)_move, _body.velocity.y);
            // 3.1. Прыгаем, если нужно
            if (_groundSensor.Grounded && (float)_jump > jumpThreshold)
            {
                _body.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
            }
            // 4. Разворачиваем персонажа, если нужно
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

        // Инициализируем первое поколение
        InitNN(nnTopology);
        // Создаём особь
        _individual = new GA.Individual(_nn.WeightsCount);
        // Копируем веса в особь
        CopytWeightsToIndividual();
        // Настраиваем таймер, отвечающий за жизнь персонажа
        _aliveTimer = new Timer(aliveTime);
        _aliveTimer.Elapsed += OnTimedEvent;
        _aliveTimer.AutoReset = true;
        _aliveTimer.Enabled = true;
        _aliveTimer.Start();

        _initialized = true;
    }

    // Жив ли персонаж
    public bool Alive
    {
        get
        {
            return _alive;
        }
        set
        {
            // Значение не поменялось, просто выходим
            if (_alive == value) {
                return;
            }
            _alive = value;
            if (_alive)
            {
                // Персонаж жив
                for (int i = 0; i < _sensors.Length; i++)
                {
                    // Показываем сенсоры
                    _sensors[i].Show();
                }
                _groundSensor.Show();
                // Запускаем таймер
                _aliveTimer.Start();
            }
            else
            {
                // Персонаж умер
                for (int i = 0; i < _sensors.Length; i++)
                {
                    // Скрываем сенсоры
                    _sensors[i].Hide();
                }
                _groundSensor.Hide();
                // Останавливаем таймер
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

    // Копируем гены из особи в веса нейронной сети
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
        // Время жизни вышло, убиваем персонажа
        Alive = false;
    }

}
