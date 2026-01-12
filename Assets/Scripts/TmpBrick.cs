using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class TmpBrick : MonoBehaviour
{
    [Header("Timings")]
    [SerializeField] float deactivateDelay = 0.3f;
    [SerializeField] float respawnDelay = 2f;

    [Header("Detection")]
    [SerializeField] string playerTag = "cat";

    Collider col;
    MeshRenderer meshRenderer;
    Outline outline;
    bool active = true;

    void Awake()
    {
        col = GetComponent<Collider>();
        meshRenderer = GetComponent<MeshRenderer>();
        outline = GetComponent<Outline>();
    }

    void OnTriggerEnter(Collider collision)
    {
        if (!active) return;
        if (!collision.CompareTag(playerTag)) return;

        outline.enabled = true;
        StartCoroutine(DeactivateRoutine());
    }

    IEnumerator DeactivateRoutine()
    {
        active = false;
        AudioManager.instance.play("brick break");
        yield return new WaitForSeconds(deactivateDelay);

        // Disable visuals + collision
        SetBrickState(false);

        yield return new WaitForSeconds(respawnDelay);

        // Enable again
        SetBrickState(true);
        AudioManager.instance.play("brick join");

        active = true;
        outline.enabled = false;

    }

    void SetBrickState(bool state)
    {
        col.enabled = state;

        meshRenderer.enabled = state;
    }
}
