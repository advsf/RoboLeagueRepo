using UnityEngine;
using UnityEngine.EventSystems;

public class TouchPanel : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    [Header("Settings")]
    public float deadzone = 0.1f;

    private Vector2 touchDelta;
    public Vector2 GetTouchDelta => touchDelta;

    private bool isDragging;

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;

        touchDelta = Vector2.zero;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (eventData.delta.magnitude > deadzone)
            touchDelta = eventData.delta;
        else
            touchDelta = Vector2.zero;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        touchDelta = Vector2.zero;
    }

    private void LateUpdate()
    {
        // prevents the screen from moving even when holding still
        touchDelta = Vector2.zero;
    }
}