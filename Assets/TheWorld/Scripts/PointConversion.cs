using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointConversion
{
    public static List<Vector3> PlaneToSphere(List<Vector2> plane)
    {
        var sphere = new List<Vector3>();

        foreach (var p in plane)
        {
            var uv = new Vector2(p.x * 0.5f, p.y);
            sphere.Add(UVToSphere(uv));
        }
        
        Vector3 UVToSphere(Vector2 uv)
        {
            uv = new Vector2(Mathf.Clamp01(uv.x), Mathf.Clamp01(uv.y));

            float lon = (uv.x / 0.5f - 1) * Mathf.PI;
            float lat = (uv.y - 0.5f) * Mathf.PI;

            float y = Mathf.Sin(lat);
            float r = Mathf.Cos(lat);
            float x = Mathf.Sin(lon) * r;
            float z = -Mathf.Cos(lon) * r;
            return new Vector3(x, y, z);
        }
        return sphere;
    }

    public static List<Vector2> SphereToPlane(List<Vector3> sphere, float xOffset = 0f, float yOffset = 0f)
    {
        var plane = new List<Vector2>();

        foreach (var s in sphere)
        {
            var uv = SphereToUV(s);
            plane.Add(new Vector2((uv.x + xOffset) * 2.0f, uv.y));
        }
        return plane;
    }
    
    public static List<Vector2> SphereToUV(List<Vector3> sphere, float xOffset = 0f, float yOffset = 0f)
    {
        var plane = new List<Vector2>();

        foreach (var s in sphere)
        {
            var uv = SphereToUV(s);
            plane.Add(new Vector2((uv.x + xOffset), uv.y));
        }
        return plane;
    }

    public static List<Vector4> TextureToSphere(List<Vector3> points, Texture2D colorMap)
    {
        var plane = new List<Vector4>();

        foreach (var p in points)
        {
            var uv = SphereToUV(p);
            plane.Add(colorMap.GetPixelBilinear(uv.x, uv.y));
        }
        return plane;
    }

    private static Vector3 SphereToUV(Vector3 point)
    {
        point.Normalize();

        float lat = Mathf.Asin(point.y);
        float lon = Mathf.Atan2(point.x, -point.z);

        float u = (lon / Mathf.PI + 1) * 0.5f;
        float v = lat / Mathf.PI + 0.5f;

        return new Vector2(Mathf.Clamp01(u), v);
    }
}
