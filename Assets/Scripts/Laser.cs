using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Laser : MonoBehaviour
{
    [SerializeField]
    private float _speed = 8.5f;
    [SerializeField]
    private Transform _pool;
    private Transform _lastParent;

   
    void Start()
    {
        if (_pool == null)
            _pool = GameObject.Find("LaserPool").transform;
        if (_lastParent == null)
            SetLastParent(transform.parent);
        
    }

    void Update()
    {
        if (transform.gameObject.activeInHierarchy && transform.parent != _pool)
            Movement();
    }

    private void Movement()
    {
        if (Vector3.Distance(transform.position, _lastParent.position) > 15)
            transform.parent = _pool;
        else
            transform.Translate(transform.TransformDirection(Vector3.forward)* _speed * Time.deltaTime, Space.World);
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
            SetLastParent(transform.parent);
        }
    }

    public void SetLastParent(Transform parent)
    {
        _lastParent = parent;
        transform.parent = null;
    }
}
