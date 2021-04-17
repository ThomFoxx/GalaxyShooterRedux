using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8.5f;
    private Transform _pool;
    private Transform _container;
    private Transform _lastOwner;

   
    void Start()
    {
        if (_pool == null)
            _pool = GameObject.Find("LaserPool").transform;
        if (_container == null)
            _container = GameObject.Find("LaserContainer").transform;
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
        if (Vector3.Distance(Vector3.zero, transform.position) < 30)
            transform.Translate(transform.TransformDirection(Vector3.forward) * _speed * Time.deltaTime, Space.World);
        else
            transform.parent = _pool;

        if (transform.position.z > 14)
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

    public void SendToPool()
    {
        transform.parent = _pool;
    }
}
