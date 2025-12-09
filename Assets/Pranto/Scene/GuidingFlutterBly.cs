using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class GuidingFlutterBly : MonoBehaviour
{
    public Transform player;
    public Transform target;

    public float frontDistance = 2f;
    public float height = 1.3f;

    public float moveSpeed = 4f;
    public float flutterAmount = 0.35f;
    public float flutterSpeed = 2f;

    public float segmentLength = 8f;
    public float easeOutDistance = 2f;
    public float fadeDuration = 2f;

    private Vector3[] pathPoints;
    private float t = 0f;
    private float traveled = 0f;
    private float noiseSeed;
    private bool fading = false;
    private float currentSpeedMultiplier = 1f;

    private ParticleSystem ps;
    private LineRenderer lr;

    private void Start()
    {
        ps = GetComponentInChildren<ParticleSystem>();
        lr = GetComponent<LineRenderer>();
        noiseSeed = Random.value * 100f;
        Trigger();
    }

    void Trigger()
    {
        BuildNavPath();
        if (ps != null) ResetParticleAlpha();
        if (lr != null) ResetLineRendererAlpha();
        fading = false;
        currentSpeedMultiplier = 1f;

        StartCoroutine(FrontAndGuide());
    }

    IEnumerator FrontAndGuide()
    {
        Vector3 front = player.position + player.forward * frontDistance;
        front.y += height;

        while (Vector3.Distance(transform.position, front) > 0.2f)
        {
            transform.position = Vector3.Lerp(transform.position, front, Time.deltaTime * moveSpeed * 0.8f);
            yield return null;
        }

        t = 0f;
        traveled = 0f;
        StartCoroutine(MoveAndFade());
    }

    void BuildNavPath()
    {
        NavMeshPath npath = new NavMeshPath();
        NavMesh.CalculatePath(player.position, target.position, NavMesh.AllAreas, npath);

        int count = Mathf.Min(npath.corners.Length, 4);
        pathPoints = new Vector3[count];

        for (int i = 0; i < count; i++)
            pathPoints[i] = npath.corners[i];
    }

    IEnumerator MoveAndFade()
    {
        while (true)
        {
            if (pathPoints == null || pathPoints.Length < 2) yield break;

            // Base position & flutter
            Vector3 basePos = Vector3.Lerp(pathPoints[0], pathPoints[1], t);
            basePos.y += height;

            Vector3 forward = (pathPoints[1] - pathPoints[0]).normalized;
            Vector3 perp = Vector3.Cross(Vector3.up, forward);
            float flutter = Mathf.Sin((Time.time + noiseSeed) * flutterSpeed) * flutterAmount;
            Vector3 flutterOffset = perp * flutter;
            flutterOffset.y += Mathf.Sin((Time.time * flutterSpeed * 0.5f) + noiseSeed) * (flutterAmount * 0.4f);

            transform.position = basePos + flutterOffset;
            transform.LookAt(basePos + forward);

            // Track distance
            if (t > 0f) traveled += Vector3.Distance(transform.position, basePos);

            // Easing & fade start
            float remaining = segmentLength - traveled;
            if (remaining <= easeOutDistance)
            {
                currentSpeedMultiplier = Mathf.Clamp01(remaining / easeOutDistance);
                if (!fading)
                {
                    fading = true;
                    StartCoroutine(FadeAndDestroy());
                }
            }

            // Advance along path with speed multiplier
            t += Time.deltaTime * (moveSpeed * currentSpeedMultiplier / 10f);
            t = Mathf.Clamp01(t);

            if (currentSpeedMultiplier <= 0.01f) yield break;

            yield return null;
        }
    }

    IEnumerator FadeAndDestroy()
    {
        float timer = 0f;

        ParticleSystem.MainModule main = ps != null ? ps.main : default;
        Color psColor = ps != null ? main.startColor.color : Color.white;

        Color lrStartColor = lr != null ? lr.startColor : Color.white;
        Color lrEndColor = lr != null ? lr.endColor : Color.white;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, timer / fadeDuration);

            // Fade particle
            if (ps != null)
            {
                psColor.a = alpha;
                main.startColor = psColor;
            }

            // Fade line renderer
            if (lr != null)
            {
                lr.startColor = new Color(lrStartColor.r, lrStartColor.g, lrStartColor.b, alpha);
                lr.endColor = new Color(lrEndColor.r, lrEndColor.g, lrEndColor.b, alpha);
            }

            yield return null;
        }

        Destroy(gameObject);
    }

    void ResetParticleAlpha()
    {
        if (ps == null) return;
        ParticleSystem.MainModule main = ps.main;
        Color c = main.startColor.color;
        c.a = 1f;
        main.startColor = c;
        ps.Play();
    }

    void ResetLineRendererAlpha()
    {
        if (lr == null) return;
        Color sc = lr.startColor;
        Color ec = lr.endColor;
        lr.startColor = new Color(sc.r, sc.g, sc.b, 1f);
        lr.endColor = new Color(ec.r, ec.g, ec.b, 1f);
    }
}
