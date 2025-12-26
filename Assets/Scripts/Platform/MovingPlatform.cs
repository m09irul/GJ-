using UnityEngine;
using DG.Tweening;
using System.Drawing;

public class MovingPlatform : MonoBehaviour
{
    public GameObject pointsHolder;
    [SerializeField] Transform[] points;
    [SerializeField] float speed = 2f;
    [SerializeField] bool autoMove;

    int index = 0;
    int direction = 1;
    Tween tween;
    public Ease ease;
    void Start()
    {
        Transform parent = pointsHolder.transform;

        points = new Transform[parent.childCount];

        for (int i = 0; i < parent.childCount; i++)
        {
            points[i] = parent.GetChild(i);
        }

        if (autoMove)
            Activate();
    }

    public void Activate()
    {
        Move();
    }

    void Move()
    {
        tween?.Kill();

        int next = index + direction;

        // Reverse when reaching ends
        if (next >= points.Length || next < 0)
        {
            direction *= -1;
            next = index + direction;
        }

        float distance = Vector3.Distance(transform.position, points[next].position);
        float duration = distance / speed;

        tween = transform.DOMove(points[next].position, duration)
            .SetEase(ease)
            .OnComplete(() =>
            {
                index = next;

                if (autoMove)
                    Move(); // continue automatically
            });
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
