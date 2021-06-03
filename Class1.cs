using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    [Header("Enemy Variables")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private GameObject _enemyContainer;

    // New Enemy Type
    [SerializeField] private GameObject _enemy2Prefab;
    public bool _enemy2Exists = false;

    [Header("Powerup Variables")]
    [SerializeField] private GameObject[] powerups;

    [Header("General Spawn Manager")]
    private bool _stopSpawning = false;



    public void StartSpawning()
    {
        StartCoroutine(SpawnEnemyRoutine());
        StartCoroutine(SpawnPowerupRoutine());
        StartCoroutine(SpawnEnemy2Routine());
    }

    IEnumerator SpawnEnemy2Routine()
    {
        yield return new WaitForSeconds(5.0f);
        while (_stopSpawning == false && _enemy2Exists == false)
        {
            Vector3 spawnPosition = new Vector3(-7, 2, 0);
            //GameObject newEnemy2 = 
            Instantiate(_enemy2Prefab, spawnPosition, Quaternion.identity);
            //newEnemy2.transform.parent = _enemyContainer.transform;
            _enemy2Exists = true;
            yield return new WaitForSeconds(2.0f);

        }

    }


    IEnumerator SpawnEnemyRoutine()
    {
        yield return new WaitForSeconds(8.0f);
        while (_stopSpawning == false)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-8f, 8f), 7, 0);
            GameObject newEnemy = Instantiate(_enemyPrefab, spawnPosition, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
            yield return new WaitForSeconds(10.0f);
        }

    }

    IEnumerator SpawnPowerupRoutine()
    {
        yield return new WaitForSeconds(10.0f);
        while (_stopSpawning == false)
        {
            Vector3 spawnPosition = new Vector3(Random.Range(-8f, 8f), 7, 0);
            int randomPowerUp = Random.Range(0, 7); // if int the last number never gets called  
            Instantiate(powerups[randomPowerUp], spawnPosition, Quaternion.identity);
            yield return new WaitForSeconds(Random.Range(3.0f, 10.0f));
        }
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }

    public void Enemy2Exists()
    {
        _enemy2Exists = true;
    }

    public void Enemy2Dead()
    {
        Debug.LogError("Bool Triggered.");
        _enemy2Exists = false;
    }



}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
 
public class Enemy2 : MonoBehaviour
{
    [SerializeField] private float _enemy2Speed = 5.0f;
    private Player _player;
    private Enemy _enemy1;
    private Animator _anim;
    private AudioSource _audioSource;
    private SpawnManager _spawnManager;
    [SerializeField] private GameObject _minePrefab; //enemy fire
    private float _fireRateEnemy = 5.0f; //enemy fire
    private float _canFire = -1; //enemy fire


    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        _audioSource = GetComponent<AudioSource>();
        _enemy1 = GameObject.Find("Enemy").GetComponent<Enemy>();
        _spawnManager = GameObject.Find("Spawn_Manager").GetComponent<SpawnManager>();

        if (_player == null)
        {
            Debug.LogError("The player is NULL.");
        }
        _anim = GetComponent<Animator>();
        if (_anim == null)
        {
            Debug.LogError("Animator is Null.");
        }

        if (_spawnManager == null)
        {
            Debug.LogError("The Spawn Manager is NULL:");
        }
    }

    void Update()
    {
        CalculateMovement();

        /* if (Time.time > _canFire)
         {
             _fireRateEnemy = Random.Range(8f, 10f);
             _canFire = Time.time + _fireRateEnemy;
             GameObject enemyLaser = Instantiate(_minePrefab, transform.position + new Vector3(0, -0.4f, 0), Quaternion.identity);
             LaserScript[] lasers = enemyLaser.GetComponentsInChildren<LaserScript>();
 
             for (int i = 0; i < lasers.Length; i++) //review
             {
                 lasers[i].AssignEnemyLaser();
             }
         } */
    }

    void CalculateMovement()
    {

        transform.Translate(Vector3.right * _enemy2Speed * Time.deltaTime);

        if (transform.position.x > 11f)
        {
            transform.position = new Vector3(-11, 2, 0);
        }

    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Player")
        {
            Player player = other.transform.GetComponent<Player>();

            if (player != null)
            {
                player.Damage();
            }

            _anim.SetTrigger("OnEnemyDeath");
            _enemy2Speed = 0;
            _audioSource.Play();
            Destroy(this.gameObject, 1.0f);

        }

        if (other.tag == "Laser")
        {
            Destroy(other.gameObject);

            if (_player != null)
            {
                _player.ScorePlus(10);
            }
            _anim.SetTrigger("OnEnemyDeath");
            _audioSource.Play();
            
            _spawnManager.Enemy2Dead();  ////Doesn't work
            Destroy(this.gameObject);


            //Destroy(GetComponent<Collider2D>());


        }


        if (other.tag == "Mine")
        {
            Destroy(other.gameObject);
            if (_player != null)
            {
                _player.ScorePlus(10);
            }
            _anim.SetTrigger("OnEnemyDeath");
            _enemy2Speed = 0;
            _audioSource.Play();
            Destroy(GetComponent<Collider2D>());
            Destroy(this.gameObject, 2.8f);

        }

        if (other.tag == "Firewall")
        {
            _anim.SetTrigger("OnEnemyDeath");
            _enemy2Speed = 0;
            _audioSource.Play();
            Destroy(this.gameObject, 2.8f);
        }

    }
}