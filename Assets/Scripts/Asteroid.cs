using UnityEngine;

public class Asteroid : MonoBehaviour
{
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    [Range(0f, 1f)]
    private float _xAxis, _yAxis, _zAxis;
    [SerializeField]
    private bool _isStarterAsteroid = false;

    void Start()
    {
        if (_xAxis == 0 && _yAxis == 0 && _zAxis == 0)
        {
            _xAxis = Random.Range(0f, 1f);
            _yAxis = Random.Range(0f, 1f);
            _zAxis = Random.Range(0f, 1f);
        }
    }

    void Update()
    {
        transform.Rotate(_xAxis, _yAxis, _zAxis, Space.Self);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent.TryGetComponent(out Laser laser))
        {
            laser.SendToPool();
            SpawnManager.Instance.SpawnExplosion(transform.position);
            PlaySFX();
            _collider.enabled = false;
            Destroy(this.gameObject, .6f);
            if (_isStarterAsteroid)
            {
                UIManager.Instance.UpdateWaveDisplay(1);
                SpawnManager.Instance.StartSpawning();
            }
        }
    }

    private void PlaySFX()
    {
        AudioManager.Instance.PlaySFX(1,0);
    }
}
