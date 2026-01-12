using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FarmingTrigger : MonoBehaviour
{
     private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("cat"))
            return;

        UIManager.Instance.farmingPanel.SetActive(true);
        AudioManager.instance.play("inventory open");

        
    }
}
