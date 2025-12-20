using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPanel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    private Vector2 touchDelta;
    public Vector2 GetTouchDelta => touchDelta;

    private bool isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        touchDelta = eventData.delta;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        touchDelta = Vector2.zero;
    }

    private void LateUpdate()
    {
        // reset to prevent some sort of weird rotation
        if (!isDragging) 
            touchDelta = Vector2.zero;
    }
}