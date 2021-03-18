using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField]
    private float _speed = 5;

    // Start is called before the first frame update
    void Start()
    {
        //Take current position = new position vector3.zero
        transform.position = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        Movement();
    }

    private void Movement()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        transform.Translate(Vector3.right * horizontalInput * _speed * Time.deltaTime);
        transform.Translate(Vector3.forward * verticalInput * _speed * Time.deltaTime);

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
