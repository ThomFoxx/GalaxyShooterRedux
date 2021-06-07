using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8.5f;
    [SerializeField]
    private float _enemySpeedMultiplier = 1.5f;
    private Transform _lastOwner;

    void Start()
    {

        if (_lastOwner == null)
            SetLastOwner(PoolManager.Instance.transform);
    }

    void Update()
    {
        if (transform.gameObject.activeInHierarchy)
            if (_lastOwner != null)
                Movement();
            else
                SendToPool();
    }

    private void Movement()
    {
        if (_lastOwner.CompareTag("Enemy"))
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed * _enemySpeedMultiplier * Time.deltaTime, Space.World);
        else
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed * Time.deltaTime, Space.World);

        if (transform.position.z > 14 && _lastOwner.CompareTag("Player"))
            SendToPool();
        else if (transform.position.z < -14 && _lastOwner.CompareTag("Enemy"))
            SendToPool();
    }


    public void SetLastOwner(Transform owner)
    {
        _lastOwner = owner;        
    }

    public Transform ReportLastOwner()
    {
        return _lastOwner;
    }

    public void SendToPool()
    {
        transform.localPosition = Vector3.zero;
        transform.localScale = Vector3.one;
        transform.gameObject.SetActive(false);
        SetLastOwner(PoolManager.Instance.transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_lastOwner.CompareTag("GameController"))
            if (!_lastOwner.CompareTag("Player") && other.CompareTag("Player"))
            {
                if (other.TryGetComponent(out Player player))
                    player.Damage();

                SendToPool();
            }
    }
}
