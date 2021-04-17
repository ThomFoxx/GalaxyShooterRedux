using System.Collections;
using UnityEngine;

public class Teleporter : MonoBehaviour
{

    private float _scale = 1;
    [SerializeField]
    private Collider _collider;
    [SerializeField]
    private Vector2 _xLimits, _zLimits;
    private bool _isTeleporting = false;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RandomTeleport());
    }

    private IEnumerator RandomTeleport()
    {
        yield return new WaitForSeconds(2f);
        while (true)
        {
            if (!_isTeleporting)
            {
                float RNDX = Random.Range(_xLimits.x, _xLimits.y);
                float RNDZ = Random.Range(_zLimits.x, _zLimits.y);
                _isTeleporting = true;
                //StartCoroutine(Teleport(new Vector3(RNDX, 0, RNDZ)));
                yield return Teleport(new Vector3(RNDX, 0, RNDZ));
            }   
            //yield return new WaitForEndOfFrame();
        }
    }

    private IEnumerator Teleport(Vector3 NewPOS)
    {
        _collider.enabled = false;
        while (_scale > 0)
        {
            _scale -= Time.deltaTime;
            transform.localScale = new Vector3(_scale, 1, _scale);
            yield return new WaitForEndOfFrame();
        }
        if (_scale <= 0)
        {
            transform.position = NewPOS;
            _scale = 0;
            transform.localScale = new Vector3(_scale, 1, _scale);
        }
        while (_scale < 1)
        {
            _scale += Time.deltaTime;
            transform.localScale = new Vector3(_scale, 1, _scale);
            yield return new WaitForEndOfFrame();
        }
        if (_scale >= 1)
        {
            _scale = 1;
            transform.localScale = new Vector3(_scale, 1, _scale);
            _isTeleporting = false;
            _collider.enabled = true;
        }
    }
}