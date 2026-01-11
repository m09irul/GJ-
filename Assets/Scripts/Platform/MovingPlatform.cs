using UnityEngine;
using DG.Tweening;

public class MovingPlatform : MonoBehaviour
{
    [Header("Path")]
    public GameObject pointsHolder;
    private Transform[] points;

    [Header("Movement")]
    [SerializeField] float speed = 2f;
    [SerializeField] bool autoMove = false;
    [SerializeField] float waitAtPoint = 1f;
    public Ease ease = Ease.Linear;

    [Header("Handle")]
    [SerializeField] Transform handle;
    [SerializeField] float handleAngleStart = -60f;
    [SerializeField] float handleAngleEnd = -120f;
    [SerializeField] float handleRotateDuration = 0.3f;
    [SerializeField] private GameObject bottom;
    [Space]

    int index = 0;
    int direction = 1;
    bool playerInside;

    Tween moveTween;
    Tween waitTween;

    void Start()
    {
        // Cache points
        Transform parent = pointsHolder.transform;
        points = new Transform[parent.childCount];
        for (int i = 0; i < parent.childCount; i++)
            points[i] = parent.GetChild(i);

        // Start auto move
        if (autoMove)
            MoveNext();
    }

    /* =========================
     * AUTO / MANUAL CONTROL
     * ========================= */

    void MoveNext()
    {
        int nextIndex = index + direction;

        if (nextIndex >= points.Length || nextIndex < 0)
        {
            direction *= -1;
            nextIndex = index + direction;
        }

        MoveTo(nextIndex, () =>
        {
            if (autoMove)
            {
                waitTween = DOVirtual.DelayedCall(waitAtPoint, MoveNext);
            }
        });
    }

    void MoveTo(int targetIndex, System.Action onComplete = null)
    {
        moveTween?.Kill();

        float dist = Vector3.Distance(transform.position, points[targetIndex].position);
        float duration = dist / speed;

        RotateHandle(direction == 1);

        moveTween = transform.DOMove(points[targetIndex].position, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                index = targetIndex;
                moveTween = null;
                onComplete?.Invoke();
            });
    }

    /* =========================
     * MANUAL TRIGGER CONTROL
     * ========================= */

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("cat")) return;

        other.transform.SetParent(transform);
        playerInside = true;

        if (autoMove) return;

        direction = 1;
        MoveTo(1);
        ToggleOutline(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("cat")) return;

        other.transform.SetParent(null);
        playerInside = false;

        if (autoMove) return;

        direction = -1;
        MoveTo(0);
        ToggleOutline(false);
    }
    private void ToggleOutline(bool state)
    {
        if (handle && handle.TryGetComponent(out Outline h))
            h.enabled = state;

        if (bottom && bottom.TryGetComponent(out Outline b))
            b.enabled = state;
    }
    /* =========================
     * HANDLE
     * ========================= */

    void RotateHandle(bool forward)
    {
        if (!handle) return;

        handle.DOKill();

        float targetX = forward ? handleAngleEnd : handleAngleStart;

        handle.DOLocalRotate(
            new Vector3(targetX, 0f, 0f),
            handleRotateDuration
        ).SetEase(Ease.InOutSine);
    }
}
