using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItem : MonoBehaviour
{
    public enum FoodType
    {
        edible, inedible
    }

    public FoodType foodType;

    public void lockFood()
    {
        gameObject.layer = LayerMask.NameToLayer("Default");
        if (foodType == FoodType.edible)
        {
            Debug.Log("Food is edible. Dog is happy!");
        }
        else
        {
            Debug.Log("Food is inedible. Dog is sad!");
        }
    }
}
