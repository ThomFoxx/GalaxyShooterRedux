using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class Thrusters : MonoBehaviour
{
    [SerializeField]
    private Vector3 _scale;
    [SerializeField]
    private Vector3 _position;
    [SerializeField]
    private VisualEffect _thrusterFlame;
    [SerializeField]
    private Texture2D[] _animationFrames;
    private int _thrustMode = 0; // -1 = Reverse, 0 = Zero, 1 = Forward
    
   


    void Start()
    {
        _thrusterFlame.transform.localScale = _scale;
        _position = transform.localPosition;
        StartCoroutine(AnimateThruster());
    }

    public void ForwardThrust()
    {
        if (_thrustMode != 1)
        {
            _thrusterFlame.transform.localScale = _scale * 2;
            transform.localPosition = new Vector3(0, _position.y, _position.z-.5f);
            _thrustMode = 1;
        }
    }

    public void ZeroThrust()
    {
        if (_thrustMode != 0)
        {
            _thrusterFlame.transform.localScale = _scale;
            transform.localPosition = _position;
            _thrustMode = 0;
        }
    }

    public void ReverseThrust()
    {
        if (_thrustMode != -1)
        {
            _thrusterFlame.transform.localScale = _scale * .5f;
            transform.localPosition = new Vector3(_position.x, _position.y, _position.z + .25f); ;
            _thrustMode = -1;
        }
    }


    IEnumerator AnimateThruster()
    {
        int frame = 0;
        while(true)
        {
            yield return new WaitForEndOfFrame();
            _thrusterFlame.SetTexture("_thruster", _animationFrames[frame]);
            frame++;
            if (frame > _animationFrames.Length - 1)
                frame = 0;
        }
    }
}
