using UnityEngine;

public class VissionConeHandler : MonoBehaviour
{
    private float initialZSize;
    public float defaultZScale = 1.0f; // Set this to your 'full' length scale

    private void Start()
    {
        // Get the actual length of the mesh at scale 1
        Renderer rend = GetComponent<Renderer>();
        initialZSize = rend.bounds.size.z / transform.localScale.z;
    }

    // Use Stay to keep the scale consistent while touching
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Untagged")) // Recommended: filter what can block vision
        {
            Vector3 contactPoint = other.ClosestPoint(transform.position);
            float distance = Vector3.Distance(transform.position, contactPoint);

            // Add a tiny buffer (1.05f) so the trigger stays "inside" the wall
            float newScaleZ = (distance / initialZSize) * 1.1f;

            transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y, newScaleZ);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Reset to original size when nothing is blocking
        transform.localScale = new Vector3(0.5f, 1, defaultZScale);
    }
}
