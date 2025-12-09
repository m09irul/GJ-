using UnityEngine;

public class GuidingFlutterBly : MonoBehaviour
{
    public Transform player;
    public Transform target;
    public ParticleSystem butterflies;

    public float behindOffset = 1.5f;
    public float frontOffset = 1.5f;
    public float burstDistance = 8f;
    public float moveSpeed = 3f;
    public float fadeTime = 2f;

    private Vector3 frontPos;
    private Vector3 burstEnd;
    private float t = 0;

    private enum State { MoveToFront, CurveBurst, Fade }
    private State state;

    void Start()
    {
        // Start behind player
        transform.position = player.position - player.forward * behindOffset;

        // Where to move first
        frontPos = player.position + player.forward * frontOffset;

        state = State.MoveToFront;
    }

    void Update()
    {
        switch (state)
        {
            case State.MoveToFront:
                MoveToFront();
                break;

            case State.CurveBurst:
                DoCurveBurst();
                break;

            case State.Fade:
                FadeOut();
                break;
        }
    }

    void MoveToFront()
    {
        transform.position = Vector3.MoveTowards(
            transform.position,
            frontPos,
            moveSpeed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, frontPos) < 0.1f)
        {
            // Look toward the destination
            transform.LookAt(target.position);

            // Decide burst end point (straight direction)
            burstEnd = transform.position + transform.forward * burstDistance;

            // Start curved burst
            t = 0;
            state = State.CurveBurst;
        }
    }

    void DoCurveBurst()
    {
        t += Time.deltaTime * (moveSpeed * 0.4f);

        // Control points
        Vector3 p0 = frontPos;

        // Curve influence sideways
        Vector3 side = Vector3.Cross(transform.forward, Vector3.up);
        float sideStrength = 2f;  // how wide the curve is
        Vector3 p1 = p0 + transform.forward * (burstDistance * 0.3f) + side * sideStrength;

        // Lift upward a little (makes butterflies float)
        Vector3 p2 = burstEnd + Vector3.up * 1.5f;

        Vector3 newPos = Bezier3(p0, p1, p2, t);
        transform.position = newPos;

        // Rotate along curve for smoother direction
        Vector3 forward = Bezier3Tangent(p0, p1, p2, t).normalized;
        if (forward.sqrMagnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(forward), 0.1f);

        if (t >= 1f)
        {
            state = State.Fade;
        }
    }

    // Quadratic Bezier
    Vector3 Bezier3(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return Mathf.Pow(1 - t, 2) * a +
               2 * (1 - t) * t * b +
               t * t * c;
    }

    // Derivative (for rotation)
    Vector3 Bezier3Tangent(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return 2 * (1 - t) * (b - a) + 2 * t * (c - b);
    }

    float fadeT = 0;
    void FadeOut()
    {
        fadeT += Time.deltaTime;

        var emission = butterflies.emission;
        emission.rateOverTime = Mathf.Lerp(50, 0, fadeT / fadeTime);

        if (fadeT >= fadeTime)
            Destroy(gameObject);
    }
}
