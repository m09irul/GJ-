using UnityEngine;

public class Laser : MonoBehaviour
{
    public enum LaserMode
    {
        Static,
        Linear,
        PingPong,
        Circular
    }

    [Header("Mode")]
    public LaserMode mode = LaserMode.Static;

    [Header("Motion")]
    public Vector3 direction = Vector3.right;
    public float speed = 2f;
    public float distance = 3f;

    private Vector3 startPos;
    private bool isFrozen;
    private float t;

    void Awake()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (isFrozen) return;

        switch (mode)
        {
            case LaserMode.Linear:
                transform.position += direction.normalized * speed * Time.deltaTime;
                break;

            case LaserMode.PingPong:
                t += Time.deltaTime * speed;
                transform.position = startPos +
                    direction.normalized * Mathf.Sin(t) * distance;
                break;

            case LaserMode.Circular:
                t += Time.deltaTime * speed;
                transform.position = startPos +
                    new Vector3(Mathf.Cos(t), 0, Mathf.Sin(t)) * distance;
                break;
        }
    }

    public void Freeze()   => isFrozen = true;
    public void Unfreeze() => isFrozen = false;
}
