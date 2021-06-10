using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    [Tooltip("See Spawn Manager Prefab Array")]
    private int _type;
    private bool _magnetized = false;
    private Transform _player;

    private void OnEnable()
    {
        Player.OnMagnetPull += MagnetON;
        Player.OnMagnetStop += MagnetOFF;
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if(_magnetized)
        {
            float step =_speed * 2 * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, _player.position, step);
        }
        else
            transform.Translate(Vector3.back * _speed * Time.deltaTime);

        if (transform.position.z < -17)
            Destroy(gameObject);

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out Player player))
        {
            player.PowerUp(_type);            
            AudioManager.Instance.PlaySFX(0, 0);
            Destroy(gameObject);
        }
    }

    private void MagnetON()
    {
        if (gameObject.activeSelf)
            _magnetized = true;
        else
            _magnetized = false;
    }

    private void MagnetOFF()
    {
        _magnetized = false;
    }

    private void OnDisable()
    {
        Player.OnMagnetPull -= MagnetON;
        Player.OnMagnetStop -= MagnetOFF;
    }
}
