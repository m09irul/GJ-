using UnityEngine;

public class Item : MonoBehaviour
{
    [SerializeField] private StimulusType stimulusType;
    [SerializeField] private float destroyDelay = 0.2f;

    private bool triggered;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Ground") || triggered)
            return;

        triggered = true;

        AIStimulusDispatcher.Emit(
            new AIStimulus(stimulusType, transform.position)
        );

        Destroy(gameObject, destroyDelay);
    }
}
