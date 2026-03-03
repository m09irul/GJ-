using System;
using UnityEngine;

public class Temp : MonoBehaviour
{
    [SerializeField] public int WantedLevel;
    [SerializeField] private CharacterController ch;
    private float speed = 3f;

    public static Action<int> WantedLevelUpdate;
    // Start is called before the first frame update
    void Start()
    {
        WantedLevelUpdate?.Invoke(1);
    }

    float gravity = -9.81f;
    float yVelocity;

    void Update()
    {
        float z = Input.GetAxis("Horizontal");
        float x = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        move *= speed;

        if (ch.isGrounded && yVelocity < 0)
            yVelocity = -2f;

        yVelocity += gravity * Time.deltaTime;
        move.y = yVelocity;

        ch.Move(move * Time.deltaTime);
    }

}
