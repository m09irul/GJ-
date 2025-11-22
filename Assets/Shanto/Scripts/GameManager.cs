using UnityEngine;

public class GameManager : MonoBehaviour
{
    private float startTime = 0f;
    public float takenTime;
    public static GameManager Instance;

    public Transform pickupPoint;
    public Transform destinationPoint;

    public bool hasPackage = false;
    public bool taskCompleted = false;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        startTime = Time.time;
        Debug.Log("Go to Pickup Point!");
    }

    public void PlayerReachedPickup()
    {
        if (!hasPackage)
        {
            hasPackage = true;
            Debug.Log("Package Picked! Now go to Destination.");
        }
    }

    public void PlayerReachedDestination()
    {
        if (hasPackage)
        {
            takenTime = Time.time - startTime;
            Debug.Log("Time Taken: " + takenTime + " seconds.");
            hasPackage = false;
            taskCompleted = true;
            Debug.Log("Delivery Completed!");
        }
        else
        {
            Debug.Log("You don't have any package.");
        }
    }
}
