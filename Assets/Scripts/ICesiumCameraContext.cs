using UnityEngine;
using CesiumForUnity;

public interface ICesiumCameraContext
{
    public Camera Camera { get; }
    public CesiumCameraAction CesiumCameraAction { get; set; }
    public CesiumCameraAction[] CesiumCameraActions { get; }
    public CameraInput Input { get; }

    public CesiumGeoreference Georeference { get; }

    public CesiumGlobeAnchor GlobeAnchor { get; }
}
