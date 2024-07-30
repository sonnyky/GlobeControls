using CesiumForUnity;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public static class CameraUtils
{
    /// <summary>
    /// Rotates the transform around the center of the globe.
    /// </summary>
    /// <param name="transform">HPTransform component.</param>
    /// <param name="rotation">The rotation in universe space.</param>
    public static void RotateAroundGlobeCenter(CesiumGeoreference geo, CesiumGlobeAnchor anchor, Transform transform, Quaterniond rotation)
    {
        RotateAroundPivot(geo, anchor, transform, rotation, double3.zero);
    }

    public static void RotateAroundPivot(CesiumGeoreference geo, CesiumGlobeAnchor anchor, Transform transform, Quaterniond rotation, double3 pivot)
    {
        var unityPos = geo.TransformEarthCenteredEarthFixedPositionToUnity(anchor.positionGlobeFixed);
        var lastCamPosUS = unityPos;
        var pivotToCamPosUS = lastCamPosUS - pivot;
        var newPivotToCamPosUS = rotation * pivotToCamPosUS;
        var newCamPosUS = pivot + newPivotToCamPosUS;

        anchor.positionGlobeFixed = geo.TransformUnityPositionToEarthCenteredEarthFixed( newCamPosUS);
        Rotate(geo, anchor, transform, rotation);
    }

    
    public static void Rotate(CesiumGeoreference geo, CesiumGlobeAnchor anchor, Transform transform, Quaterniond rotation)
    {
        SetRotation(geo, anchor, transform, (Quaternion)rotation * anchor.rotationEastUpNorth);
    }

    
    public static void SetPositionRotation(CesiumGeoreference geo, CesiumGlobeAnchor anchor, Transform transform, quaternion rotation)
    {
        SetRotation(geo, anchor, transform, rotation);
    }

    public static void SetRotation(CesiumGeoreference geo, CesiumGlobeAnchor anchor, Transform transform, quaternion rotation)
    {
        anchor.rotationEastUpNorth = rotation;
        var unityPos = geo.TransformEarthCenteredEarthFixedPositionToUnity(anchor.positionGlobeFixed);

        var globeCenterToCamDirUS = (float3)math.normalize(unityPos);

        if (math.abs(math.dot(transform.right, globeCenterToCamDirUS)) > 0.01D
            && math.abs(math.dot(transform.forward, globeCenterToCamDirUS)) < 0.9D)
        {
            var camForwardUS = transform.forward;
            var fixedCamRightUS = math.cross(globeCenterToCamDirUS, camForwardUS);
            var fixedCamUpUS = math.cross(camForwardUS, fixedCamRightUS);
            anchor.rotationEastUpNorth = quaternion.LookRotation(camForwardUS, fixedCamUpUS);
        }
    }

    /// <summary>
    /// Redirects the forward direction to the center of globe when the transform is higher than a given elevation.
    /// </summary>
    /// <param name="transform">HPTransform component.</param>
    /// <param name="startRecenterElevation">The elevation to tell if the recenter should be started.</param>
    /// <param name="recenterSpeed">The rotation speed for recenter.</param>
    /// <param name="lerp">Interpolation parameter with the range [0, 1].</param>
    public static void RecenterCameraRotation(CesiumGeoreference geo, CesiumGlobeAnchor anchor, Transform transform, double startRecenterElevation, double recenterSpeed, float lerp)
    {
        var coordinates = anchor.longitudeLatitudeHeight;

        if (coordinates.z >= startRecenterElevation)
        {
            var UpVector = new double3(transform.up.x, transform.up.y, transform.up.z);
            var angleUp = Angle(UpVector, -anchor.positionGlobeFixed);
            var pitchAngle = math.clamp(recenterSpeed, 0, angleUp - 90D);
            var pitchAxis = new double3(transform.right.x, transform.right.y, transform.right.z);
            var pitchRotation = Quaterniond.AngleAxis(pitchAngle, pitchAxis);
            pitchRotation = Quaterniond.Lerp(Quaterniond.identity, pitchRotation, lerp);
            Rotate(geo, anchor, transform, pitchRotation);
        }
    }

    static double Angle(double3 from, double3 to)
    {
        var fromDir = math.normalize(from);
        var toDir = math.normalize(to);
        var dot = math.clamp(math.dot(fromDir, toDir), -1D, 1D);
        return math.degrees(math.acos(dot));
    }
}
