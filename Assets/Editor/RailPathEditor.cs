using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RailPath))]
public class RailPathEditor : Editor
{
    RailPath rail;

    void OnEnable()
    {
        rail = (RailPath)target;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(8);
        GUILayout.Label("Scene Controls", EditorStyles.boldLabel);
        GUILayout.Label("SHIFT + Click : Add node");
        GUILayout.Label("Move Handle  : Move node");
        GUILayout.Label("Arrow Handle : Change width");
    }

    void OnSceneGUI()
    {
        Event e = Event.current;

        HandleAddNode(e);
        DrawNodes();
        DrawCorridor();
    }

    // ---------------- ADD NODE ----------------
    void HandleAddNode(Event e)
    {
        if (e.type == EventType.MouseDown && e.shift && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Undo.RecordObject(rail, "Add Rail Node");

                rail.nodes.Add(new RailNode
                {
                    position = rail.transform.InverseTransformPoint(hit.point),
                    halfWidth = 1f
                });

                EditorUtility.SetDirty(rail);
                e.Use();
            }
        }
    }

    // ---------------- DRAW & EDIT NODES ----------------
    void DrawNodes()
    {
        for (int i = 0; i < rail.nodes.Count; i++)
        {
            RailNode node = rail.nodes[i];
            Vector3 worldPos = WorldPos(i);

            // ---- MOVE NODE ----
            EditorGUI.BeginChangeCheck();
            Vector3 newPos = Handles.PositionHandle(worldPos, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(rail, "Move Rail Node");
                node.position = rail.transform.InverseTransformPoint(newPos);
                EditorUtility.SetDirty(rail);
            }

            // ---- WIDTH HANDLE (SIDEWAYS, NOT RADIUS) ----
            Vector3 dir = GetDirection(i);
            Vector3 side = Vector3.Cross(Vector3.up, dir).normalized;

            Vector3 rightPoint = worldPos + side * node.halfWidth;

            EditorGUI.BeginChangeCheck();
            Vector3 newRight = Handles.Slider(
                rightPoint,
                side,
                HandleUtility.GetHandleSize(rightPoint) * 0.2f,
                Handles.ArrowHandleCap,
                0f
            );

            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(rail, "Change Rail Width");
                node.halfWidth = Mathf.Max(
                    0f,
                    Vector3.Dot(newRight - worldPos, side)
                );
                EditorUtility.SetDirty(rail);
            }

            // ---- VISUAL WIDTH LINE ----
            Handles.color = Color.yellow;
            Handles.DrawLine(
                worldPos - side * node.halfWidth,
                worldPos + side * node.halfWidth
            );

            Handles.Label(worldPos + Vector3.up * 0.2f, $"Node {i}");
        }
    }

    // ---------------- DRAW CORRIDOR ----------------
    void DrawCorridor()
    {
        Handles.color = Color.cyan;

        for (int i = 0; i < rail.nodes.Count - 1; i++)
        {
            Vector3 a = WorldPos(i);
            Vector3 b = WorldPos(i + 1);

            Vector3 dirA = GetDirection(i);
            Vector3 dirB = GetDirection(i + 1);

            Vector3 sideA = Vector3.Cross(Vector3.up, dirA).normalized;
            Vector3 sideB = Vector3.Cross(Vector3.up, dirB).normalized;

            float wA = rail.nodes[i].halfWidth;
            float wB = rail.nodes[i + 1].halfWidth;

            Handles.DrawLine(a, b);
            Handles.DrawLine(a - sideA * wA, b - sideB * wB);
            Handles.DrawLine(a + sideA * wA, b + sideB * wB);
        }
    }

    // ---------------- HELPERS ----------------
    Vector3 WorldPos(int i)
    {
        return rail.transform.TransformPoint(rail.nodes[i].position);
    }

    Vector3 GetDirection(int i)
    {
        if (rail.nodes.Count < 2)
            return Vector3.forward;

        if (i == 0)
            return (WorldPos(1) - WorldPos(0)).normalized;

        if (i == rail.nodes.Count - 1)
            return (WorldPos(i) - WorldPos(i - 1)).normalized;

        return (WorldPos(i + 1) - WorldPos(i - 1)).normalized;
    }
}