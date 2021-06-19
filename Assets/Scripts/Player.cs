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
    private GameObject _missilePrefab;
    [SerializeField]
    private Vector3 _missileScale;
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
    [SerializeField]
    [Tooltip("'Active Cycle' GO to effect any change in PlayMode")]
    private float _missileCoolDown = 1;
    private WaitForSeconds _missileCoolDownTimer;
    private float _tripleShotCooldownTimer = 0;
    private float _speedBoostCooldownTimer = 0;
    private float _spreadShotCooldownTimer = 0;
    [SerializeField]
    private int _score;
    private bool _isExploding = false;
    [SerializeField]
    private float _thrusterBoostAmount;
    private float _thrusterBoostMultiplier;
    private int _ammoCount = 15;
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
    private int _missileCount = 0;
    private bool _missileReloaded = true;
    private bool _canUseMagnet = false;
    private float _magnetTimer;
    [SerializeField]
    private float _maxMagnetTimer = 5;


    [SerializeField]
    [Tooltip("For Debugging/Testing")]
    private float Speed;
    [SerializeField]
    Collider[] HitInfo = new Collider[10];


    public delegate void PlayerDeath();
    public static event PlayerDeath OnPlayerDeath;
    public delegate void PlayerDamaged(int LivesCount);
    public static event PlayerDamaged OnPlayerDamaged;
    public delegate void ReloadAmmo(int Current, int Max);
    public static event ReloadAmmo OnReloadAmmo;
    public delegate void AmmoTypeChange(int Type);
    public static event AmmoTypeChange OnAmmoTypeChange;
    public delegate void MagnetPull();
    public static event MagnetPull OnMagnetPull;
    public static event MagnetPull OnMagnetStop;

    private void OnEnable()
    {
        _laserCoolDownTimer = new WaitForSeconds(_laserCoolDown);
        _missileCoolDownTimer = new WaitForSeconds(_missileCoolDown);
        Enemy.OnEnemyDeath += EnemyDeath;
        Teleporter.OnEnemyDeath += EnemyDeath;
        Aggressive_Enemy.OnEnemyDeath += EnemyDeath;
        Mine_Layer.OnEnemyDeath += EnemyDeath;
        ShieldUnit.OnEnemyDeath += EnemyDeath;
        Mine.OnMineDestroy += EnemyDeath;
    }

    void Start()
    {

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

        PowerUpFunctions();
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

    private void PowerUpFunctions()
    {
        if (_canUseMagnet)
        {
            if (Input.GetKeyDown(KeyCode.C))
            {
                if (OnMagnetPull != null)
                    OnMagnetPull();

                StartCoroutine(MagentTimerRoutine());
            }
            else if (Input.GetKeyUp(KeyCode.C))
            {
                if (OnMagnetStop != null)
                    OnMagnetStop();

                StopCoroutine(MagentTimerRoutine());
            }
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
                    GameObject GO = PoolManager.Instance.RequestFromPool(_laserPrefab);
                    SetupPooledLaser(GO, laser);
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
                    GameObject GO = PoolManager.Instance.RequestFromPool(_laserPrefab);
                    SetupPooledLaser(GO, laser);
                }
                _laserCanFire = false;
                StartCoroutine(LaserReloadTimer());
                PlaySFX(2);
                _ammoCount--;
            }
            if (!_tripleShotActive && !_spreadShotActive)//Normal Shot
            {
                GameObject GO = PoolManager.Instance.RequestFromPool(_laserPrefab);
                SetupPooledLaser(GO, _laserOffset);

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
        if (Input.GetKeyDown(KeyCode.LeftControl) && _missileReloaded && _missileCount > 0)
        { 
            _missileCount--;
            UIManager.Instance.MissileUpdate(_missileCount);
            GameObject GO = PoolManager.Instance.RequestFromPool(_missilePrefab);
            GO.transform.forward = Vector3.forward;
            GO.transform.position = _laserOffset.position;
            GO.transform.localScale = _missileScale;
            GO.SetActive(true);
            GO.GetComponent<Missile>().SetLastOwner(transform);
            GO.GetComponent<Missile>().SetTarget(TargetForMissile());
           
            StartCoroutine(MissileReloadTimer());
            
        }

    }

    private void SetupPooledLaser(GameObject Obj, Transform Offset)
    {
        Obj.transform.position = Offset.position;
        Obj.transform.rotation = Offset.rotation;
        Obj.transform.localScale = Offset.localScale;
        Obj.GetComponent<Laser>().SetLastOwner(this.transform);
        Obj.SetActive(true);
    }

    IEnumerator LaserReloadTimer()
    {
        yield return _laserCoolDownTimer;
        _laserCanFire = true;
    }

    IEnumerator MissileReloadTimer()
    {
        yield return _missileCoolDownTimer;
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
        StartCoroutine(UIManager.Instance.UpdateLives(_lives));
    }

    private void PlaySFX(int SFXGroup)
    {
        AudioManager.Instance.PlaySFX(SFXGroup);
    }

    private void EnemyDeath(int PointValue, Transform notUsed)
    {
        Debug.Log("Enemy Destroyed");
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
            case 6:
                _missileCount = 3;
                StartCoroutine(UIManager.Instance.UpdateMissiles(_missileCount));
                break;
            case 7:
                if (_canUseMagnet)
                {
                    _magnetTimer = _maxMagnetTimer;
                    UIManager.Instance.MagnetUpdate(Mathf.CeilToInt(_magnetTimer));
                }
                else
                {
                    _canUseMagnet = true;
                    _magnetTimer = _maxMagnetTimer;
                    UIManager.Instance.MagnetUpdate(Mathf.CeilToInt(_magnetTimer));
                }

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

    IEnumerator MagentTimerRoutine()
    {
        UIManager.Instance.MagnetUpdate(Mathf.CeilToInt(_magnetTimer));
        while(Input.GetKey(KeyCode.C) && _magnetTimer > 0)
        {
           yield return new WaitForEndOfFrame();
            _magnetTimer -= Time.deltaTime;
            UIManager.Instance.MagnetUpdate(Mathf.CeilToInt(_magnetTimer));
        }
        if (_magnetTimer <= 0)
        {
            _canUseMagnet = false;
            UIManager.Instance.MagnetUpdate(0);
            if (OnMagnetStop != null)
                OnMagnetStop();
        }
    }

    private Transform TargetForMissile()
    {
        GameObject[] targets;
        GameObject closestTarget = null;
        float dist, minDist = 100f;
        targets = GameObject.FindGameObjectsWithTag("Enemy");
        for (int x = 0; x < targets.Length; x++)
        {
            dist = Vector3.Distance(transform.position, targets[x].transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closestTarget = targets[x];
            }
        }
        return closestTarget.transform;
    }

    private void OnDisable()
    {
        Enemy.OnEnemyDeath -= EnemyDeath;
        Teleporter.OnEnemyDeath -= EnemyDeath;
        Aggressive_Enemy.OnEnemyDeath -= EnemyDeath;
        Mine_Layer.OnEnemyDeath -= EnemyDeath;
        ShieldUnit.OnEnemyDeath -= EnemyDeath;
        Mine.OnMineDestroy -= EnemyDeath;
    }
}
