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
    private bool _isExploding = false;


    public delegate void EnemyDeath(int pointValue);
    public static event EnemyDeath OnEnemyDeath;

    private void OnEnable()
    {
        _isTeleporting = false;
        Player.OnPlayerDeath += PlayerDeath;
        StartCoroutine(SingleRandomTeleport());
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, (1*Time.deltaTime), Space.World);
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

                        SpawnManager.Instance.CountEnemyDeath();
                    }
                }
                break;
            case "Player":
                if (other.TryGetComponent(out Player player))
                    player.Damage();

                SpawnManager.Instance.SpawnExplosion(transform.position);
                PlaySFX(1);
                StartCoroutine(SendToPool());
                if (OnEnemyDeath != null)
                    OnEnemyDeath(_pointValue / 2);

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