using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Aggressive_Enemy : MonoBehaviour
{
    [SerializeField]
    private bool _horizontalFlight;
    [SerializeField]
    private float _shipSpeed;
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
    private bool _laserCanFire = true;
    [SerializeField]
    private Vector2 _laserCoolDown = new Vector2(.5f, 3);
    [SerializeField]
    private GameObject _target;
    [SerializeField]
    private int _shieldChance;
    [SerializeField]
    private bool _shieldActive;
    [SerializeField]
    private GameObject _shield;
    private Renderer _shieldRenderer;

    public delegate void EnemyDeath(int pointValue, Transform self);
    public static event EnemyDeath OnEnemyDeath;
    public delegate void EnemyRespawn(Transform self);
    public static event EnemyRespawn OnEnemyRespawn;


    private void Awake()
    {
        _shieldRenderer = _shield.GetComponent<Renderer>();
        _target = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnEnable()
    {
        Player.OnPlayerDeath += PlayerDeath;
        StartCoroutine(LaserReloadTimer());
        StartCoroutine(AttackPattern());
        _speed = _shipSpeed;
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
        {
            if (_target != null)
                transform.LookAt(_target.transform);

            transform.Translate(Vector3.forward * _speed * Time.deltaTime, Space.Self);
        }
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
    }

    private void SetupPoolObject(GameObject Obj, Transform Offset)
    {
        Obj.transform.position = Offset.position;
        Obj.transform.rotation = Offset.rotation;
        Obj.transform.localScale = Offset.localScale;
        Obj.GetComponent<Laser>().SetLastOwner(this.transform);
        Obj.SetActive(true);
    }

    IEnumerator AttackPattern()
    {
        while (transform.gameObject.activeSelf)
        {
            yield return new WaitForSeconds(3f);
            float tempSpeed = _speed;
            _speed = 0;
            yield return new WaitForSeconds(5f);
            _speed = tempSpeed;
        }
        yield return new WaitForEndOfFrame();
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

                float RNGShield = Random.Range(0, 100);
                if (RNGShield <= _shieldChance * SpawnManager.Instance.ReportCurrentWave())
                {
                    _shieldActive = true;
                    ShieldPowerON();
                }
                else
                {
                    _shieldActive = false;
                    ShieldPowerOFF();
                }

                if (_target != null)
                    transform.LookAt(_target.transform.position);
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
                        if (!_shieldActive)
                        {
                            laser.SendToPool();

                            SpawnManager.Instance.SpawnExplosion(transform.position);
                            PlaySFX(1);
                            StartCoroutine(SendToPool());
                            if (OnEnemyDeath != null)
                                OnEnemyDeath(_pointValue, this.transform);

                            SpawnManager.Instance.CountEnemyDeath();
                        }
                        else
                        {
                            _shieldActive = false;
                            StartCoroutine(ShieldPowerDownRoutine());
                            laser.SendToPool();
                        }
                    }
                }
                else if (other.TryGetComponent(out Missile missile))
                {
                    if (!_shieldActive)
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
                    else
                    {
                        _shieldActive = false;
                        StartCoroutine(ShieldPowerDownRoutine());
                        missile.SendToPool();
                    }
                }
                break;
            case "Player":
                if (other.TryGetComponent(out Player player))
                {
                    player.Damage();


                    if (!_shieldActive)
                    {
                        SpawnManager.Instance.SpawnExplosion(transform.position);
                        PlaySFX(1);
                        StartCoroutine(SendToPool());
                        if (OnEnemyDeath != null)
                            OnEnemyDeath(0, this.transform);

                        SpawnManager.Instance.CountEnemyDeath();
                    }
                    else
                    {
                        _shieldActive = false;
                        StartCoroutine(ShieldPowerDownRoutine());
                    }
                }
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

    private void ShieldPowerON()
    {
        _shield.SetActive(true);
        _shieldRenderer.material.SetFloat("_power", 1f);
    }

    private void ShieldPowerOFF()
    {
        _shield.SetActive(false);
        _shieldRenderer.material.SetFloat("_power", 0);
    }

    IEnumerator ShieldPowerDownRoutine()
    {
        _collider.enabled = false;
        float power = 2f;
        _shieldRenderer.material.SetFloat("_power", power);
        if (!_shieldActive)
        {
            yield return new WaitForSeconds(.33f);
            while (power > -3f)
            {
                yield return new WaitForEndOfFrame();
                power -= Time.deltaTime * 20;
                _shieldRenderer.material.SetFloat("_power", power);
            }
            power = 0f;
            _shieldRenderer.material.SetFloat("_power", power);
            _shieldActive = false;
            _shield.SetActive(false);
        }
        _collider.enabled = true;
    }

    private void PlayerDeath()
    {
        _respawning = false;
    }

    private void OnDisable()
    {
        Player.OnPlayerDeath -= PlayerDeath;
        StopCoroutine(AttackPattern());
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
