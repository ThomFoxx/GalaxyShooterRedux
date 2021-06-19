using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldUnit : MonoBehaviour
{

    private bool _isExploding = false;
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private int _pointValue;
    [SerializeField]
    private bool _shieldActive;
    [SerializeField]
    private GameObject _shield;
    private Renderer _shieldRenderer;

    public delegate void EnemyDeath(int pointValue, Transform self);
    public static event EnemyDeath OnEnemyDeath;

    private void Awake()
    {
        _shieldRenderer = _shield.GetComponent<Renderer>();
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
                            OnEnemyDeath(_pointValue / 2, this.transform);

                        SpawnManager.Instance.CountEnemyDeath();
                    }
                    else
                    {
                        _shieldActive = false;
                        StartCoroutine(ShieldPowerDownRoutine());
                    }
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
