using UnityEngine;

[CreateAssetMenu(fileName = "CosmeticInventory", menuName = "Scriptable Objects/CosmeticInventory")]
public class CosmeticInventory : ScriptableObject
{
    public GameObject[] hairPrefabs;
    public GameObject[] accessoriesPrefabs;
    public GameObject[] emotePrefabs;
    public GameObject[] ballsPrefabs;
    public GameObject[] trailsPrefab;

    public GameObject GetSelectedHair(string name)
    {
        foreach (GameObject hair in hairPrefabs)
            if (hair.name.Equals(name))
                return hair;

        return null;
    }

    public GameObject GetSelectedAccessory(string name)
    {
        foreach (GameObject accessory in accessoriesPrefabs)
            if (accessory.name.Equals(name))
                return accessory;

        return null;
    }

    public GameObject GetSelectedEmotes(string name)
    {

        return null;
    }

    public GameObject GetSelectedBalls(string name)
    {
        foreach (GameObject ball in ballsPrefabs)
            if (ball.name.Equals(name))
                return ball;

        return null;
    }

    public GameObject GetSelectedTrails(string name)
    {
        foreach (GameObject trails in trailsPrefab)
            if (trails.name.Equals(name))
                return trails;

        return null;
    }
}
