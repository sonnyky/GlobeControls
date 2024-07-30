using Unity.Mathematics;
using UnityEngine;
using CesiumForUnity;

public static class CesiumUtils
{
    /// <summary>
    /// Checks that the clicked position is on the globe
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="positionSS"></param>
    /// <param name="positionUS"></param>
    /// <returns>Returns true if the clicked position intersects with the globe</returns>
    public static bool RayCastGlobe(Camera camera, float3 positionSS, out double3 positionUS)
    {
        // only consider near < 120000 position since we will be moving the camera only on short range
        var result = RayCast(camera, positionSS, out var positionWS);
        positionUS = result ? positionWS : double3.zero;
        return result;

    }

    public static bool RayCast(Camera camera, float3 positionSS, out float3 positionWS)
    {
        var ray = camera.ScreenPointToRay(positionSS);

        if (Physics.Raycast(ray, out var hitInfo))
        {
            positionWS = hitInfo.point;
            return true;
        }
        else
        {
            positionWS = float3.zero;
            return false;
        }
    }

    public static Vector3 GetGeodesicUpDirection(CesiumGeoreference _geo, CesiumGlobeAnchor _anc)
    {
        if (_geo != null)
        {
            double3 positionECEF = new double3()
            {
                x = _anc.ecefX,
                y = _anc.ecefY,
                z = _anc.ecefZ,
            };
            double3 upECEF = CesiumWgs84Ellipsoid.GeodeticSurfaceNormal(positionECEF);
            double3 upUnity =
                _geo.TransformEarthCenteredEarthFixedDirectionToUnity(upECEF);

            return (float3)upUnity;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public static bool RaycastTowardsEarthCenter(CesiumGeoreference _geo, Vector3 startPos, out float hitDistance)
    {
        double3 center =
            _geo.TransformEarthCenteredEarthFixedPositionToUnity(new double3(0.0));

        RaycastHit hitInfo;
        if (Physics.Linecast(startPos, (float3)center, out hitInfo))
        {
            hitDistance = Vector3.Distance(startPos, hitInfo.point);
            return true;
        }

        hitDistance = 0.0f;
        return false;
    }

}
