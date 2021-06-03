using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private GameObject _enemyPrefab, _explosionPrefab;
    [SerializeField]
    private GameObject[] _powerupPrefabs;
    [SerializeField]
    private Transform _enemyPool, _enemyContainer;
    [SerializeField]
    private int _enemyCountLimit;

    [SerializeField]
    [Tooltip("Number of Enemies in each Wave. 0 = Boss Wave")]
    private int[] _waveCounts;
    private int _spawnedEnemiesInWave;
    private int _currentWave = 0;

    private bool _spawning = false;

    private static SpawnManager _instance;
    public static SpawnManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("Spawn Manager is Null!!!");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += PlayerDeath;
        Enemy.OnEnemyDeath += EnemyDeath;
    }

    void Start()
    {
        
    }

    public void StartSpawning()
    {
        if (!_spawning)
        {
            StartCoroutine(WaveStart());
            StartCoroutine(PowerUpSpawnRoutine());
            //_spawning = true;
        }
    }

    private void PlayerDeath()
    {
        _spawning = false;
    }

    private void EnemyDeath(int notUsed)
    {
        if (_spawnedEnemiesInWave >= _waveCounts[_currentWave] && _enemyContainer.childCount == 0)
            StartCoroutine(WaveStart());
    }

    IEnumerator WaveStart()
    {
        if (_currentWave < _waveCounts.Length)
        {
            UIManager.Instance.UpdateWaveDisplay(_currentWave + 1);
            _spawnedEnemiesInWave = 0;
            _spawning = true;
            yield return new WaitForSeconds(5);
            StartCoroutine(EnemySpawnRoutine());
        }
        //else
            //Display Winner's Game Over

    }
    IEnumerator PowerUpSpawnRoutine()
    {
        yield return new WaitForSeconds(5f);
        while (_spawning)
        {
            float RNGTime = Random.Range(3f, 5f);
            yield return new WaitForSeconds(RNGTime);
            Vector3 launch;
            if (!GameManager.Instance.IsHorizontalFlight())
            {
                float RNG = Random.Range(-20f, 20f);
                launch = new Vector3(RNG, 0, 20 + RNGTime);
            }
            else
            {
                float RNG = Random.Range(-3.75f, 5.5f);
                launch = new Vector3(0, RNG, 15);
            }
            SpawnPowerUP(launch);
        }
    }

    private void SpawnPowerUP(Vector3 LaunchPOS)
    {        
        int RNGItem = Random.Range(0, _powerupPrefabs.Length);
        Instantiate(_powerupPrefabs[RNGItem], LaunchPOS, Quaternion.identity);
    }

    IEnumerator EnemySpawnRoutine()
    {
        while (_spawning)
        {
            while (_currentWave < _waveCounts.Length && _spawnedEnemiesInWave < _waveCounts[_currentWave])
            {
                Vector3 launch;
                if (!GameManager.Instance.IsHorizontalFlight())
                {
                    float RNG = Random.Range(-20f, 20f);
                    launch = new Vector3(RNG, 0, 20);
                }
                else
                {
                    float RNG = Random.Range(-3.75f, 5.5f);
                    launch = new Vector3(0, RNG, 15);
                }
                SpawnEnemy(launch);
                yield return new WaitForSeconds(5);
            }
            if (_enemyContainer.childCount == 0)
            {
                _spawning = false;
                yield return new WaitForEndOfFrame();

                if (_currentWave <= _waveCounts.Length-1)
                    _currentWave++;

                StartCoroutine(WaveStart());
            }
            yield return new WaitForEndOfFrame();
        }

    }

    private void SpawnEnemy(Vector3 LaunchPOS)
    {
        if (_enemyPool.childCount < 1)
        {
            if(_enemyContainer.childCount < _enemyCountLimit)
                Instantiate(_enemyPrefab, LaunchPOS, Quaternion.Euler(0, 180, 0), _enemyContainer);
        }
        else
        {
            GameObject Enemy = _enemyPool.GetChild(0).gameObject;
            Enemy.transform.position = LaunchPOS;
            Enemy.transform.parent = _enemyContainer;
            Enemy.SetActive(true);
        }
        _spawnedEnemiesInWave++;
    }

    public void SpawnExplosion(Vector3 BoomPOS)
    {
        Instantiate(_explosionPrefab, BoomPOS + new Vector3(0, 2, 0), Quaternion.identity);
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= PlayerDeath;
        Enemy.OnEnemyDeath -= EnemyDeath;
    }
}
