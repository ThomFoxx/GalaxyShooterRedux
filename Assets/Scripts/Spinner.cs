using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spinner : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    private bool _isBlinker;
    [SerializeField]
    private Light[] _lights;

    private void Start()
    {
        if (_isBlinker)
            StartCoroutine(Blinker());
    }
    private void Update()
    {
        transform.Rotate(Vector3.forward, _speed*Time.deltaTime);

    }

    IEnumerator Blinker()
    {
        while(_isBlinker)
        {
            yield return new WaitForSeconds(.5f);
            foreach (Light light in _lights)
            {
                light.enabled = false;
            }
            yield return new WaitForSeconds(.33f);
            foreach (Light light in _lights)
            {
                light.enabled = true;
            }
        }
    }

}
