using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUp : MonoBehaviour
{
    [SerializeField]
    private float _speed;
    [SerializeField]
    [Tooltip("0 = Tripple Shot, 1 = Speed Boost, 2 = Shields")]
    private int _type;

    private void Update()
    {
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
}
