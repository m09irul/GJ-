using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    public string message;              // What the tutorial says
    public float triggerRadius = 5f;    // Distance to activate
    public bool triggered = false;      // Prevent repeat triggers

    private Transform player;

    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;
    }

    private void Update()
    {
        if (triggered) return;

        if (Vector3.Distance(player.position, transform.position) < triggerRadius)
        {
            triggered = true;
            TutorialManager.Instance.ShowTutorial(message);
        }
    }
}
