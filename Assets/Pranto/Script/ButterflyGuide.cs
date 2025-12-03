using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButterflyGuide : MonoBehaviour
{
    public Transform player;
    public Transform currentTarget;
    public float moveSpeed = 5f;
    public float verticalBobSpeed = 2f;
    public float verticalBobAmount = 0.2f;

    public float trailInterval = 1.5f;
    private float trailTimer = 0f;

    public GameObject butterflyPrefab;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(player == null || currentTarget == null) return;

        transform.position = player.position + new Vector3(0, Mathf.Sin(Time.time * verticalBobSpeed) * verticalBobAmount, 0);
        trailTimer += Time.deltaTime;

        if(trailTimer >= trailInterval){
            trailTimer = 0f;
            SpawnGuideTowardsTarget();
        }        



    }


    void SpawnGuideTowardsTarget(){
        Vector3 dir = (currentTarget.position - player.position).normalized;

        GameObject b = Instantiate(butterflyPrefab, player.position, Quaternion.identity);

        b.transform.rotation = Quaternion.LookRotation(dir);

        b.AddComponent<FlyTowards>().Initialize(dir);

    }


    //eitare quest manager theika daak marba tomra
    public void UpdateTarget(Transform newTarget){
        currentTarget = newTarget;
    }
}
