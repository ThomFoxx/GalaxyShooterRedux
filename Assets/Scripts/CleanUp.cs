using UnityEngine;

public class CleanUp : MonoBehaviour
{
    [SerializeField]
    [Range(0f,10f)]
    private float _cleanUpDelay;

    void Start()
    {
        Destroy(this.gameObject, _cleanUpDelay);
    }
}