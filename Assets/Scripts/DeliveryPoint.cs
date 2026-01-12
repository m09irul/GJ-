using System.Runtime.CompilerServices;
using UnityEngine;
using DG.Tweening;
public class DeliveryPoint : MonoBehaviour
{
    public enum PointType { Pickup, Destination }
    public PointType pointType;
    public GameObject package;
    public Collider nextTrigger;
    void Start()
    {
        if (nextTrigger != null)
            nextTrigger.isTrigger = false;

        transform.DORotate(new Vector3(0, 360, 0), 10f, RotateMode.FastBeyond360)
             .SetEase(Ease.Linear)
             .SetLoops(-1);
    }
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("cat")) return;

        var button = UIManager.Instance.pick_deliverButton;
        button.onClick.RemoveAllListeners();
        button.gameObject.SetActive(true);

        if (pointType == PointType.Pickup && !GameManager.Instance.hasPackage)
        {
            button.onClick.AddListener(AddPickupListener);
        }
        else if (pointType == PointType.Destination && GameManager.Instance.hasPackage)
        {
            button.onClick.AddListener(AddDestinationListener);
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("cat")) return;

        UIManager.Instance.pick_deliverButton.onClick.RemoveAllListeners();
        UIManager.Instance.pick_deliverButton.gameObject.SetActive(false);
    }
    void AddPickupListener()
    {
        if (nextTrigger != null)
            nextTrigger.isTrigger = true;

        GameManager.Instance.PlayerReachedPickup();

        package.SetActive(false);
        UIManager.Instance.pick_deliverButton.gameObject.SetActive(false);
    }
    void AddDestinationListener()
    {
        GameManager.Instance.PlayerReachedDestination();
        package.SetActive(true);
        UIManager.Instance.pick_deliverButton.gameObject.SetActive(false);
    }
}
