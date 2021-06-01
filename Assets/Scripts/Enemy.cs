using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{

    [SerializeField]
    private bool _horizontalFlight;
    [SerializeField]
    private float _speed;
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private int _pointValue;
    private bool _respawning = true;
    private Transform _pool;
    private Transform _container;
    private bool _isExploding = false;
    private Transform _laserPool;
    [SerializeField]
    private Transform _laserOffset;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private bool _laserCanFire = true;
    [SerializeField]
    private Vector2 _laserCoolDown = new Vector2(3, 7);
    private GameObject _target;

    public delegate void EnemyDeath(int pointValue);
    public static event EnemyDeath OnEnemyDeath;

    private void OnEnable()
    {
        Player.OnPlayerDeath += PlayerDeath;
    }

    void Start()
    {
        if (_pool == null)
            _pool = GameObject.Find("Enemy_Pool").transform;
        if (_container == null)
            _container = GameObject.Find("Enemy_Container").transform;
        if (_laserPool == null)
            _laserPool = GameObject.Find("Laser_Pool").transform;
    }

    private void Awake()
    {
        int RNG = Random.Range(0, 10);
        if (RNG >= 0)
            _target = GameObject.FindGameObjectWithTag("Player");
        else
            _target = null;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Weapon();
        Respawn();
    }

    private void Movement()
    {
        if (!_isExploding)
            transform.Translate(Vector3.forward * _speed * Time.deltaTime, Space.Self);
    }

    private void Weapon()
    {
        if (_laserCanFire)
        {
            GameObject laser;
            Vector3 launch = _laserOffset.position;
            if (_laserPool.childCount < 1)
            {
                laser = Instantiate(_laserPrefab, launch, transform.rotation, transform);
                laser.transform.localScale = _laserOffset.localScale;
            }
            else
                PullLaserFromPool(launch, _laserOffset);


            _laserCanFire = false;
            StartCoroutine(LaserReloadTimer());
            PlaySFX(2);
        }
    }

    private void PullLaserFromPool(Vector3 LaunchPOS, Transform Offset)
    {
        GameObject laserTemp = _laserPool.GetChild(0).gameObject;
        laserTemp.transform.parent = this.transform;
        laserTemp.transform.position = LaunchPOS;
        laserTemp.transform.rotation = Offset.rotation;
        laserTemp.transform.localScale = Offset.localScale;
        laserTemp.GetComponent<Laser>().SetLastOwner(this.transform);
        laserTemp.SetActive(true);
    }

    IEnumerator LaserReloadTimer()
    {
        float RNG = Random.Range(_laserCoolDown.x, _laserCoolDown.y);
        yield return new WaitForSeconds(RNG);
        _laserCanFire = true;
    }

    private void Respawn()
    {
        if (!_horizontalFlight && transform.position.z <= -17)
            if (_respawning)
            {
                float RNG = Random.Range(-20f, 20f);
                transform.position = new Vector3(RNG, 0, 20);
                if (_target != null)
                    transform.LookAt(_target.transform.position);
            }
            else
                StartCoroutine(SendToPool());

        else if (_horizontalFlight && transform.position.z <= -15)
            if (_respawning)
            {
                float RNG = Random.Range(-3.75f, 5.5f);
                transform.position = new Vector3(0, RNG, 15);
            }
            else
                StartCoroutine(SendToPool());
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Laser"))
        {
            if (other.transform.parent.TryGetComponent(out Laser laser))
            {
                if (laser.ReportLastOwner().CompareTag("Player"))
                {
                    laser.SendToPool();

                    SpawnManager.Instance.SpawnExplosion(transform.position);
                    PlaySFX(1);
                    StartCoroutine(SendToPool());
                    if (OnEnemyDeath != null)
                        OnEnemyDeath(_pointValue);
                }
            }
        }
        else if (other.CompareTag("Player"))
        {
            if (other.TryGetComponent(out Player player))
                player.Damage();

            SpawnManager.Instance.SpawnExplosion(transform.position);
            PlaySFX(1);
            StartCoroutine(SendToPool());
            if (OnEnemyDeath != null)
                OnEnemyDeath(_pointValue / 2);
        }
        else if (other.CompareTag("Enemy") && transform.position.z >= 15)
        {
            if (!_horizontalFlight)
                if (_respawning)
                    transform.position = new Vector3(Random.Range(-20f, 20f), 0, Random.Range(20f, 23f));
                else
                    StartCoroutine(SendToPool());
            else if (_horizontalFlight)
                if (_respawning)
                    transform.position = new Vector3(0, Random.Range(-3.75f, 5.5f), Random.Range(15f, 18f));
                else
                    StartCoroutine(SendToPool());
        }
    }

    private void PlaySFX(int SFXGroup)
    {
        AudioManager.Instance.PlaySFX(SFXGroup);
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

    public IEnumerator SendToPool()
    {
        _isExploding = true;
        _collider.enabled = false;
        yield return new WaitForSeconds(.33f);
        _isExploding = false;
        _collider.enabled = true;
        transform.parent = _pool;
    }
}
