using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField]
    private GameObject[] _pooledObjects;
    [SerializeField]
    private GameObject _pool;
    [SerializeField]
    private Dictionary<string, List<GameObject>> _objectPool = new Dictionary<string, List<GameObject>>();



    private static PoolManager _instance;
    public static PoolManager Instance
    {
        get
        {
            if (_instance == null)
            {
                Debug.Log("The ObjectPooler is Null");
            }
            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        GeneratePools();
    }

    private void GeneratePools()
    {
        foreach (GameObject Object in _pooledObjects)
        {
            List<GameObject> list = new List<GameObject>();
            _objectPool.Add(Object.name, list);

            for (int i = 0; i < 10; i++)
            {
                GameObject newGO = Instantiate(Object);
                newGO.SetActive(false);
                newGO.transform.parent = _pool.transform;
                list.Add(newGO);
            }
        }
    }

    public GameObject RequestFromPool(GameObject RequestedPrefab)
    {
        List<GameObject> Pool = _objectPool[RequestedPrefab.name];

        foreach (GameObject Obj in Pool)
        {
            if (Obj.activeSelf == false)
                return Obj;
        }

        GameObject newObj = Instantiate(RequestedPrefab, _pool.transform);
        Pool.Add(newObj);
        return newObj;
    }
}
