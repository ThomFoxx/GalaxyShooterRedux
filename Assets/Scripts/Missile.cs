using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8.5f;
    [SerializeField]
    private float _enemySpeedMultiplier = 1.5f;
    private Transform _lastOwner;
    [SerializeField]
    private bool _isTracking = false;
    [SerializeField]
    private Transform _target;

    public delegate void MissileDeath(Transform self);
    public static event MissileDeath OnMissileDeath;

    private void OnEnable()
    {
        Player.OnPlayerDeath += PlayerDeath;
        Enemy.OnEnemyDeath += EnemyDeath;
        Enemy.OnEnemyRespawn += EnemyRespawn;
        _isTracking = false;
        _target = null;
    }

    void Start()
    {
        if (_lastOwner == null)
            SetLastOwner(PoolManager.Instance.transform);
    }

    void Update()
    {
        if (!_lastOwner.gameObject.activeSelf)
            ResetTarget();

        if (transform.gameObject.activeSelf)
            if (_lastOwner != null)
                Movement();
            else
            {
                Debug.Log("Sending Missile to Pool Due to No Owner");
                SendToPool();
            }
    }

    private void Movement()
    {
        if (_isTracking)
            transform.LookAt(_target);

        if (_lastOwner.CompareTag("Enemy"))
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed / _enemySpeedMultiplier * Time.deltaTime, Space.World);
        else
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed * Time.deltaTime, Space.World);

        if (transform.position.x > 20 | transform.position.x < -20)
        {
            Debug.Log("Sending Missile to Pool due to X");
            SendToPool();
        }
        if (transform.position.z > 25 | transform.position.z < -25 )
        {
            Debug.Log("Sending Missile to Pool due to Z");
            SendToPool();
        }
    }

    public void SetTarget(Transform target)
    {
        _target = target;
        _isTracking = true;
    }

    public void ResetTarget()
    {
        _target = null;
        _isTracking = false;
        _lastOwner = PoolManager.Instance.transform;
    }

    public void SetLastOwner(Transform owner)
    {
        _lastOwner = owner;
    }

    public Transform ReportLastOwner()
    {
        return _lastOwner;
    }

    public void SendToPool()
    {
        if (OnMissileDeath != null)
        {
            Debug.Log("Sending Destruction Confirmation");
            OnMissileDeath(this.transform);
        }

        _isTracking = false;
        _target = null;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;        
        transform.gameObject.SetActive(false);
        SetLastOwner(PoolManager.Instance.transform);
    }

    private void PlaySFX(int SFXGroup)
    {
        AudioManager.Instance.PlaySFX(SFXGroup);
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Laser":
                SpawnManager.Instance.SpawnExplosion(transform.position);
                PlaySFX(1);
                Debug.Log("Sending Missile to Pool Due to Laser Impact");
                SendToPool();
                break;
            case "Player":
                if (_lastOwner != other.transform)
                {
                    if (other.TryGetComponent(out Player player))
                        player.Damage();

                    SpawnManager.Instance.SpawnExplosion(transform.position);
                    PlaySFX(1);
                    Debug.Log("Sending Missile to Pool Due to Player Impact");
                    SendToPool();
                }
                break;
            case "Enemy":
                if (!_lastOwner.CompareTag("Enemy"))
                {
                    SpawnManager.Instance.SpawnExplosion(transform.position);
                    PlaySFX(1);
                    Debug.Log("Sending Missile to Pool Due to Enemy Impact(Player)");
                    SendToPool();
                }
                else if (!other.TryGetComponent(out Teleporter Telelport))
                {
                    Debug.Log("Sending Missile to Pool Due to Enemy Impact(Teleporter)");
                    SpawnManager.Instance.SpawnExplosion(transform.position);
                    PlaySFX(1);
                    SendToPool();
                }
                break;
            default:
                break;
        }
    }

    private void PlayerDeath()
    {
        if (transform.gameObject.activeSelf)
        {
            Debug.Log("Sending Missile to Pool Due to Player Death");
            SendToPool();
        }
    }

    private void EnemyDeath(int notUsed, Transform enemy)
    {
        if (transform.gameObject.activeSelf && enemy == _target)
        {
            _isTracking = false;
            _target = null;
        }
    }

    private void EnemyRespawn(Transform enemy)
    {
        if (transform.gameObject.activeSelf && enemy == _target)
        {
            _isTracking = false;
            _target = null;
        }
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= PlayerDeath;
        Enemy.OnEnemyDeath -= EnemyDeath;
        Enemy.OnEnemyRespawn -= EnemyRespawn;
    }
}
