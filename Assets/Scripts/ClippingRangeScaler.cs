using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CesiumForUnity;

[RequireComponent(typeof(Camera))]
public class ClippingRangeScaler : MonoBehaviour
{
    [SerializeField]
    private bool useCesiumRebase = true;
    [SerializeField]
    private float nearMin = 1;
    [SerializeField]
    private float nearMax = 1000;
    [SerializeField]
    private float farMin = 100000;
    [SerializeField]
    private float farMax = 1e+09f;

    private float _altitudeMax = 3e+07f;
    private float _currentAltitude = 0;
    private CesiumGlobeAnchor _anchor;

    private Camera _cam;

    private void Awake()
    {
        _cam = GetComponent<Camera>();
        _anchor = GetComponent<CesiumGlobeAnchor>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (useCesiumRebase && _anchor != null)
            _currentAltitude = (float)_anchor.height;
        else
            _currentAltitude = Camera.main.transform.position.magnitude;
        var multiplier = _currentAltitude / _altitudeMax;
        _cam.nearClipPlane = Mathf.Clamp(nearMax * multiplier, nearMin, nearMax);
        _cam.farClipPlane = Mathf.Clamp(farMax * multiplier, farMin, farMax);
    }
}
