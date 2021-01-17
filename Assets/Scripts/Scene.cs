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

    private GameObject[] _playersObjects;
    private Player[] _players;
    private Camera _camera;
    private GA.Population _population;
    private GA.GeneticAlgorithm _ga;

    private bool _initialized = false;

    void Start()
    {
        _playersObjects = new GameObject[PopulationSize];
        _players = new Player[PopulationSize];
        for (int i = 0; i < _playersObjects.Length; i++)
        {
            _playersObjects[i] = Instantiate(PlayerPrefab);
            _players[i] = _playersObjects[i].GetComponent<Player>();
            _players[i].Init(NNTopology);
        }
        Reset();
        _camera = GetComponentInChildren<Camera>();
        _population = new GA.Population(PopulationSize, _players[0].nn.WeightsCount);
        _ga = new GA.GeneticAlgorithm(
            tournamentSize, swapChance, mutationChance, stddev, new System.Random());
        _initialized = true;
    }

    private void Reset()
    {
        for (int i = 0; i < _playersObjects.Length; i++)
        {
            if (_playersObjects[i])
            {
                _playersObjects[i].transform.position = new Vector3(-7.5f, -3f, -1f);
                _playersObjects[i].SetActive(true);
                _players[i].IsAlive = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (!_initialized)
            return;
        for (int i = 0; i < _playersObjects.Length; i++)
        {
            if (_playersObjects[i].transform.position.y < -5.0f)
            {
                _playersObjects[i].SetActive(false);
                _players[i].IsAlive = false;
            }
            else if (!_players[i].IsAlive)
            {
                _playersObjects[i].SetActive(false);
            }
        }
        bool reset = true;
        for (int i = 0; i < _playersObjects.Length; i++)
        {
            if (_players[i].IsAlive)
            {
                reset = false;
                break;
            }
        }
        var alivePlayers = _players.Where(p => p.IsAlive);
        if (alivePlayers.Count() > 0) {
            float bestX = alivePlayers.Max(p => p.transform.position.x);
            var best = alivePlayers.Where(p => p.transform.position.x == bestX).First();
            if (best)
            {
                _camera.transform.position = new Vector3(
                    best.transform.position.x,
                    best.transform.position.y,
                    _camera.transform.position.z);
            }
        }
        if (reset)
        {
            for (int p = 0; p < _players.Length; p++)
            {
                _population[p].Fitness = _players[p].transform.position.x;
                int pos = 0;
                for (int l = 0; l < _players[p].nn.LayersCount; l++)
                {
                    for (int r = 0; r < _players[p].nn[l].Rows; r++)
                    {
                        for (int c = 0; c < _players[p].nn[l].Cols; c++)
                        {
                            _population[p][pos++] = _players[p].nn[l][r][c];
                        }
                    }
                   
                }
            }
            _ga.Run(_population);
            for (int p = 0; p < _players.Length; p++)
            {
                int pos = 0;
                for (int l = 0; l < _players[p].nn.LayersCount; l++)
                {
                    for (int r = 0; r < _players[p].nn[l].Rows; r++)
                    {
                        for (int c = 0; c < _players[p].nn[l].Cols; c++)
                        {
                             _players[p].nn[l][r][c] = _population[p][pos++];
                        }
                    }

                }
            }
            Reset();
        }
        
    }


}
