using System.Runtime.CompilerServices;
using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    public enum PointType { Pickup, Destination }
    public enum quest{one, two, three}
    public quest currentQuest;
    public PointType pointType;
    private bool pickupActivatedOnce = false;
    private bool destinationActivatedOnce = false;
    [SerializeField] GameObject pickupButton;
    [SerializeField] GameObject destinationButton;
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("cat"))
        {
            
            if (pointType == PointType.Pickup && !pickupActivatedOnce){
                pickupButton.SetActive(true);
                pickupActivatedOnce = true;
            }
            else if(!destinationActivatedOnce)
            {
                destinationActivatedOnce = true;
                destinationButton.SetActive(true);
            }
        }
    }


    public void callPlayerReachedPickup()
    {
        pickupButton.SetActive(false);
        GameManager.Instance.PlayerReachedPickup();
        
    }

    public void callPlayerReachedDestination()
    {
        GameManager.Instance.PlayerReachedDestination();
        destinationButton.SetActive(false);
    }
}
