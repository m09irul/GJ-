using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StarsUI : MonoBehaviour
{
    public TextMeshProUGUI starText;
    public void SetValue(int currentValue)
    {
        starText.text = currentValue.ToString();
    }
}
