using Unity.Mathematics;
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
        AudioManager.instance.play("ItemDrop");
        var dropFx = Instantiate(PrefabDatabase.Instance.GetPrefab(7), transform.position, quaternion.identity);
        Destroy(dropFx, 2f);

        AIStimulusDispatcher.Emit(
            new AIStimulus(stimulusType, transform.position)
        );

        Destroy(gameObject, destroyDelay);
    }
}
