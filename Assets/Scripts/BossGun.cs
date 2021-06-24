using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossGun : MonoBehaviour
{
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private int _pointValue;
    [SerializeField]
    private Transform[] _laserOffset;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private bool _laserCanFire = true;
    [SerializeField]
    private Vector2 _laserCoolDown = new Vector2(.5f, 3);
    private GameObject _target;
    private bool _isExploding;
    [SerializeField]
    private GameObject _damage;
    private int _rounds = 5;


    public delegate void EnemyDeath(int pointValue, Transform self);
    public static event EnemyDeath OnEnemyDeath;

    private void Awake()
    {
        _target = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnEnable()
    {
        Boss.OnFireEnable += EnableFiring;
        Boss.OnFireDisable += DisableFiring;
        Player.OnPlayerDeath += PlayerDeath;
    }

    private void Update()
    {
        Movement();
        Weapon();
    }

    private void Movement()
    {
        if (!_isExploding)
        {
            if (_target != null)
                transform.LookAt(_target.transform);
        }
    }

    private void Weapon()
    {
        if (_laserCanFire && !_isExploding)
        {
            foreach (Transform laser in _laserOffset)
            {
                GameObject GO = PoolManager.Instance.RequestFromPool(_laserPrefab);
                SetupPoolObject(GO, laser);
            }
            _laserCanFire = false;
            if (_rounds > 0)
            {
                StartCoroutine(LaserReloadTimer());
            }
            else
            {
                StartCoroutine(LaserPause());
            }
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

    private void EnableFiring()
    {
        _laserCanFire = true;
        _isExploding = false;
    }

    private void DisableFiring()
    {
        _isExploding = true;
        _laserCanFire = false;
    }

    private void PlayerDeath()
    {
        Debug.Log("Player is dead already.");
        DisableFiring();
    }

    IEnumerator LaserReloadTimer()
    {
        float RNG = Random.Range(_laserCoolDown.x, _laserCoolDown.y);
            yield return new WaitForSeconds(RNG);
            _laserCanFire = true;
            _rounds--;
    }

    IEnumerator LaserPause()
    {
        yield return new WaitForSeconds(5);
        _rounds = 5;
        _laserCanFire = true;
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
                if (other.TryGetComponent(out Laser laser))
                {
                    if (laser.ReportLastOwner().CompareTag("Player"))
                    {
                        laser.SendToPool();

                        SpawnManager.Instance.SpawnExplosion(transform.position);
                        PlaySFX(1);
                        StartCoroutine(DisableGun());
                        if (OnEnemyDeath != null)
                            OnEnemyDeath(_pointValue, this.transform);

                    }
                }
                else if (other.TryGetComponent(out Missile missile))
                {
                    missile.SendToPool();

                    SpawnManager.Instance.SpawnExplosion(transform.position);
                    PlaySFX(1);
                    StartCoroutine(DisableGun());
                    if (OnEnemyDeath != null)
                        OnEnemyDeath(_pointValue, this.transform);

                }
                break;
            default:
                break;

        }
    }

    public IEnumerator DisableGun()
    {
        _isExploding = true;
        _collider.enabled = false;
        yield return new WaitForSeconds(.33f);
        _damage.SetActive(true);
        transform.tag = "";
    }

    private void OnDisable()
    {
        Boss.OnFireEnable -= EnableFiring;
        Boss.OnFireDisable += DisableFiring;
        Player.OnPlayerDeath -= PlayerDeath;
    }
}
