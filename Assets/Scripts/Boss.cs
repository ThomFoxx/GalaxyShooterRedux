using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    [SerializeField]
    private int _gunTotal;
    private int _gunsDestroyed;
    private Animation _anim;

    public delegate void FireEnable();
    public static event FireEnable OnFireEnable;
    public delegate void FireDisable();
    public static event FireDisable OnFireDisable;

    private void OnEnable()
    {
        BossGun.OnEnemyDeath += GunDown;
        _anim = GetComponent<Animation>();
    }

    public void EnableFire()
    {
        if (OnFireEnable != null)
            OnFireEnable();
    }

    public void DisableFire()
    {
        if (OnFireDisable != null)
            OnFireDisable();
    }

    private void GunDown(int notUsed, Transform NotUsed)
    {
        _gunsDestroyed++;
        if (_gunsDestroyed == _gunTotal)
        {            
            BossExit();
            GameManager.Instance.GameOver(true);
            UIManager.Instance.TriggerGameOver();
            SpawnManager.Instance.StopSpawning();
        }
    }

    private void BossExit()
    {
        _anim.Play("BossExit");
    }


    private void OnDisable()
    {
        BossGun.OnEnemyDeath -= GunDown;
        Destroy(this.gameObject);
    }


}
