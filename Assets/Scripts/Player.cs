using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    private float _speedPowerUpAmount = 1.5f;
    private float _speedBoostMultipler = 1;
    private Vector3 _direction;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private GameObject _sparkPrefab;
    [SerializeField]
    [Tooltip("Normal Shot Offset")]
    private Transform _laserOffset;
    [SerializeField]
    [Tooltip("Triple Shot Offset")]
    private Transform[] _tripleShotOffset;
    [SerializeField]
    [Tooltip("Spread Shot Offset")]
    private Transform[] _spreadShotOffset;
    [SerializeField]
    [Tooltip("Normal Shot Offset")]
    private Transform _sparkOffset;
    private Transform _laserPool;
    [SerializeField]
    private bool _tripleShotActive, _speedBoostActive, _shieldActive, _spreadShotActive;
    [SerializeField]
    private GameObject _shield;
    private int _shieldCount;
    private Renderer _shieldRenderer;
    [SerializeField]
    private float[] _shieldPowerLevels;
    [SerializeField]
    private Image _shieldPowerImage;
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
    private float _spreadShotCooldownTimer = 0;
    [SerializeField]
    private int _score;
    private bool _isExploding = false;
    [SerializeField]
    private float _thrusterBoostAmount;
    private float _thrusterBoostMultiplier;
    private int _ammoCount = 5;
    [SerializeField]
    private int _ammoClip;
    [SerializeField]
    private int _maxAmmo;
    [SerializeField]
    private float _thrusterPowerMax = 5;
    [SerializeField]
    private float _thrusterPower;
    [SerializeField]
    private Image _thrusterPowerImage;
    [SerializeField]
    private bool _canThrust = true;
    private bool _thrusterActive = false;


    [SerializeField]
    [Tooltip("For Debugging/Testing")]
    private float Speed;


    public delegate void PlayerDeath();
    public static event PlayerDeath OnPlayerDeath;
    public delegate void PlayerDamaged(int LivesCount);
    public static event PlayerDamaged OnPlayerDamaged;
    public delegate void ReloadAmmo(int Current, int Max);
    public static event ReloadAmmo OnReloadAmmo;
    public delegate void AmmoTypeChange(int Type);
    public static event AmmoTypeChange OnAmmoTypeChange;

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
        _shieldPowerImage.material.SetFloat("_visibility", _shieldPowerLevels[_shieldCount]);

        if (OnReloadAmmo != null)
            OnReloadAmmo(_ammoCount, _maxAmmo);
        if (OnAmmoTypeChange != null)
            OnAmmoTypeChange(0);

        _thrusterPower = _thrusterPowerMax;
        _thrusterPowerImage.material.SetFloat("_visibility", _thrusterPower);
    }

    void Update()
    {
        Thruster();

        if (!_isExploding)
            Movement();

        Weapon();
    }

    private void Movement()
    {
        float horizontalInput;
        float verticalInput;
        Vector3 totalDirection;

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

            EngineMaintence(horizontalInput);
        }
        else if (!_horizontalFlight)
        { //Top Down motion
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            _direction = new Vector3(horizontalInput, 0, verticalInput);
            totalDirection = _direction * _speed * _speedBoostMultipler * _thrusterBoostMultiplier * Time.deltaTime;
            transform.Translate(totalDirection);

            //For Tracking the Speed Changes in Inspector. REMOVE LATER. ~THK~
            Speed = _speed * _speedBoostMultipler * _thrusterBoostMultiplier;

            if (transform.position.z >= 0)
                transform.position = new Vector3(transform.position.x, 0, 0);
            else if (transform.position.z <= -10.75f)
                transform.position = new Vector3(transform.position.x, 0, -10.75f);
            if (transform.position.x >= 22f)
                transform.position = new Vector3(22f, 0, transform.position.z);
            else if (transform.position.x <= -22f)
                transform.position = new Vector3(-22f, 0, transform.position.z);

            EngineMaintence(verticalInput);
            RollControl(horizontalInput);
        }
    }

    private void Thruster()
    {
        if (Input.GetKey(KeyCode.LeftShift) && _thrusterPower > 0 && _canThrust)
        {
            _thrusterBoostMultiplier = _thrusterBoostAmount;
            StopCoroutine(ThrusterPowerUp());
            if (!_thrusterActive)
                StartCoroutine(ThrusterPowerDown());
        }
        else 
        {
            _thrusterActive = false;
            _thrusterBoostMultiplier = 1;
            StopCoroutine(ThrusterPowerDown());
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            StopCoroutine(ThrusterPowerDown());
            _thrusterActive = false;
            StartCoroutine(ThrusterPowerUp());
        }
        
    }

    private IEnumerator ThrusterPowerDown()
    {
        _thrusterActive = true;
        while (_thrusterPower > 0 && _thrusterActive)
        {
            yield return new WaitForEndOfFrame();
            _thrusterPower -= Time.deltaTime;
            _thrusterPowerImage.material.SetFloat("_visibility", _thrusterPower);
        }
        if (_thrusterPower < _thrusterPowerMax * .25f)
            _canThrust = false;
        _thrusterActive = false;
        if (_thrusterPower < 0)
            _thrusterPower = 0;
    }

    private IEnumerator ThrusterPowerUp()
    {
        while (_thrusterPower < _thrusterPowerMax)
        {
            yield return new WaitForEndOfFrame();
            _thrusterPower += Time.deltaTime/2;
            _thrusterPowerImage.material.SetFloat("_visibility", _thrusterPower);
        }
        _canThrust = true;
            if (_thrusterPower > _thrusterPowerMax)
            _thrusterPower = _thrusterPowerMax;
    }

    private void EngineMaintence(float Input)
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
        if (Input.GetKey(KeyCode.Space) && _laserCanFire && _ammoCount > 0)
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
                _ammoCount--;
            }
            if (_spreadShotActive) //Spread Shot
            {
                foreach (Transform laser in _spreadShotOffset)
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
                _ammoCount--;
            }
            if (!_tripleShotActive && !_spreadShotActive)//Normal Shot
            {
                Vector3 launch = _laserOffset.position;
                if (_laserPool.childCount < 1)
                    Instantiate(_laserPrefab, launch, transform.rotation, this.transform);
                else
                    PullLaserFromPool(launch, _laserOffset);

                _laserCanFire = false;
                StartCoroutine(LaserReloadTimer());
                PlaySFX(2);
                _ammoCount--;
            }
            if (OnReloadAmmo != null)
                OnReloadAmmo(_ammoCount, _maxAmmo);
        }
        else if (Input.GetKey(KeyCode.Space) && _laserCanFire)
        {
            Vector3 launch = _sparkOffset.position;
            GameObject spark = Instantiate(_sparkPrefab, launch, transform.rotation, this.transform);
            _laserCanFire = false;
            StartCoroutine(LaserReloadTimer());
            Destroy(spark.gameObject, .5f);
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
                StartCoroutine(TriggerDamage());
        }
        else if (_shieldCount >= 1)
        {
            _shieldCount--;
            _shieldPowerImage.material.SetFloat("_visibility", _shieldPowerLevels[_shieldCount]);
            StartCoroutine(ShieldPowerDownRoutine());
        }
    }

    private IEnumerator TriggerDamage()
    {
        bool triggered = false;
        int RNDDamage;
        while (!triggered)
        {
            yield return new WaitForEndOfFrame();
            RNDDamage = Random.Range(0, _damagePoints.Length);
            if (!_damagePoints[RNDDamage].activeInHierarchy)
            {
                _damagePoints[RNDDamage].SetActive(true);
                triggered = true;
            }         
        }
    }

    private IEnumerator RepairDamage()
    {
        bool triggered = false;
        int RNDDamage;
        while (!triggered)
        {
            yield return new WaitForEndOfFrame();
            RNDDamage = Random.Range(0, _damagePoints.Length);
            if (_damagePoints[RNDDamage].activeInHierarchy)
            {
                _damagePoints[RNDDamage].SetActive(false);
                triggered = true;
            }
        }
        _lives++;
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

                Reload();
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
            case 3:
                Reload();
                break;
            case 4:
                if (_lives < 3)
                    StartCoroutine(RepairDamage());
                break;
            case 5:
                _spreadShotCooldownTimer += 5;
                if (!_spreadShotActive)
                    StartCoroutine(SpreadShotRoutine());

                Reload();
                break;
            default:
                break;
        }
    }

    private void Reload()
    {
        if (_ammoCount < _maxAmmo)
        {
            _ammoCount += _ammoClip;
            if (_ammoCount > _maxAmmo)
                _ammoCount = _maxAmmo;
        }
        if (OnReloadAmmo != null)
            OnReloadAmmo(_ammoCount, _maxAmmo);
    }

    IEnumerator TripleShotRoutine()
    {
        if (OnAmmoTypeChange != null)
            OnAmmoTypeChange(1);
        _tripleShotActive = true;
        while (_tripleShotActive)
        {
            yield return new WaitForEndOfFrame();
            _tripleShotCooldownTimer -= Time.deltaTime;
            if (_tripleShotCooldownTimer <= 0)
            {
                _tripleShotCooldownTimer = 0;
                _tripleShotActive = false;
                if (OnAmmoTypeChange != null)
                {
                    if (_spreadShotActive)
                        OnAmmoTypeChange(2);
                    else
                        OnAmmoTypeChange(0);
                }
            }
        }
    }

    IEnumerator SpreadShotRoutine()
    {
        if (OnAmmoTypeChange != null)
            OnAmmoTypeChange(2);
        _spreadShotActive = true;
        while (_spreadShotActive)
        {
            yield return new WaitForEndOfFrame();
            _spreadShotCooldownTimer -= Time.deltaTime;
            if (_spreadShotCooldownTimer <= 0)
            {
                _spreadShotCooldownTimer = 0;
                _spreadShotActive = false;
                if (OnAmmoTypeChange != null)
                {
                    if (_tripleShotActive)
                        OnAmmoTypeChange(1);
                    else
                        OnAmmoTypeChange(0);
                }
            }
        }
    }

    IEnumerator SpeedBoostRoutine()
    {
        _speedBoostActive = true;
        _speedBoostMultipler = _speedPowerUpAmount;
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
        float powerLevel = 0;

        while (power > -0.5f)
        {
            yield return new WaitForEndOfFrame();
            powerLevel += Time.deltaTime * 4.5f;
            power -= Time.deltaTime * 15;
            _shieldRenderer.material.SetFloat("_power", power);
            _shieldPowerImage.material.SetFloat("_visibility", powerLevel);
        }
        while (power < 1.5f)
        {
            yield return new WaitForEndOfFrame();
            power += Time.deltaTime * 5;
            _shieldRenderer.material.SetFloat("_power", power);
        }
        _shieldPowerImage.material.SetFloat("_visibility", _shieldPowerLevels[_shieldCount]);
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
