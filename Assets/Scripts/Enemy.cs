using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    private bool _horizontalFlight;
    [SerializeField]
    private float _speed;
    private bool _respawning = true;
    private Transform _pool;
    private Transform _container;


    private void OnEnable()
    {
        Player.OnPlayerDeath += PlayerDeath;
    }

    void Start()
    {
        if (_pool == null)
            _pool = GameObject.Find("EnemyPool").transform;
        if (_container == null)
            _container = GameObject.Find("EnemyContainer").transform;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();

        Respawn();
    }

    private void Movement()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime, Space.Self);
    }

    private void Respawn()
    {
        if (!_horizontalFlight && transform.position.z <= -10)
            if (_respawning)
            {
                float RNG = Random.Range(-8.5f, 8.5f);
                transform.position = new Vector3(RNG, 0, 8);
            }
            else
                SendToPool();
        else if (_horizontalFlight && transform.position.z <= -15)
            if (_respawning)
            {
                float RNG = Random.Range(-3.75f, 5.5f);
                transform.position = new Vector3(0, RNG, 15);
            }
            else
                SendToPool();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Laser"))
        {
            if (other.transform.parent.TryGetComponent(out Laser laser))
                laser.SendToPool();

            SendToPool();
        }
        else if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out Player player))
                player.Damage();

            Debug.Log("Hit Player");
            SendToPool();
        }
        else if (other.CompareTag("Enemy"))
        {
            if (!_horizontalFlight)
                if (_respawning)
                    transform.position = new Vector3(Random.Range(-8.5f, 8.5f), 0, Random.Range(7f,10f));
                else
                    SendToPool();
            else if (_horizontalFlight)
                if (_respawning)
                    transform.position = new Vector3(0, Random.Range(-3.75f, 5.5f), Random.Range(15f,18f));
                else
                    SendToPool();
        }
       
            Debug.Log(other.tag);
    }

    private void PlayerDeath()
    {
        _respawning = false;
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= PlayerDeath;
    }

    private void OnTransformParentChanged()
    {
        if (transform.parent == _pool)
        {
            transform.localPosition = Vector3.zero;
            transform.gameObject.SetActive(false);
        }
    }

    public void SendToPool()
    {
        transform.parent = _pool;
    }
}
