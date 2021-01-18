using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{
    [SerializeField]
    private GameObject PlayerPrefab;
    [SerializeField]
    private uint PopulationSize;
    [SerializeField]
    private uint[] NNTopology;
    [SerializeField]
    private int tournamentSize = 3;
    [SerializeField]
    private double swapChance = 0.8;
    [SerializeField]
    private double mutationChance = 0.8;
    [SerializeField]
    private double stddev = 0.1;

    private Camera _camera;

    // Данные игрока
    private struct PlayerData
    {
        // Игрок, как объект движка
        public GameObject playerObject;
        // Игрок, как объект из скрипта
        public Player player;
    }

    // Игроки
    private PlayerData[] _players;
    
    private GA.GeneticAlgorithm _ga;

    private bool _initialized = false;

    // Создаём и инициализируем персонажей
    private void InitPlayers()
    {
        _players = new PlayerData[PopulationSize];
        for (int i = 0; i < _players.Length; i++)
        {
            _players[i].playerObject = Instantiate(PlayerPrefab);
            _players[i].player = _players[i].playerObject.GetComponent<Player>();
            _players[i].player.Init(NNTopology);
        }
    }

    // Сброс персонажей
    private void ResetPlayers()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i].playerObject)
            {
                // Игрок появляется в начальной точке
                _players[i].playerObject.transform.position = new Vector3(-7.5f, -3f, -1f);
                _players[i].playerObject.SetActive(true);
                _players[i].player.Alive = true;
            }
        }
    }

    // Есть ли живие персонажи
    private bool PopulationAlive()
    {
        bool alive = false;
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i].player.Alive)
            {
                alive = true;
                break;
            }
        }
        return alive;
    }

    // Обработка персонажей
    private void ProcessPlayers()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            // Если игрок провалился в пропасть
            if (_players[i].playerObject.transform.position.y < -5.0f)
            {
                // убиваем игрока
                _players[i].playerObject.SetActive(false);
                _players[i].player.Alive = false;
            }
            // Если же игрок умер по таймауту
            else if (!_players[i].player.Alive)
            {
                // скрываем игрока ("убиваем")
                _players[i].playerObject.SetActive(false);
            }
        }
    }

    // Обработка камеры
    private void ProcessCamera()
    {
        // Камера следит за игроком, который продвинулся дальше всех по горизонтали
        // Выбираем живых игроков
        var alivePlayers = _players.Where(p => p.player.Alive);
        // Если есть живые игроки
        if (alivePlayers.Count() > 0)
        {
            // Получаем х-координату наиболее продвинувшегося игрока
            float bestX = alivePlayers.Max(p => p.playerObject.transform.position.x);
            // Выбираем первого наиболее продвинувшегося игрока
            var best = alivePlayers.Where(p => p.playerObject.transform.position.x == bestX).First();
            // Перемещаем камеру по х и у к игроку
            _camera.transform.position = new Vector3(
                best.playerObject.transform.position.x,
                best.playerObject.transform.position.y,
                _camera.transform.position.z);
        }
    }

    void Start()
    {
        // Получаем камеру
        _camera = GetComponentInChildren<Camera>();

        // Инизиализируем первое поколение
        InitPlayers();
        // Перемещаем игроков к исходной точке
        ResetPlayers();

        _ga = new GA.GeneticAlgorithm(
            tournamentSize, swapChance, mutationChance, stddev, new System.Random());

        _initialized = true;
    }

    void FixedUpdate()
    {
        if (!_initialized)
        {
            return;
        }

        // Обрабатываем игроков
        ProcessPlayers();
        // Обрабатываем камеру
        ProcessCamera();

        // Если вся популяция погибла (упала в пропасть или вышло время жизни)
        if (!PopulationAlive())
        {
            // Получаем значение приспособленности особей как величину продвижения по оси х
            for (int p = 0; p < _players.Length; p++)
            {
                _players[p].player.Individual.Fitness = _players[p].playerObject.transform.position.x;
            }
            // Создаём популяцию
            List<GA.Individual> population = new List<GA.Individual>();
            for (int p = 0; p < _players.Length; p++)
            {
                population.Add(_players[p].player.Individual);
            }
            // Запускаем генетический алгоритм
            _ga.Run(population);
            // В популяции новые значения после генетического алгоритма (новая популяция)
            // Обновляем веса нейронных сетей игроков
            for (int p = 0; p < _players.Length; p++)
            {
                _players[p].player.Individual = population[p];
                _players[p].player.FitWeights();
            }
            // Сброс и перемещение игроков в исходную точку
            ResetPlayers();
        }
    }

}
