using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraShake : MonoBehaviour
{
    

    private void OnEnable()
    {
        Player.OnPlayerDamaged += ShakeCamera;
    }

    private void ShakeCamera(int NotUsed)
    {
        StartCoroutine(Shake(.25f, .1f));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        Vector3 orignalPosition = transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float z = Random.Range(-1f, 1f) * magnitude;

            transform.position = new Vector3(x, orignalPosition.y, z);
            elapsed += Time.deltaTime;
            yield return 0;
        }
        transform.position = orignalPosition;
    }

    private void OnDisable()
    {
        Player.OnPlayerDamaged -= ShakeCamera;
    }
}
