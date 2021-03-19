using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private bool _horizontalFlight;
    [SerializeField]
    private float _speed = 5;
    private Vector3 _direction;
    [SerializeField]
    private GameObject _laserPrefab;
    [SerializeField]
    private float _laserOffset;
    private Transform _laserPool;
    [SerializeField]
    private bool _laserCanFire;
    [SerializeField]
    [Tooltip("Won't change edited during Play Mode")]
    private float _laserCoolDown = .25f;
    private WaitForSeconds _laserCoolDownTimer;

    private void OnEnable()
    {
        _laserCoolDownTimer = new WaitForSeconds(_laserCoolDown);
    }
    // Start is called before the first frame update
    void Start()
    {
        //_laserCoolDownTimer = new WaitForSeconds(_laserCoolDown);
        if (_laserPool == null)
            _laserPool = GameObject.Find("LaserPool").transform;
        //Take current position = new position vector3.zero
        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
        Weapon();
    }

    private void Movement()
    {
        float horizontalInput;
        float verticalInput;

        if (_horizontalFlight)
        { //Side scroll motion
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            _direction = new Vector3(0, verticalInput, horizontalInput);
            transform.Translate(_direction * _speed * Time.deltaTime);

            if (transform.position.z >= 3)
                transform.position = new Vector3(0, transform.position.y, 3);
            else if (transform.position.z <= -8.3f)
                transform.position = new Vector3(0, transform.position.y, -8.3f);
            if (transform.position.y >= 5.2f)
                transform.position = new Vector3(0, 5.2f, transform.position.z);
            else if (transform.position.y <= -3.7f)
                transform.position = new Vector3(0, -3.7f, transform.position.z);

        }
        else if (!_horizontalFlight)
        { //Top Down motion
            horizontalInput = Input.GetAxis("Horizontal");
            verticalInput = Input.GetAxis("Vertical");
            _direction = new Vector3(horizontalInput, 0, verticalInput);
            transform.Translate(_direction * _speed * Time.deltaTime);

            if (transform.position.z >= 0)
                transform.position = new Vector3(transform.position.x, 0, 0);
            else if (transform.position.z <= -4.5f)
                transform.position = new Vector3(transform.position.x, 0, -4.5f);
            if (transform.position.x >= 8.5f)
                transform.position = new Vector3(8.5f, 0, transform.position.z);
            else if (transform.position.x <= -8.5f)
                transform.position = new Vector3(-8.5f, 0, transform.position.z);
        }

    }

    private void Weapon()
    {
        if (Input.GetKeyDown(KeyCode.Space) && _laserCanFire)
        {
            Vector3 launch = transform.position + transform.TransformDirection(Vector3.forward) * _laserOffset;
            if (_laserPool.childCount < 1)
                Instantiate(_laserPrefab, launch, transform.rotation, this.transform);
            else
            {
                GameObject laserTemp = _laserPool.GetChild(0).gameObject;
                laserTemp.transform.parent = this.transform;
                laserTemp.transform.position = launch;
                laserTemp.transform.rotation = transform.rotation;
                laserTemp.GetComponent<Laser>().SetLastParent(this.transform);
                laserTemp.SetActive(true);
            }
            _laserCanFire = false;
            StartCoroutine(LaserReloadTimer());
        }
    }

    IEnumerator LaserReloadTimer()
    {
        while (!_laserCanFire)
        {
            yield return _laserCoolDownTimer;
            _laserCanFire = true;
        }
    }
}
