using System.Runtime.CompilerServices;
using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    public enum PointType { Pickup, Destination }
    public PointType pointType;
    [SerializeField] GameObject pickupButton;
    [SerializeField] GameObject destinationButton;
    public GameObject package;
    private void OnTriggerEnter(Collider other)
    {
        
        if (other.CompareTag("cat"))
        {
            Debug.Log("pointssss5555 hit the Cat!");
            if (pointType == PointType.Pickup && !GameManager.Instance.hasPackage){
                pickupButton.SetActive(true);
            }
            else if (pointType == PointType.Destination && GameManager.Instance.hasPackage)
            {
                destinationButton.SetActive(true);
            }
        }
    }


    public void callPlayerReachedPickup()
    {
        AudioManager.instance.play("CatBellSFX");
        pickupButton.SetActive(false);
        GameManager.Instance.PlayerReachedPickup();
        package.SetActive(true);
        
    }

    public void callPlayerReachedDestination()
    {
        AudioManager.instance.play("CatBellSFX");
        GameManager.Instance.PlayerReachedDestination();
        destinationButton.SetActive(false);
        package.SetActive(false);
    }
}
