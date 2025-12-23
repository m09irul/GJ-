using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PrefabEntry
{
    public int itemID;
    public GameObject prefab;
}

public class PrefabDatabase : MonoBehaviour
{
    public static PrefabDatabase Instance { get; private set; }

    [SerializeField]
    private PrefabEntry[] items;

    private Dictionary<int, GameObject> lookup;

    private void Awake()
    {
        // Singleton safety
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        BuildLookup();
    }

    private void BuildLookup()
    {
        lookup = new Dictionary<int, GameObject>();

        foreach (var entry in items)
        {
            if (entry == null || entry.prefab == null) continue;

            if (!lookup.ContainsKey(entry.itemID))
                lookup.Add(entry.itemID, entry.prefab);
            else
                Debug.LogWarning($"Duplicate itemID in ItemPrefabDatabase: {entry.itemID}");
        }
    }

    public GameObject GetPrefab(int itemID)
    {
        if (lookup == null)
            BuildLookup();

        lookup.TryGetValue(itemID, out GameObject prefab);
        return prefab;
    }
}

