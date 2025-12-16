using UnityEngine;

public class MoveObjectInACircle : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float radius = 5f;
    [SerializeField] private float speed = 1f;

    private float angle;
    private Vector3 center;

    private void Start()
    {
        center = transform.position;
    }

    private void Update()
    {
        angle += speed * Time.deltaTime;

        float x = Mathf.Cos(angle) * radius;
        float z = Mathf.Sin(angle) * radius;

        transform.position = center + new Vector3(x, 0f, z);
    }
}
