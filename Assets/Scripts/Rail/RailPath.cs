using System.Collections.Generic;
using UnityEngine;

public class RailPath : MonoBehaviour
{
    public List<RailNode> nodes = new();

    public Vector3 ProjectPoint(
        Vector3 worldPos,
        out float width
    )
    {
        Vector3 closest = Vector3.zero;
        float minDist = float.MaxValue;
        width = 1f;

        for (int i = 0; i < nodes.Count - 1; i++)
        {
            Vector3 a = transform.TransformPoint(nodes[i].position);
            Vector3 b = transform.TransformPoint(nodes[i + 1].position);

            Vector3 p = ClosestPointOnSegment(a, b, worldPos);
            float d = (worldPos - p).sqrMagnitude;

            if (d < minDist)
            {
                minDist = d;
                closest = p;

                float t = Vector3.Distance(a, p) / Vector3.Distance(a, b);
                width = Mathf.Lerp(
                    nodes[i].halfWidth,
                    nodes[i + 1].halfWidth,
                    t
                );
            }
        }

        return closest;
    }

    Vector3 ClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 p)
    {
        Vector3 ab = b - a;
        float t = Vector3.Dot(p - a, ab) / ab.sqrMagnitude;
        t = Mathf.Clamp01(t);
        return a + ab * t;
    }
}
[System.Serializable]
public class RailNode
{
    public Vector3 position;
    public float halfWidth = 1f;
}