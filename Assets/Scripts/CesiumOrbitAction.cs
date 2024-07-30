using Unity.Mathematics;
using UnityEngine;
using CesiumForUnity;

/// <summary>
/// Orbit by mouse dragging.
/// </summary>
[CreateAssetMenu(menuName = "CesiumCameraAction/Orbit", fileName = "CesiumOrbitAction")]
public class CesiumOrbitAction : CesiumCameraAction
{
    [SerializeField] private int mouseButtonIndex = 2;
    [SerializeField] private int alternativeMouseButtonIndex = 0;
    [SerializeField]
    private KeyCode[] alternativeKeyCodes = new KeyCode[]
        { KeyCode.LeftShift, KeyCode.RightShift, KeyCode.LeftMeta, KeyCode.RightMeta };
    [SerializeField] private float pitchSensitivity = 200f;
    [SerializeField] private double pitchAngleMin = -60D;
    [SerializeField] private double pitchAngleMax = 60D;
    [SerializeField] private float yawSensitivity = 400f;
    [SerializeField] private float inertiaCoefficient = 10f;

    bool isActive;
    bool isMoved;
    bool isDragStopped;
    double3 beginDragPosUS;
    float3 lastDragDeltaSS;

    protected override bool IsTriggered(ICesiumCameraContext context)
    {
        Debug.Log(context.Input.mouse.CanUse(mouseButtonIndex));
        return context.Input.mouse.CanUse(mouseButtonIndex);
    }

    protected override void OnBegin(ICesiumCameraContext context)
    {
        context.Input.mouse.Use(mouseButtonIndex);
        isActive = CesiumUtils.RayCastGlobe(context.Camera, context.Input.mouse.BeginPos, out beginDragPosUS);
        isMoved = isDragStopped = false;

        if (!isActive)
            return;
        lastDragDeltaSS = float3.zero;
    }

    protected override bool OnUpdate(ICesiumCameraContext context)
    {
        isActive &= context.Input.mouse.IsStarted;

        if (!isActive)
            return UpdateInertia(context);

        isMoved = true;

        var dragDeltaSS = context.Input.mouse.Position - context.Input.mouse.LastPos;
        UpdateDragDelta(context, dragDeltaSS);
        lastDragDeltaSS = dragDeltaSS;

        return false;
    }

    protected override void OnEnd(ICesiumCameraContext context)
    {
        base.OnEnd(context);
    }

    private bool UpdateInertia(ICesiumCameraContext context)
    {
        isDragStopped |= !context.Input.mouse.IsStarted;

        if (!isMoved)
            return isDragStopped;

        var dragDeltaSS = math.lerp(lastDragDeltaSS, float3.zero, Time.deltaTime * inertiaCoefficient);

        if (math.lengthsq(dragDeltaSS) < 1f)
            return isDragStopped;

        UpdateDragDelta(context, dragDeltaSS);
        lastDragDeltaSS = dragDeltaSS;

        return false;
    }

    private void UpdateDragDelta(ICesiumCameraContext context, float3 dragDeltaSS)
    {
        Vector3 dragPos = new Vector3((float)beginDragPosUS.x, (float)beginDragPosUS.y, (float)beginDragPosUS.z);
       
        Vector3 axis = Vector3.up * dragDeltaSS.x + context.Camera.transform.right * dragDeltaSS.y * -1;

        TestRotateAround(dragPos, axis, 0.6f, context);
        //Debug.DrawRay((float3)center, (context.Camera.transform.position - (Vector3)(float3)center), Color.red, 10000f);
    }

    private void TestRotateAround(Vector3 center, Vector3 axis, float angle, ICesiumCameraContext context)
    {
        Vector3 pos = context.Camera.transform.position;
        Quaternion rot = Quaternion.AngleAxis(angle, axis); // get the desired rotation
        Vector3 dir = pos - center; // find current direction relative to center
        dir = rot * dir; // rotate the direction

        context.Camera.transform.position = center + dir; // define new position
        // rotate object to keep looking at the center:
        Quaternion myRot = context.Camera.transform.rotation;
        context.Camera.transform.rotation *= Quaternion.Inverse(myRot) * rot * myRot;
    }
}
