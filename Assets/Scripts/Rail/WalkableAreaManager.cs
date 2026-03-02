using System.Collections.Generic;
using UnityEngine;

public class WalkableAreaManager : MonoBehaviour
{
    public List<WalkableArea> areas = new();
    public bool enableClamp = true;

    [Tooltip("Maximum vertical distance from player to polygon points to consider it reachable")]
    public float maxVerticalTolerance = 2f;

    /// <summary>
    /// Clamp player to nearest polygon in XZ, but only polygons near the player's Y
    /// </summary>
    public Vector3 ClampToNearestArea(Vector3 playerPos)
    {
        if (!enableClamp || areas.Count == 0)
            return playerPos;

        WalkableArea nearest = null;
        float bestDist = float.MaxValue;

        foreach (var area in areas)
        {
            // Check vertical distance from player to polygon points
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            foreach (var pt in area.points)
            {
                float y = pt.y + area.transform.position.y;
                minY = Mathf.Min(minY, y);
                maxY = Mathf.Max(maxY, y);
            }

            if (playerPos.y < minY - maxVerticalTolerance || playerPos.y > maxY + maxVerticalTolerance)
                continue; // polygon is too high or low, skip

            // Compute horizontal distance to polygon
            Vector3 clamped = area.ClampPoint(playerPos);
            float dist = Vector2.SqrMagnitude(
                new Vector2(playerPos.x, playerPos.z) - 
                new Vector2(clamped.x, clamped.z)
            );

            if (dist < bestDist)
            {
                bestDist = dist;
                nearest = area;
            }
        }

        if (nearest != null)
        {
            Vector3 clamped = nearest.ClampPoint(playerPos);
            clamped.y = playerPos.y; // preserve player Y
            return clamped;
        }

        return playerPos;
    }
}