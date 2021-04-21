using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField]
    AudioSource _musicSource;
    [SerializeField]
    AudioSource _SFXSource;
    [SerializeField]
    AudioClip[] _music;
    [SerializeField]
    AudioClip[] _generalSFX;
    [SerializeField]
    AudioClip[] _explosionSFX;
    [SerializeField]
    AudioClip[] _laserSFX;


    private static AudioManager _instance;
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("Audio Manager is Null!!!");

            return _instance;
        }
    }

    private void Awake()
    {
        _instance = this;
    }

    public void PlaySFX(int SFXGroup, int ClipId)
    {
        switch (SFXGroup)
        {
            case 0:
                _SFXSource.PlayOneShot(_generalSFX[ClipId]);
                break;
            case 1:
                _SFXSource.PlayOneShot(_explosionSFX[ClipId]);
                break;
            case 2:
                _SFXSource.PlayOneShot(_laserSFX[ClipId]);
                break;
            default:
                Debug.LogError("SFX Group ID is invalid.");
                break;
        }
    }

    public void PlaySFX(int SFXGroup)
    {
        int RND = 0;
        switch (SFXGroup)
        {
            case 0:
                Debug.LogError("No Random Play Back on General SFX.");
                break;
            case 1:
                RND = Random.Range(0, _explosionSFX.Length);
                _SFXSource.PlayOneShot(_explosionSFX[RND]);
                break;
            case 2:
                RND = Random.Range(0, _laserSFX.Length);
                _SFXSource.PlayOneShot(_laserSFX[RND]);
                break;
            default:
                Debug.LogError("SFX Group ID is invalid.");
                break;
        }
    }

    public void PlayMusic(int ClipId, float volume)
    {
        _SFXSource.PlayOneShot(_music[ClipId], volume);
    }
}
