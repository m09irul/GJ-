using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BountyBarUI : MonoBehaviour
{
    [Tooltip("Assign 5 bounty image objects here.")]
    public List<Image> bounties;

    public void SetValue(int currentValue)
    {
        for (int i = 0; i < bounties.Count; i++)
        {
            if (i < currentValue)
            {
                bounties[i].gameObject.SetActive(true);
            }
            else
            {
                bounties[i].gameObject.SetActive(false);
            }
        }
    }
}
