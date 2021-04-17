using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [SerializeField]
    private bool _horizontalFlight;
    [SerializeField]
    private GameObject _enemyPrefab, _explosionPrefab;
    [SerializeField]
    private GameObject[] _powerupPrefabs;
    [SerializeField]
    private Transform _enemyPool, _enemyContainer;
    [SerializeField]
    private int _enemyCountLimit;

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
    }

    void Start()
    {
        
    }

    public void StartSpawning()
    {
        if (!_spawning)
        {
            StartCoroutine(EnemySpawnRoutine());
            StartCoroutine(PowerUpSpawnRoutine());
            _spawning = true;
        }
    }

    public void PlayerDeath()
    {
        _spawning = false;
    }

    IEnumerator PowerUpSpawnRoutine()
    {
        yield return new WaitForSeconds(5f);
        while(_spawning)
        {
            float RNGTime = Random.Range(3f, 5f);
            yield return new WaitForSeconds(RNGTime);
            Vector3 launch;
            if (!_horizontalFlight)
            {
                float RNG = Random.Range(-20f, 20f);
                launch = new Vector3(RNG, 0, 20+RNGTime);
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
        yield return new WaitForSeconds(3f);
        while (_spawning )
        {
            Vector3 launch;
            if (!_horizontalFlight)
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
    }

    public void SpawnExplosion(Vector3 BoomPOS)
    {
        Instantiate(_explosionPrefab, BoomPOS + new Vector3(0, 2, 0), Quaternion.identity);
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= PlayerDeath;   
    }
}
