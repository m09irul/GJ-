using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    public enum FoodType
    {
        edible,
        inedible
    }

    public FoodType foodType;
    private bool isLocked = false;

    public bool IsLocked => isLocked;
    private void Start()
    {
        StartCoroutine(setLayer());
    }
    IEnumerator setLayer()
    {
        yield return new WaitForSeconds(1f);
        // Set the layer to "Food" for detection
        gameObject.layer = LayerMask.NameToLayer("Food");

        StopCoroutine(setLayer());
    }
    public void LockFood()
    {
        isLocked = true;
        gameObject.layer = LayerMask.NameToLayer("Default");
    }

    public void UnlockFood()
    {
        isLocked = false;
        // Change back to Food layer so it can be detected again
        gameObject.layer = LayerMask.NameToLayer("Food");
    }

    public void ConsumeFood()
    {
        if (foodType == FoodType.edible)
        {
            Debug.Log("Food is edible. Dog is happy!");
            // Destroy the food object since it was eaten
            Destroy(gameObject);
        }
        else
        {
            Debug.Log("Food is inedible. Dog is sad!");
            // Unlock the food so other dogs can try it
            //UnlockFood();
        }
    }
}