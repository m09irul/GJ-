using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class ThrowableItem : MonoBehaviour
{
    [Header("Throw Settings")]
    public float throwForce = 8f;
    public float arcForce = 4f;
    public float lifeTime = 5f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Throw(Vector3 direction)
    {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        Vector3 force =
            direction.normalized * throwForce +
            Vector3.up * arcForce;

        rb.AddForce(force, ForceMode.VelocityChange);

        Destroy(gameObject, lifeTime);
    }


}