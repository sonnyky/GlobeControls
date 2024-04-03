using Unity.Mathematics;
using UnityEngine;
using UnityEditor;

/// <summary>
/// Zoom by mouse scroll.
/// </summary>
[CreateAssetMenu(menuName = "CesiumCameraAction/Zoom", fileName = "CesiumZoomAction")]
public class CesiumZoomAction : CesiumCameraAction
{
    [SerializeField] private float zoomSensitivity = 0.01f;
    [SerializeField] private float zoomLerp = 0.1f;
    [SerializeField] private float rotationLerp = 1f;
    [SerializeField] private double minElevation = 50D;
    [SerializeField] private double maxElevation = 30_000_000D;
    [SerializeField] private double startRecenterElevation = 300_000D;
    [SerializeField] private double recenterSpeed = 2D;

    private float acceleration = 0f;
    private float idleTimer = 0f;
    private float scrollTime = 0f;
    private float idleTimeToBreakEvent = 0.5f; // after this duration passed without increments in scroll delta, we assume scroll event has ended
    float previousScrollDelta = 0f; //use this to check sudden change in scroll direction, which means the start of a new scroll event
    float accumulatedScrollDelta = 0f; // use this to check scroll inertia
    private float initialSpeed = 0f;
    private Vector3 direction = Vector3.zero;


    float3 beginScrollMousePositionSS;
    bool beginScrollOnEarth;
    double3 beginScrollPosUS;
    double targetElevation;
    double targetElevationLog;
    float signedDelta;

    protected override bool IsTriggered(ICesiumCameraContext context)
    {
        //Debug.Log((math.abs(context.Input.mouse.Scroll) > 1E-5f) + ", " + !GlobalValues.IsModalUIShown + ", " + GlobalValues.IsCameraLocked + " scrollOngoing: " + GlobalValues.scrollOngoing);
        return (math.abs(context.Input.mouse.Scroll) > 1E-5f);
    }

    double GetLogarithm(double value, double min, double max)
    {
        value = math.clamp(value, min, max);
        return math.log(value - min + 1) / math.log(max - min + 1);
    }

    double GetExponential(double value, double min, double max)
    {
        return math.pow(max - min + 1, value) + min - 1;
    }

    protected override void OnBegin(ICesiumCameraContext context)
    {
        Debug.Log("onbegin");
        beginScrollMousePositionSS = context.Input.mouse.Position;
        beginScrollOnEarth = CesiumUtils.RayCastGlobe(context.Camera, beginScrollMousePositionSS, out beginScrollPosUS);

        //var coordinates = Wgs84.GetGeodeticCoordinates(context.CesiumCameraTransform.UniversePosition);

        var coordinates = context.GlobeAnchor.longitudeLatitudeHeight;

        var beginValue = GetLogarithm(coordinates.z, minElevation, maxElevation);
        signedDelta = context.Input.mouse.Scroll;
        targetElevationLog = math.saturate(beginValue + signedDelta);
        targetElevation = GetExponential(targetElevationLog, minElevation, maxElevation);

        double3 hitPos = new double3(0, 0, 0);
        var gotHit = CesiumUtils.RayCastGlobe(Camera.main, beginScrollMousePositionSS, out hitPos);
        if (gotHit)
        {
            Vector3 f_hitPos = new Vector3((float)hitPos.x, (float)hitPos.y, (float)hitPos.z);
            Vector3 diffVector = f_hitPos - context.Camera.transform.position;
            float distance = diffVector.magnitude;
            direction = diffVector / distance;
            initialSpeed = (signedDelta * distance * 2);
            acceleration = signedDelta * distance;
            Debug.Log("Initial speed: " + initialSpeed + " and Distance: " + distance);
        }
        else
        {
            double3 center =
            context.Georeference.TransformEarthCenteredEarthFixedPositionToUnity(new double3(0.0));
            Vector3 diffVector = (Vector3)(float3)center - context.Camera.transform.position;
            float distance = diffVector.magnitude;
            direction = diffVector / distance;
        }
    }

    protected override bool OnUpdate(ICesiumCameraContext context)
    {
        var finished = UpdateElevation(context, zoomLerp);
        return finished;
    }

    protected override void OnEnd(ICesiumCameraContext context)
    {
        UpdateElevation(context, 1f);
        idleTimer = 0f;
        scrollTime = 0f;
        acceleration = 0f;
        initialSpeed = 0f;
    }

    private bool UpdateElevation(ICesiumCameraContext context, float lerp)
    {

        float currentHeight;
        CesiumUtils.RaycastTowardsEarthCenter(context.Georeference, context.Camera.transform.position, out currentHeight);

        float speed;
        speed = currentHeight * 0.2f;

        Vector3 nextPosition = context.Camera.transform.position + (direction * speed * signedDelta);
        float nextHeight;
        CesiumUtils.RaycastTowardsEarthCenter(context.Georeference, nextPosition, out nextHeight);

        context.Camera.transform.position = nextPosition;


        return true;
    }

    double3 LerpDouble(double3 input, double3 target, double t)
    {
        return input + ((target - input) * t);
    }
}
