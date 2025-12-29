using System.Collections.Generic;
using UnityEngine;

public class WalkableArea : MonoBehaviour
{
    public List<Vector3> points = new(); // XZ plane

    public Vector3 ClampPoint(Vector3 worldPos)
    {
        Vector3 local = transform.InverseTransformPoint(worldPos);
        Vector2 p = new(local.x, local.z);

        if (IsInside(p))
            return worldPos;

        Vector2 closest = ClosestPointOnPolygon(p);
        Vector3 clampedLocal = new(closest.x, local.y, closest.y);

        return transform.TransformPoint(clampedLocal);
    }

    // ------------------------

    bool IsInside(Vector2 p)
    {
        bool inside = false;
        for (int i = 0, j = points.Count - 1; i < points.Count; j = i++)
        {
            Vector2 a = new(points[i].x, points[i].z);
            Vector2 b = new(points[j].x, points[j].z);

            if (((a.y > p.y) != (b.y > p.y)) &&
                (p.x < (b.x - a.x) * (p.y - a.y) / (b.y - a.y) + a.x))
                inside = !inside;
        }
        return inside;
    }

    Vector2 ClosestPointOnPolygon(Vector2 p)
    {
        Vector2 best = p;
        float bestDist = float.MaxValue;

        for (int i = 0; i < points.Count; i++)
        {
            Vector2 a = new(points[i].x, points[i].z);
            Vector2 b = new(points[(i + 1) % points.Count].x,
                            points[(i + 1) % points.Count].z);

            Vector2 c = ClosestPointOnSegment(a, b, p);
            float d = (p - c).sqrMagnitude;

            if (d < bestDist)
            {
                bestDist = d;
                best = c;
            }
        }

        return best;
    }

    Vector2 ClosestPointOnSegment(Vector2 a, Vector2 b, Vector2 p)
    {
        Vector2 ab = b - a;
        float t = Vector2.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }
}
