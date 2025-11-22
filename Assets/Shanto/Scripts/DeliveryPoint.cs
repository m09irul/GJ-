using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    public enum PointType { Pickup, Destination }
    public PointType pointType;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("cat"))
        {
            if (pointType == PointType.Pickup)
                GameManager.Instance.PlayerReachedPickup();
            else
                GameManager.Instance.PlayerReachedDestination();
        }
    }
}
