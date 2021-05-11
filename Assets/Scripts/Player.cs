using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private int _lives = 3;
    [SerializeField]
    private bool _horizontalFlight;
    [SerializeField]
    private GameObject _shipObject;
    [SerializeField]
    private float _speed = 5;
    [SerializeField]
    private float _speedBoostMultipler = 1;
    private Vector3 _direction;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    [Tooltip("Normal Shot Offset")]
    private Transform _laserOffset;
    [SerializeField]
    [Tooltip("Triple Shot Offset")]
    private Transform[] _tripleShotOffset;
    private Transform _laserPool;
    [SerializeField]
    private bool _tripleShotActive, _speedBoostActive, _shieldActive;
    [SerializeField]
    private GameObject _shield;
    private int _shieldCount;
    private Renderer _shieldRenderer;
    [SerializeField]
    private GameObject[] _thrusters, _damagePoints;
    [SerializeField]
    private bool _laserCanFire;
    [SerializeField]
    [Tooltip("'Active Cycle' GO to effect any change in PlayMode")]
    private float _laserCoolDown = .25f;
    private WaitForSeconds _laserCoolDownTimer;
    private float _tripleShotCooldownTimer = 0;
    private float _speedBoostCooldownTimer = 0;
    [SerializeField]
    private int _score;
    private bool _isExploding = false;


    public delegate void PlayerDeath();
    public static event PlayerDeath OnPlayerDeath;
    public delegate void PlayerDamaged(int LivesCount);
    public static event PlayerDamaged OnPlayerDamaged;

    private void OnEnable()
    {
        _laserCoolDownTimer = new WaitForSeconds(_laserCoolDown);
        Enemy.OnEnemyDeath += EnemyDeath;
    }

    void Start()
    {
        if (_laserPool == null)
            _laserPool = GameObject.Find("Laser_Pool").transform;
        else
        {
            GameObject Pool = new GameObject("Laser_Pool");
            _laserPool = Pool.transform;
        }

        transform.position = Vector3.zero;
        _shieldRenderer = _shield.GetComponent<Renderer>();
    }

    void Update()
    {
        if (!_isExploding)
            Movement();

        Weapon();
    }

    private void Movement()
    {
        float horizontalInput;
        float verticalInput;

        if (_horizontalFlight)
        { //Side scroll motion
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            _direction = new Vector3(0, verticalInput, horizontalInput);
            transform.Translate(_direction * _speed * Time.deltaTime);

            if (transform.position.z >= 3)
                transform.position = new Vector3(0, transform.position.y, 3);
            else if (transform.position.z <= -8.3f)
                transform.position = new Vector3(0, transform.position.y, -8.3f);
            if (transform.position.y >= 5.2f)
                transform.position = new Vector3(0, 5.2f, transform.position.z);
            else if (transform.position.y <= -3.7f)
                transform.position = new Vector3(0, -3.7f, transform.position.z);

            ThrusterMaintence(horizontalInput);
        }
        else if (!_horizontalFlight)
        { //Top Down motion
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            _direction = new Vector3(horizontalInput, 0, verticalInput);
            transform.Translate(_direction * _speed * _speedBoostMultipler * Time.deltaTime);

            if (transform.position.z >= 0)
                transform.position = new Vector3(transform.position.x, 0, 0);
            else if (transform.position.z <= -10.75f)
                transform.position = new Vector3(transform.position.x, 0, -10.75f);
            if (transform.position.x >= 19.5f)
                transform.position = new Vector3(19.5f, 0, transform.position.z);
            else if (transform.position.x <= -19.5f)
                transform.position = new Vector3(-19.5f, 0, transform.position.z);

            ThrusterMaintence(verticalInput);
            RollControl(horizontalInput);
        }
    }

    private void ThrusterMaintence(float Input)
    {

        if (Input > 0)
        {
            foreach (GameObject Thruster in _thrusters)
            {
                Thruster.GetComponent<Thrusters>().ForwardThrust();
            }
        }
        else if (Input < 0)
        {
            foreach (GameObject Thruster in _thrusters)
            {
                Thruster.GetComponent<Thrusters>().ReverseThrust();
            }
        }
        else
        {
            foreach (GameObject Thruster in _thrusters)
            {
                Thruster.GetComponent<Thrusters>().ZeroThrust();
            }
        }
    }

    private void RollControl(float Input)
    {
        float roll = 0;
        Quaternion setRoll;
        if (Input > 0 || Input < 0)
        {
            roll = Input * 45;
        }
        else
        {
            roll = 0;
        }
        setRoll = Quaternion.Euler(roll, 90, 0);
        _shipObject.transform.rotation = setRoll;
    }

    private void Weapon()
    {
        if (Input.GetKey(KeyCode.Space) && _laserCanFire)
        {            
            if (_tripleShotActive) //Triple Shot
            {
                foreach (Transform laser in _tripleShotOffset)
                {
                    Vector3 launch = laser.position;
                    if (_laserPool.childCount < 1)
                        Instantiate(_laserPrefab, launch, laser.rotation, this.transform);
                    else
                    {
                        PullLaserFromPool(launch, laser);
                    }
                }
                _laserCanFire = false;
                StartCoroutine(LaserReloadTimer());
                PlaySFX(2);
            }
            else //Normal Shot
            {
                Vector3 launch = _laserOffset.position;
                if (_laserPool.childCount < 1)
                    Instantiate(_laserPrefab, launch, transform.rotation, this.transform);
                else
                    PullLaserFromPool(launch, _laserOffset);

                _laserCanFire = false;
                StartCoroutine(LaserReloadTimer());
                PlaySFX(2);
            }

        }
    }

    private void PullLaserFromPool(Vector3 LaunchPOS, Transform Offset)
    {
        GameObject laserTemp = _laserPool.GetChild(0).gameObject;
        laserTemp.transform.parent = this.transform;
        laserTemp.transform.position = LaunchPOS;
        laserTemp.transform.rotation = Offset.rotation;
        laserTemp.GetComponent<Laser>().SetLastOwner(this.transform);
        laserTemp.SetActive(true);
    }

    IEnumerator LaserReloadTimer()
    {
        yield return _laserCoolDownTimer;
        _laserCanFire = true;
    }

    public void Damage()
    {
        if (!_shieldActive)
        {
            _lives--;
            if (OnPlayerDamaged != null)
                OnPlayerDamaged(_lives);

            if (_lives < 1)
            {
                if (OnPlayerDeath != null)
                    OnPlayerDeath();

                _isExploding = true;
                SpawnManager.Instance.SpawnExplosion(transform.position);
                PlaySFX(1);
                Destroy(this.gameObject, .25f);
            }
            else
                TriggerDamage();
        }
        else if (_shieldCount >= 1)
        {
            _shieldCount--;
            StartCoroutine(ShieldPowerDownRoutine());
        }
    }

    private void TriggerDamage()
    {
TryAgain:
        int RNDDamage = Random.Range(0, _damagePoints.Length);
        if (!_damagePoints[RNDDamage].activeInHierarchy)
            _damagePoints[RNDDamage].SetActive(true);
        else
            goto TryAgain;
    }

    private void RepairDamage()
    {
TryAgain:
        int RNDDamage = Random.Range(0, _damagePoints.Length);
        if (_damagePoints[RNDDamage].activeInHierarchy)
            _damagePoints[RNDDamage].SetActive(false);
        else
            goto TryAgain;

        _lives++;
        if (OnPlayerDamaged != null)
            OnPlayerDamaged(_lives);
    }

    private void PlaySFX(int SFXGroup)
    {
        AudioManager.Instance.PlaySFX(SFXGroup);
    }

    private void EnemyDeath(int PointValue)
    {
        _score += PointValue;
    }

    public int GetScore()
    { return _score; }

    public void PowerUp(int type)
    {
        switch (type)
        {
            case 0:
                _tripleShotCooldownTimer += 5;
                if (!_tripleShotActive)
                    StartCoroutine(TripleShotRoutine());
                break;
            case 1:
                _speedBoostCooldownTimer += 5;
                if (!_speedBoostActive)
                    StartCoroutine(SpeedBoostRoutine());
                break;
            case 2:
                if (!_shieldActive)
                    StartCoroutine(ShieldPowerUpRoutine());
                break;
            default:
                break;
        }
    }

    IEnumerator TripleShotRoutine()
    {
        _tripleShotActive = true;
        while (_tripleShotActive)
        {
            yield return new WaitForEndOfFrame();
            _tripleShotCooldownTimer -= Time.deltaTime;
            if (_tripleShotCooldownTimer <= 0)
            {
                _tripleShotCooldownTimer = 0;
                _tripleShotActive = false;
            }
        }
    }

    IEnumerator SpeedBoostRoutine()
    {
        _speedBoostActive = true;
        _speedBoostMultipler = 2.5f;
        while (_speedBoostActive)
        {
            yield return new WaitForEndOfFrame();
            _speedBoostCooldownTimer -= Time.deltaTime;
            if (_speedBoostCooldownTimer <= 0)
            {
                _speedBoostCooldownTimer = 0;
                _speedBoostActive = false;
            }
        }
        _speedBoostMultipler = 1;
    }

    IEnumerator ShieldPowerUpRoutine()
    {
        _shieldActive = true;
        _shieldCount = 3;
        _shield.SetActive(true);
        float power = 12f;
        _shieldRenderer.material.SetFloat("_power", power);
        while (power > -0.5f)
        {
            yield return new WaitForEndOfFrame();
            power -= Time.deltaTime * 15;
            _shieldRenderer.material.SetFloat("_power", power);
        }
        while (power < 1.5f)
        {
            yield return new WaitForEndOfFrame();
            power += Time.deltaTime * 5;
            _shieldRenderer.material.SetFloat("_power", power);
        }
        power = 1.5f;
        _shieldRenderer.material.SetFloat("_power", power);

    }

    IEnumerator ShieldPowerDownRoutine()
    {
        float powerStep = 1.5f;
        float power = _shieldRenderer.material.GetFloat("_power");
        float nextPower = power + powerStep;
        while (power < nextPower)
        {
            yield return new WaitForEndOfFrame();
            power += Time.deltaTime * 10;
            _shieldRenderer.material.SetFloat("_power", power);
        }
        power = nextPower;
        _shieldRenderer.material.SetFloat("_power", power);
        if (_shieldCount == 0)
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
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= EnemyDeath;
    }
}
