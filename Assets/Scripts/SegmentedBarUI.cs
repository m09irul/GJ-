using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SegmentedBarUI : MonoBehaviour
{
    [Tooltip("Assign 5 segment image objects here.")]
    public List<Image> segments;

    [Tooltip("Assign 5 colors—each color matches each segment.")]
    public List<Color> segmentColors;

    [Tooltip("Color used when a segment is turned OFF.")]
    public Color disabledColor = new Color(1, 1, 1, 0.1f);

    public void SetValue(int currentValue)
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (i < currentValue)
            {
                // Apply the segment’s own unique color
                segments[i].color = segmentColors[i];
            }
            else
            {
                // Turn off color
                segments[i].color = disabledColor;
            }
        }
    }
}
