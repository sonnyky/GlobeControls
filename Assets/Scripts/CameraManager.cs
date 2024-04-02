using System.Linq;
using UnityEngine;
using Unity.Mathematics;
using CesiumForUnity;
using UnityEngine.UIElements;

/// <summary>
/// Custom mouse/keyboard camera controls for geospatial globe that emulate closer to e.g. Google maps vs the default FlyCamera controls.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraManager : MonoBehaviour, ICesiumCameraContext
{
    /// <summary>
    /// When this is set to true, the controller automatically adjusts the clip planes
    /// based on the camera's altitude. For some use-cases, this adjustement may not
    /// be adequate.
    /// </summary>
    public bool setClipPlanes = true;

    /// <summary>
    /// Prevent the camera from going beneath the surface of the ground.
    /// </summary>
    public bool preventGoingUnderground = true;

    /// <summary>
    /// Minimum elevation for ground hit check.
    /// </summary>
    public float groundHitElevMin = -500;

    /// <summary>
    /// Maximum elevation for ground hit check.
    /// </summary>
    public float groundHitElevMax = 10_000;

    /// <summary>
    /// Minimum distance from camera to ground.
    /// </summary>
    public float minDistanceToGround = 10;

    /// <summary>
    /// SerializeField for <see cref="CameraAction"/>.
    /// </summary>
    public CesiumCameraAction[] cameraActions = new CesiumCameraAction[0];

    Camera m_Camera;
    readonly CameraInput m_Input = new CameraInput();
    CesiumCameraAction m_CameraAction;

    [SerializeField] CesiumGeoreference m_Georeference;
    [SerializeField] CesiumGlobeAnchor m_GlobeAnchor;

    public Camera Camera => m_Camera;

    public CesiumCameraAction CesiumCameraAction { get { return m_CameraAction; } set { m_CameraAction = value; } }

    public CesiumCameraAction[] CesiumCameraActions => cameraActions;

    CameraInput ICesiumCameraContext.Input => m_Input;

    CesiumGlobeAnchor ICesiumCameraContext.GlobeAnchor => m_GlobeAnchor;
    CesiumGeoreference ICesiumCameraContext.Georeference => m_Georeference;

    double m_CameraCurrentHeight;

    void Start()
    {
        m_Camera = GetComponent<Camera>();
        m_Input.Initialize();
        double3 camPos = m_GlobeAnchor.longitudeLatitudeHeight;
        m_CameraCurrentHeight = camPos.z;
        Debug.Log("initial height at camera manager: " + m_CameraCurrentHeight);
    }

    void Update()
    {
        m_Input.Update();
        CesiumCameraAction.Process(this);
    }
}
