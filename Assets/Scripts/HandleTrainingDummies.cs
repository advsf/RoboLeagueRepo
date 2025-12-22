using UnityEngine;

public class HandleTrainingDummies : MonoBehaviour
{
    public static HandleTrainingDummies instance;

    [SerializeField] private GameObject dummyPrefab;
    [SerializeField] private Transform dummyParent;
    [SerializeField] private float forwardSpawnDistance = 2.5f;
    [SerializeField] private float doublePressInterval = 0.3f;

    private float lastBPressTime = -1f;

    private void Start()
    {
        instance = this;
    }

    private void OnDestroy()
    {
        instance = null;
    }

    private void Update()
    {
        if (!HandleCursorSettings.instance.IsUIOn())
        {
            // create dummy
            if (Input.GetKeyDown(KeyCode.N))
                CreateDummy();

            // delete all dummies (double-press B)
            if (Input.GetKeyDown(KeyCode.B))
                DestroyAllDummy();
        }
    }

    public void CreateDummy()
    {
        Transform player = PlayerMovement.instance.transform;
        Vector3 spawnPos = player.position + player.forward * forwardSpawnDistance;
        Quaternion spawnRot = Quaternion.Euler(0, player.eulerAngles.y, 0);

        Instantiate(dummyPrefab, spawnPos, spawnRot, dummyParent);
    }

    public void DestroyAllDummy()
    {
        if (Time.time - lastBPressTime <= doublePressInterval)
            foreach (Transform dummy in dummyParent)
                Destroy(dummy.gameObject);

        lastBPressTime = Time.time;
    }
}
