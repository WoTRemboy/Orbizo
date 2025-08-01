using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Pool))]
public class CloudSpawner : MonoBehaviour
{
    // ──────────────── Inspector Fields ────────────────
    [Header("Cloud prefabs")]
    [SerializeField] private GameObject[] clouds;          // Prefabs to spawn

    [Header("Spawn area (Y-axis, local to background)")]
    [SerializeField] private float minY = -3f;             // Lowest local Y position
    [SerializeField] private float maxY = 3f;             // Highest local Y position

    [Header("Timing & speed")]
    [SerializeField] private float interval = 3f;         // Seconds between spawns
    [SerializeField] private float minSpeed = 0.4f;       // Minimum cloud speed
    [SerializeField] private float maxSpeed = 0.8f;       // Maximum cloud speed

    [Header("Off-screen offset")]
    [SerializeField] private float screenOffset = 1f;      // Extra distance beyond the visible edge

    // ──────────────── Runtime Fields ────────────────
    private Pool pool;
    private Camera cam;
    private float halfScreenWidth;                        // Half of the visible width (world units)

    // ──────────────── Unity Events ────────────────
    private void Start()
    {
        cam = Camera.main;
        halfScreenWidth = cam.orthographicSize * cam.aspect; // Convert camera size to world units

        pool = GetComponent<Pool>();

        // Build object-pool list: two instances of each prefab
        var list = new List<KeyValuePair<GameObject, int>>();
        foreach (GameObject c in clouds)
            list.Add(new KeyValuePair<GameObject, int>(c, 2));

        pool.CreatePool(list, transform);

        StartCoroutine(SpawnCloud());
    }

    // ──────────────── Coroutines ────────────────
    private IEnumerator SpawnCloud()
    {
        while (true)
        {
            // X position: right camera edge plus offset
            float worldX = cam.transform.position.x + halfScreenWidth + screenOffset;
            // Y position: random inside the configured vertical range (local → world)
            float worldY = transform.position.y + Random.Range(minY, maxY);

            Vector2 spawnPos = new Vector2(worldX, worldY);

            // Pick a random prefab from the pool
            int prefabIndex = Random.Range(0, clouds.Length);
            GameObject cloud = pool.getPooledObject(prefabIndex);

            cloud.transform.position = spawnPos;                                  // World position
            cloud.GetComponent<CloudMover>().speed = Random.Range(minSpeed, maxSpeed);
            cloud.SetActive(true);

            yield return new WaitForSeconds(interval);
        }
    }
}
