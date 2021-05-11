using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8.5f;
    [SerializeField]
    private float _enemySpeedMultiplier = 1.5f;
    private Transform _pool;
    private Transform _container;
    private Transform _lastOwner;


    void Start()
    {
        if (_pool == null)
            _pool = GameObject.Find("Laser_Pool").transform;
        if (_container == null)
            _container = GameObject.Find("Laser_Container").transform;
        if (_lastOwner == null)
            SetLastOwner(transform.parent);        
    }

    void Update()
    {
        if (transform.gameObject.activeInHierarchy && transform.parent != _pool)
            Movement();
    }

    private void Movement()
    {
    /*    if (Vector3.Distance(Vector3.zero, transform.position) < 30)
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed * Time.deltaTime, Space.World);
        else
            transform.parent = _pool;
    */
        if (_lastOwner.CompareTag("Enemy"))
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed * _enemySpeedMultiplier * Time.deltaTime, Space.World);
        else
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed * Time.deltaTime, Space.World);

        if (transform.position.z > 7 && _lastOwner.CompareTag("Player"))
            transform.parent = _pool;
        else if (transform.position.z < -10 && _lastOwner.CompareTag("Enemy"))
            transform.parent = _pool;
    }

    private void OnTransformParentChanged()
    {
        if (transform.parent == _pool)
        {
            transform.localPosition = Vector3.zero;
            transform.gameObject.SetActive(false);
        }
        else if (transform.parent != null)
        {
            SetLastOwner(transform.parent);
        }
    }

    public void SetLastOwner(Transform parent)
    {
        _lastOwner = parent;
        transform.parent = _container;
    }

    public Transform ReportLastOwner()
    {
        return _lastOwner;
    }

    public void SendToPool()
    {
        transform.parent = _pool;
    }
}
