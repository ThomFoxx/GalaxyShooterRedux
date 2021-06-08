using System.Collections;
using UnityEngine;

public class Teleporter : MonoBehaviour
{

    private float _scale = 1;
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private Vector2 _xLimits, _zLimits;
    private bool _isTeleporting = false;

    [SerializeField]
    private int _pointValue;
    private bool _respawning = true;
    private bool _isExploding;
    [SerializeField]
    private GameObject _missilePrefab;
    [SerializeField]
    private Vector3 _missileScale;
    private Missile _missile = new Missile();
    [SerializeField]
    private Transform _player;


    public delegate void EnemyDeath(int pointValue, Transform self);
    public static event EnemyDeath OnEnemyDeath;

    private void OnEnable()
    {
        _isTeleporting = false;
        Player.OnPlayerDeath += PlayerDeath;
        Missile.OnMissileDeath += MissileLoss;
        StartCoroutine(SingleRandomTeleport());

    }

    void Start()
    {
        _player = GameObject.FindObjectOfType<Player>().transform;
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, (1 * Time.deltaTime), Space.World);
        if (transform.position.x < _xLimits.x)
            SingleRandomTeleport();
        if (transform.position.x > _xLimits.y)
            SingleRandomTeleport();
    }

    public IEnumerator WeaponCheck()
    {
        while (_player != null && transform.gameObject.activeSelf)
        {
            Weapon();
            float RNG = Random.Range(5, 10);
            yield return new WaitForSeconds(RNG);
        }
    }

    private void Weapon()
    {
        float RNG = Random.Range(-20, 20);
        GameObject GO = PoolManager.Instance.RequestFromPool(_missilePrefab);
        _missile = GO.GetComponent<Missile>();
        _missile.transform.position = new Vector3(RNG, 0, 20);
        _missile.transform.rotation = Quaternion.Euler(0, 180, 0);
        _missile.transform.localScale = _missileScale;
        _missile.SetLastOwner(this.transform);
        _missile.transform.gameObject.SetActive(true);
        _missile.SetTarget(_player);
        Debug.Log("Firing " + _missile.name);
        _missile = new Missile();
    }

    public void ReleaseMissile()
    {
        _missile = new Missile();
    }

    private void MissileLoss(Transform missile)
    {
        if (_missile != null && missile == _missile.transform)
        {
            Debug.Log("Received Termination");
            _missile = new Missile();
        }
    }

    private IEnumerator SingleRandomTeleport()
    {
        if (!_isTeleporting)
        {
            float RNDX = Random.Range(_xLimits.x, _xLimits.y);
            float RNDZ = Random.Range(_zLimits.x, _zLimits.y);
            _isTeleporting = true;
            yield return Teleport(new Vector3(RNDX, 0, RNDZ));
        }
        yield return new WaitForEndOfFrame();

    }

    private IEnumerator ContinualRandomTeleport()
    {
        while (true)
        {
            if (!_isTeleporting)
            {
                float RNDX = Random.Range(_xLimits.x, _xLimits.y);
                float RNDZ = Random.Range(_zLimits.x, _zLimits.y);
                _isTeleporting = true;
                yield return Teleport(new Vector3(RNDX, 0, RNDZ));
            }
            yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Teleport(Vector3 NewPOS)
    {
        _collider.enabled = false;

        while (_scale > 0)
        {
            _scale -= Time.deltaTime;
            transform.localScale = new Vector3(_scale, 1, _scale);
            yield return new WaitForEndOfFrame();
        }
        if (_scale <= 0)
        {
            transform.position = NewPOS;
            _scale = 0;
            transform.localScale = new Vector3(_scale, 1, _scale);
        }
        while (_scale < 1)
        {
            _scale += Time.deltaTime;
            transform.localScale = new Vector3(_scale, 1, _scale);
            yield return new WaitForEndOfFrame();
        }
        if (_scale >= 1)
        {
            _scale = 1;
            transform.localScale = new Vector3(_scale, 1, _scale);
            _isTeleporting = false;
            _collider.enabled = true;
        }
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
                else if (other.TryGetComponent(out Missile missile) && !missile.ReportLastOwner().TryGetComponent(out Teleporter teleport))
                {
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
                StartCoroutine(SingleRandomTeleport());
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
        Missile.OnMissileDeath += MissileLoss;
    }

    public IEnumerator SendToPool()
    {
        _isExploding = true;
        _collider.enabled = false;
        _missile.ResetTarget();
        ReleaseMissile();
        StopCoroutine(WeaponCheck());

        yield return new WaitForSeconds(.33f);

        _isExploding = false;
        _collider.enabled = true;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.gameObject.SetActive(false);        
    }
}