using Unity.Mathematics;
using UnityEngine;
using CesiumForUnity;
using UnityEditor;
using Unity.VisualScripting;

/// <summary>
/// Pan by mouse dragging.
/// </summary>
[CreateAssetMenu(menuName = "CesiumCameraAction/Pan", fileName = "CesiumPanAction")]
public class CesiumPanAction : CesiumCameraAction
{
    [SerializeField] private int mouseButtonIndex = 0;
    [SerializeField] private float rotationLerp = 0.5f;
    [SerializeField] private double inertiaCoefficient = 0.5D;
    [SerializeField] private double maxAngularSpeed = 1000D;
    [SerializeField] private float speed = 5f;

    Vector3 direction = Vector3.zero;

    CameraInput.PointerType pointerType;
    bool isActive;
    bool isMoved;
    bool isDragStopped;
    bool isTilesCollisionOff = false;
    double3 beginDragPosUS;
    Quaterniond lastRotation;

    double3 initCenter;
    float accAngle = 0f;

    double3 previousPos;

    protected override bool IsTriggered(ICesiumCameraContext context)
    {
        return (context.Input.mouse.CanUse(mouseButtonIndex) || context.Input.oneTouch.CanUse());
    }

    protected override void OnBegin(ICesiumCameraContext context)
    {
        pointerType = context.Input.mouse.CanUse(mouseButtonIndex) ? CameraInput.PointerType.Mouse : CameraInput.PointerType.Touch;
        context.Input.mouse.Use(mouseButtonIndex);
        context.Input.oneTouch.Use();
        isActive = CesiumUtils.RayCastGlobe(context.Camera, context.Input.mouse.BeginPos, out beginDragPosUS);

        CesiumUtils.RayCastGlobe(context.Camera, context.Input.mouse.BeginPos, out previousPos);

        isMoved = isDragStopped = false;

        initCenter = context.Georeference.TransformEarthCenteredEarthFixedPositionToUnity(new double3(0.0));
        accAngle = 0f;

        double3 hitPos = new double3(0, 0, 0);
        var gotHit = CesiumUtils.RayCastGlobe(Camera.main, context.Input.mouse.BeginPos, out hitPos);
        if (gotHit)
        {
            Vector3 f_hitPos = new Vector3((float)hitPos.x, (float)hitPos.y, (float)hitPos.z);
            Vector3 diffVector = f_hitPos - context.Camera.transform.position;
            float distance = diffVector.magnitude;
            direction = diffVector / distance;
        }
        if (!isActive)
            return;
        lastRotation = Quaterniond.identity;
    }

    protected override bool OnUpdate(ICesiumCameraContext context)
    {

        // get the current position on the globe
        double3 currentPos;
        CesiumUtils.RayCastGlobe(context.Camera, context.Input.mouse.Position, out currentPos);

        double3 lastPos;
        CesiumUtils.RayCastGlobe(context.Camera, context.Input.mouse.LastPos, out lastPos);

        double3 currentCenter = context.Georeference.TransformEarthCenteredEarthFixedPositionToUnity(new double3(0.0));

        // get the axis vector
        var begin = (currentCenter - lastPos);
        var end = (currentCenter - currentPos);
        var axis = Vector3.Cross((Vector3)(float3)begin, (Vector3)(float3)end);

        //Debug.Log("axis: " + axis + " begin: " + begin + " and end: " + end);
        //Debug.Log("lastPos: " + lastPos + " currentPos: " + currentPos + " and current center: " + currentCenter);
        //var angle = Vector3.Angle((float3) begin.normalized, (float3) end.normalized);
        var angle = SignedAngle(begin, end, (double3)(float3)axis);
        float speed = (float)-angle;
        TestRotateAround((Vector3)(float3)currentCenter, axis, speed, context);

        //Debug.Log("diff: " + diff + " and currentCenter: " + initCenter + " and new center: " + newCenter);
        Debug.DrawRay((float3)currentCenter, axis * 5000f, Color.red, 10000f);

        return !Input.GetMouseButton(0);
    }

    double Angle(double3 from, double3 to)
    {
        var fromDir = math.normalize(from);
        var toDir = math.normalize(to);
        var dot = math.clamp(math.dot(fromDir, toDir), -1D, 1D);
        return math.degrees(math.acos(dot));
    }

    double SignedAngle(double3 from, double3 to, double3 axis)
    {
        var unsignedAngle = Angle(from, to);
        var cross = math.cross(from, to);
        var sign = math.sign(math.dot(cross, axis));
        return unsignedAngle * sign;
    }

    private void TestRotateAround(Vector3 center, Vector3 axis, float angle, ICesiumCameraContext context)
    {
        Vector3 pos = context.Camera.transform.position;
        Quaternion rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
        Vector3 dir = pos - center; // find current direction relative to center
        dir = rot * dir; // rotate the direction

        context.Camera.transform.position = center + dir; // define new position
        // rotate object to keep looking at the center:
        //Quaternion myRot = context.Camera.transform.rotation;
        //context.Camera.transform.rotation *= Quaternion.Inverse(myRot) * rot * myRot;
    }

    protected override void OnEnd(ICesiumCameraContext context)
    {
        base.OnEnd(context);

    }
    private bool UpdateInertia(ICesiumCameraContext context)
    {
        isDragStopped |= !context.Input.GetPointer(pointerType).IsStarted;

        if (!isMoved)
            return isDragStopped;

        lastRotation.ToAngleAxis(out var angle, out var axis);

        angle = math.min(angle, maxAngularSpeed * Time.deltaTime);

        var delta = Time.deltaTime * inertiaCoefficient;

        if (angle < delta)
            return isDragStopped;

        var rotation = Quaterniond.AngleAxis(angle - delta, axis);
        CameraUtils.RotateAroundGlobeCenter(context.Georeference, context.GlobeAnchor, context.Camera.transform, rotation);
        lastRotation = rotation;

        return false;
    }
}