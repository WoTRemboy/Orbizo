using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//script for regular scence, replacing PlatformSpawner
public class RegularPlatformSpawner : MonoBehaviour
{
    [Header("Spawn settings")]
    public Platform[] platforms;
    public float platformLength = 3f;
    public int amountAtStart = 6;

    [Tooltip("Игрок (нужен только по X)")]
    public Transform player;

    [Header("Off-screen settings")]
    [SerializeField] private float screenOffset = 2f;     // ← запас за краем экрана

    // --- runtime ---
    private Vector2 playerGroundPosition;
    private Vector2 lastSpawnPoint;
    private Vector2 checkPoint;
    private Vector2 checkPointDistance;
    private float sqrPlatformLength;

    // camera data
    private float halfScreenWidth;

    private List<KeyValuePair<GameObject, int>> platformList;
    private List<float> cd, cdAtStart;
    private Pool pool;
    private SpawnManager spawnManager;

    void Start()
    {
        spawnManager = GameObject.Find("GameManager").GetComponent<SpawnManager>();
        pool = GetComponent<Pool>();

        // ► ширина видимой области (мировые ед.!)
        halfScreenWidth = Camera.main.orthographicSize * Camera.main.aspect;

        playerGroundPosition = new Vector2(player.position.x, 0f);

        StructProcessor();
        pool.CreatePool(platformList, transform);

        lastSpawnPoint = SpawnAtStart(amountAtStart);

        CalculateCheckPointDistance();               // ← обновлённый расчёт
        sqrPlatformLength = Mathf.Pow(platformLength - 0.2f, 2f);
    }

    void Update()
    {
        playerGroundPosition = new Vector2(player.position.x, 0f);
        checkPoint = playerGroundPosition + checkPointDistance;

        if ((checkPoint - lastSpawnPoint).sqrMagnitude > sqrPlatformLength)
        {
            GameObject obj = SpawnPlatform(cd, checkPoint);
            spawnManager.SendMessage("SpawnOtherStuff", obj.transform, SendMessageOptions.DontRequireReceiver);
            lastSpawnPoint = checkPoint;
        }
    }

    /* ---------- key change ---------- */
    void CalculateCheckPointDistance()
    {
        // базовая «длина очереди» из amountAtStart
        float baseDistance = amountAtStart * platformLength;

        // минимальное расстояние, обеспечивающее появление за экраном
        float minDistance = halfScreenWidth + screenOffset;

        float dist = Mathf.Max(baseDistance, minDistance);
        checkPointDistance = new Vector2(dist, 0f);
    }
    /* -------------------------------- */

    Vector2 SpawnAtStart(int amount)
    {
        SpawnPlatform(cdAtStart, playerGroundPosition);

        Vector2 last = playerGroundPosition;
        for (int i = 0; i < amount - 1; i++)
        {
            last.x += platformLength;
            SpawnPlatform(cdAtStart, last);
        }
        return last;
    }

    GameObject SpawnPlatform(List<float> cd, Vector2 position)
    {
        GameObject obj = pool.getPooledObject(BinarySearch.RandomNumberGenerator(cd));
        obj.transform.position = position;
        obj.SetActive(true);
        return obj;
    }

    void StructProcessor () 
	{	
		platformList = new List<KeyValuePair<GameObject, int>> (); 
		cd = new List<float> ();
		cdAtStart = new List<float> ();
		
		float sum1 = 0;
		float sum2 = 0;
		
		cd.Add (sum1);
		cdAtStart.Add (sum2);
		
		foreach (Platform platform in platforms)
		{
			platformList.Add (new KeyValuePair<GameObject, int> (platform.platformPrefab, (platform.amount > 0) ? platform.amount : 1));
			
			sum1 += platform.probability;
			
			cd.Add (sum1);
			
			sum2 = platform.canSpawnAtStart ? sum2 + platform.probability : sum2;
			
			cdAtStart.Add (sum2);
		}

		if (sum1 != 1f)
			cd = cd.ConvertAll (x => x /sum1);
		
		if (sum2 == 0f)
			cdAtStart = cd; 
		else if (sum2 != 1f)
			cdAtStart = cdAtStart.ConvertAll(x => x / sum2);
	}

}
