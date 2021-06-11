using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MineDetector : MonoBehaviour
{
    [SerializeField]
    private GameObject _mine;

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Player player))
        {
            player.Damage();
            _mine.GetComponent<Mine>().Explosion();
        }
    }
}
