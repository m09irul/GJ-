using UnityEngine;

public class CarTrigger : MonoBehaviour
{
    public enum TriggerState
    {
        Start,
        End
    }

    [Header("State")]
    public TriggerState triggerState;
    public VanController vanController;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("cat"))
            return;

        if (triggerState == TriggerState.Start)
            vanController.StartVan();
        else{
            vanController.StopVan();
            this.enabled = false;
        }
    }
}
