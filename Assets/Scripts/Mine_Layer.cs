using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine_Layer : MonoBehaviour
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
    private bool _isExploding = false;
    [SerializeField]
    private Transform _laserOffset;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private Transform[] _mineOffeset;
    [SerializeField]
    private GameObject _minePrefab;
    [SerializeField]
    private bool _laserCanFire = true;
    [SerializeField]
    private Vector2 _laserCoolDown = new Vector2(.5f, 3);
    private GameObject _target;
    [SerializeField]
    private int _maxMineCount = 3;
    private int _mineCount;
    private bool _mineReady = true;

    public delegate void EnemyDeath(int pointValue, Transform self);
    public static event EnemyDeath OnEnemyDeath;
    public delegate void EnemyRespawn(Transform self);
    public static event EnemyRespawn OnEnemyRespawn;


    private void Awake()
    {            
        _target = GameObject.FindGameObjectWithTag("Player");        
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += PlayerDeath;
        //StartCoroutine(LaserReloadTimer());
    }

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
            GameObject GO = PoolManager.Instance.RequestFromPool(_laserPrefab);
            SetupPoolObject(GO, _laserOffset);

            _laserCanFire = false;
            StartCoroutine(LaserReloadTimer());
            PlaySFX(2);
        }
        if (_target != null)
            if (_mineCount > 0 && MiningPosition() && _mineReady)
            {
                StartCoroutine(ReleaseMines());
            }
    }

    private bool MiningPosition()
    {
        if (transform.position.z < _target.transform.position.z)
            return true;

        return false;
    }

    private void ReloadMines()
    {
        _mineCount = Random.Range(1, _maxMineCount + 1);
    }

    IEnumerator ReleaseMines()
    {
        _mineReady = false;
        float RNG = Random.Range(0.25f, 2);
        yield return new WaitForSeconds(RNG/_mineCount);

        while (_mineCount > 0)
        {
            for (int i = 0; i < _mineCount; i++)
            {
                yield return new WaitForSeconds(.01f);
                for (int x = 0; x < 2; x++)
                {
                    GameObject Mine = PoolManager.Instance.RequestFromPool(_minePrefab);
                    SetupPoolObject(Mine, _mineOffeset[x]);
                    StartCoroutine(Mine.GetComponent<Mine>().Slowdown(_mineCount));
                }
                _mineCount--;
            }
        }
        _mineReady = true;
    }

    private void SetupPoolObject(GameObject Obj, Transform Offset)
    {
        Obj.transform.position = Offset.position;
        Obj.transform.rotation = Offset.rotation;
        Obj.transform.localScale = Offset.localScale;
        Obj.SetActive(true);
    }

    IEnumerator LaserReloadTimer()
    {
        float RNG = Random.Range(_laserCoolDown.x, _laserCoolDown.y);
        yield return new WaitForSeconds(RNG);
        _laserCanFire = true;
    }

    private void Respawn()
    {
        if (!GameManager.Instance.IsHorizontalFlight() && transform.position.z <= -17)
            if (_respawning)
            {
                float RNG = Random.Range(-20f, 20f);
                transform.position = new Vector3(RNG, 0, 20);
                ReloadMines();
                if (OnEnemyRespawn != null)
                    OnEnemyRespawn(this.transform);
            }
            else
                StartCoroutine(SendToPool());

        else if (GameManager.Instance.IsHorizontalFlight() && transform.position.z <= -15)
            if (_respawning)
            {
                float RNG = Random.Range(-3.75f, 5.5f);
                transform.position = new Vector3(0, RNG, 15);
                ReloadMines();
            }
            else
                StartCoroutine(SendToPool());
    }

    private void OnTriggerEnter(Collider other)
    {
        switch (other.tag)
        {
            case "Laser":
                if (other.TryGetComponent(out Laser laser))
                {
                    if (laser.ReportLastOwner().CompareTag("Player"))
                    {
                        laser.SendToPool();

                        SpawnManager.Instance.SpawnExplosion(transform.position);
                        PlaySFX(1);
                        StartCoroutine(SendToPool());
                        if (OnEnemyDeath != null)
                            OnEnemyDeath(_pointValue, this.transform);

                        SpawnManager.Instance.CountEnemyDeath();
                    }
                }
                else if (other.TryGetComponent(out Missile missile))
                {
                    Debug.Log("Sending Missile to Pool Due to Enemy Impact");
                    missile.SendToPool();

                    SpawnManager.Instance.SpawnExplosion(transform.position);
                    PlaySFX(1);
                    StartCoroutine(SendToPool());
                    if (OnEnemyDeath != null)
                        OnEnemyDeath(_pointValue, this.transform);

                    SpawnManager.Instance.CountEnemyDeath();
                }
                break;
            case "Player":
                if (other.TryGetComponent(out Player player))
                    player.Damage();

                SpawnManager.Instance.SpawnExplosion(transform.position);
                PlaySFX(1);
                StartCoroutine(SendToPool());
                if (OnEnemyDeath != null)
                    OnEnemyDeath(_pointValue / 2, this.transform);

                SpawnManager.Instance.CountEnemyDeath();
                break;
            case "Enemy":
                if (transform.position.z > 14)
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
                break;
            default:
                break;
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

    public IEnumerator SendToPool()
    {
        _isExploding = true;
        _collider.enabled = false;
        yield return new WaitForSeconds(.33f);
        _isExploding = false;
        _collider.enabled = true;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.gameObject.SetActive(false);
    }
}
