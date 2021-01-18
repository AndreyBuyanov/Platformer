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

    private struct PlayerData
    {
        public GameObject playerObject;
        public Player player;
    }

    private PlayerData[] _players;
    
    private GA.GeneticAlgorithm _ga;

    private bool _initialized = false;

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

    private void ResetPlayers()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i].playerObject)
            {
                _players[i].playerObject.transform.position = new Vector3(-7.5f, -3f, -1f);
                _players[i].playerObject.SetActive(true);
                _players[i].player.Alive = true;
            }
        }
    }

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

    private void ProcessPlayers()
    {
        for (int i = 0; i < _players.Length; i++)
        {
            if (_players[i].playerObject.transform.position.y < -5.0f)
            {
                _players[i].playerObject.SetActive(false);
                _players[i].player.Alive = false;
            }
            else if (!_players[i].player.Alive)
            {
                _players[i].playerObject.SetActive(false);
            }
        }
    }

    private void ProcessCamera()
    {
        var alivePlayers = _players.Where(p => p.player.Alive);
        if (alivePlayers.Count() > 0)
        {
            float bestX = alivePlayers.Max(p => p.playerObject.transform.position.x);
            var best = alivePlayers.Where(p => p.playerObject.transform.position.x == bestX).First();
            _camera.transform.position = new Vector3(
                best.playerObject.transform.position.x,
                best.playerObject.transform.position.y,
                _camera.transform.position.z);
        }
    }

    void Start()
    {
        _camera = GetComponentInChildren<Camera>();

        InitPlayers();
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

        ProcessPlayers();

        ProcessCamera();

        if (!PopulationAlive())
        {
            for (int p = 0; p < _players.Length; p++)
            {
                _players[p].player.Individual.Fitness = _players[p].playerObject.transform.position.x;
            }

            List<GA.Individual> population = new List<GA.Individual>();
            for (int p = 0; p < _players.Length; p++)
            {
                population.Add(_players[p].player.Individual);
            }

            _ga.Run(population);

            for (int p = 0; p < _players.Length; p++)
            {
                _players[p].player.Individual = population[p];
                _players[p].player.FitWeights();
            }

            ResetPlayers();
        }

    }


}
