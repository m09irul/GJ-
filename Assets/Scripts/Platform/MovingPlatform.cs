using UnityEngine;
using DG.Tweening;

public class MovingPlatform : MonoBehaviour
{
    public GameObject pointsHolder;
    [SerializeField] Transform[] points;
    [SerializeField] float speed = 2f;
    [SerializeField] bool autoMove;

    [Header("Handle")]
    [SerializeField] float handleAngleA = -60f;
    [SerializeField] float handleAngleB = -120f;
    [SerializeField] float handleRotateDuration = 0.3f;

    int index = 0;
    int direction = 1;

    Tween moveTween;
    public Ease ease;

    bool handleToggled;

    void Start()
    {
        Transform parent = pointsHolder.transform;
        points = new Transform[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
            points[i] = parent.GetChild(i);

        if (autoMove)
            Activate();
    }

    public void Activate(GameObject handle = null)
    {
        // If platform cannot move, do NOTHING
        if (!CanMove())
            return;

        // Rotate handle ONLY if movement is allowed
        if (handle)
            RotateHandle(handle.transform);

        Move();
    }

    bool CanMove()
    {
        return moveTween == null || !moveTween.IsActive() || !moveTween.IsPlaying();
    }

    void Move()
    {
        int next = index + direction;

        if (next >= points.Length || next < 0)
        {
            direction *= -1;
            next = index + direction;
        }

        float distance = Vector3.Distance(transform.position, points[next].position);
        float duration = distance / speed;

        moveTween = transform.DOMove(points[next].position, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                index = next;

                if (autoMove)
                    Move();
            });
    }
    void RotateHandle(Transform handle)
    {
        handleToggled = !handleToggled;

        float targetX = handleToggled ? handleAngleB : handleAngleA;

        handle.DOKill();

        handle.DOLocalRotate(
            new Vector3(targetX, 0f, 0f),
            handleRotateDuration
        ).SetEase(Ease.InOutSine);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cat"))
            other.transform.SetParent(transform);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("cat"))
            other.transform.SetParent(null);
    }
}
