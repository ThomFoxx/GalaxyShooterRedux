using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private Collider _detectionCollider;
    [SerializeField]
    private int _pointValue = 10;
    private bool _isExploding;
    [SerializeField]
    private float _speed;
    


    public delegate void MineDestroy(int pointValue, Transform self);
    public static event MineDestroy OnMineDestroy;

    private void Update()
    {
        if (_speed > 0 )
            Movement();
    }

    private void Movement()
    {
        transform.Translate(Vector3.forward * _speed * Time.deltaTime, Space.Self);

    }

    public IEnumerator Slowdown(int TravelTime)
    {
        for (int i = 0; i < TravelTime; i++)
        {
            yield return new WaitForSeconds(1f);

        }
        _speed = 0;
    }
        
    private void PlaySFX(int SFXGroup)
    {
        AudioManager.Instance.PlaySFX(SFXGroup);
    }

    public void Explosion()
    {
        SpawnManager.Instance.SpawnExplosion(transform.position);
        PlaySFX(1);
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

                        Explosion();
                        if (OnMineDestroy != null)
                            OnMineDestroy(_pointValue, this.transform);

                    }
                }
                break;

            case "Player":
                if (other.TryGetComponent(out Player player))
                    player.Damage();

                Explosion();
                if (OnMineDestroy != null)
                    OnMineDestroy(0, this.transform);
                                
                break;            

            default:
                break;
        }
    }

    public IEnumerator SendToPool()
    {
        _isExploding = true;
        _collider.enabled = false;
        _detectionCollider.enabled = false;
        yield return new WaitForSeconds(.33f);
        _isExploding = false;
        _collider.enabled = true;
        _detectionCollider.enabled = true;
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.gameObject.SetActive(false);
    }
}
