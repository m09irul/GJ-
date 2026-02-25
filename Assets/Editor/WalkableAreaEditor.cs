using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(WalkableArea))]
public class WalkableAreaEditor : Editor
{
    WalkableArea area;
    Transform handleTransform;
    Quaternion handleRotation;

    void OnEnable()
    {
        area = (WalkableArea)target;
        handleTransform = area.transform;
        handleRotation = Tools.pivotRotation == PivotRotation.Local
            ? handleTransform.rotation
            : Quaternion.identity;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Scene Controls:\n" +
            "• Shift + Click → Add point\n" +
            "• Drag handle → Move point\n" +
            "• Ctrl + Click on point → Remove\n",
            MessageType.Info
        );
    }

    void OnSceneGUI()
    {
        if (area.points == null)
            return;

        Event e = Event.current;

        // Draw & move existing points
        for (int i = 0; i < area.points.Count; i++)
        {
            Vector3 worldPos = handleTransform.TransformPoint(area.points[i]);

            EditorGUI.BeginChangeCheck();
            Vector3 newWorldPos = Handles.PositionHandle(worldPos, handleRotation);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(area, "Move Walkable Area Point");
                area.points[i] = handleTransform.InverseTransformPoint(newWorldPos);
                EditorUtility.SetDirty(area);
            }

            // Ctrl + Click to remove
            float size = HandleUtility.GetHandleSize(worldPos) * 0.15f;
            if (e.control && e.type == EventType.MouseDown)
            {
                if (Vector2.Distance(
                        HandleUtility.WorldToGUIPoint(worldPos),
                        e.mousePosition
                    ) < 12f)
                {
                    Undo.RecordObject(area, "Remove Walkable Area Point");
                    area.points.RemoveAt(i);
                    EditorUtility.SetDirty(area);
                    e.Use();
                    return;
                }
            }

            Handles.Label(worldPos + Vector3.up * 0.2f, i.ToString());
        }

        // ---------- DRAW BOUNDARY LINES ----------
        if (area.points.Count >= 2)
        {
            Handles.color = Color.yellow;

            Vector3[] worldPoints = new Vector3[area.points.Count + 1];

            for (int i = 0; i < area.points.Count; i++)
            {
                worldPoints[i] =
                    handleTransform.TransformPoint(area.points[i]);
            }

            // close the loop
            worldPoints[area.points.Count] = worldPoints[0];

            Handles.DrawAAPolyLine(4f, worldPoints);
        }
        // ---------- DRAW FILLED AREA ----------
        if (area.points.Count >= 3)
        {
            Handles.color = new Color(1f, 1f, 0f, 0.08f);

            Vector3 center = Vector3.zero;
            foreach (var p in area.points)
                center += handleTransform.TransformPoint(p);
            center /= area.points.Count;

            for (int i = 0; i < area.points.Count; i++)
            {
                Vector3 a = handleTransform.TransformPoint(area.points[i]);
                Vector3 b = handleTransform.TransformPoint(
                    area.points[(i + 1) % area.points.Count]
                );

                Handles.DrawAAConvexPolygon(center, a, b);
            }
        }

        // Shift + Click to add new point on XZ plane
        if (e.alt && e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            Plane plane = new Plane(Vector3.up, handleTransform.position);

            if (plane.Raycast(ray, out float enter))
            {
                Vector3 hit = ray.GetPoint(enter);

                Undo.RecordObject(area, "Add Walkable Area Point");
                area.points.Add(handleTransform.InverseTransformPoint(hit));
                EditorUtility.SetDirty(area);

                e.Use();
            }
        }
    }
}
