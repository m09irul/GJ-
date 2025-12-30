using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private CinemachineVirtualCamera virtualCamera;

    [Header("Debug")]
    [SerializeField] private bool drawGizmos = true;
    [SerializeField] private Color gizmoColor = new Color(0f, 0.6f, 1f, 0.25f);

    private void Reset()
    {
        GetComponent<Collider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("cat")) return;

        CinemachineController.Instance.SetCamera(virtualCamera);
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!drawGizmos) return;

        Gizmos.color = gizmoColor;
        Collider col = GetComponent<Collider>();

        if (col is BoxCollider box)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
#endif
}
