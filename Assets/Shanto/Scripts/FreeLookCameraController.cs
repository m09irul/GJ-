using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class FreeLookCameraController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    Image imgControlArea;
    [SerializeField] CinemachineFreeLook freeLookCam;
    string strMouseX = "Mouse X", strMouseY = "Mouse Y";
    // Start is called before the first frame update
    void Start()
    {
        imgControlArea = GetComponent<Image>();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if(RectTransformUtility.ScreenPointToLocalPointInRectangle(
            imgControlArea.rectTransform, eventData.position,
            eventData.enterEventCamera, out Vector2 posOut
        ))
        {
            freeLookCam.m_XAxis.m_InputAxisName = strMouseX;
            freeLookCam.m_YAxis.m_InputAxisName = strMouseY;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log(4);
        freeLookCam.m_XAxis.m_InputAxisName = null;
        freeLookCam.m_YAxis.m_InputAxisName = null;
        freeLookCam.m_XAxis.m_InputAxisValue = 0;
        freeLookCam.m_YAxis.m_InputAxisValue = 0;
    }
}
