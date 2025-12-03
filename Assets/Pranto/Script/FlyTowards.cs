using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyTowards : MonoBehaviour
{
    public float speed = 6f;
    public Vector3 direction;


    // Start is called before the first frame update
    public void Initialize(Vector3 dir){
        direction = dir;
        Destroy(gameObject, 2.5f);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }
}
