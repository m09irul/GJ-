using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText;
    public void SetValue(int currentValue)
    {
        coinText.text = currentValue.ToString();
    }
}
