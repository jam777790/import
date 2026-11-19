using UnityEngine;
using UnityEngine.Pool;
using System;
using System.Collections.Generic;
using Random = UnityEngine.Random;

[Serializable]
public class MonsterSpawnData
{
    public Monster prefab;             // 프리팹만 등록! (프리팹 안에 이미 자기 SO가 들어있음)
    public float startTime;            // 출현 시작 시간
    public float endTime = 9999f;      // 출현 종료 시간
    [Range(1, 100)]
    public int spawnWeight = 10;       // 출현 가중치
}

public class Spawner : MonoBehaviour
{
    [Header("몬스터 스폰 테이블")]
    public List<MonsterSpawnData> spawnTable = new List<MonsterSpawnData>();

    [Header("스폰 주기 & 반경")]
    public float spawnRadius = 13f;
    public float initialInterval = 1.0f;
    public float minInterval = 0.25f;
    public float intervalDecreaseRate = 0.05f;

    [Header("중간 보스 (90초)")]
    public float midBossTime = 90f;
    public Monster midBossPrefab;      // 중간 보스도 프리팹만 등록
    public int rushMonsterCount = 20;

    [Header("최종 보스 (180초 / 3분)")]
    public float finalBossTime = 180f;
    public Monster finalBossPrefab;    // 보스 프리팹만 등록

    [Header("경험치 보석 설정 (4주차 추가)")]
    public ExpGem expGemPrefab;        //  드랍할 경험치 보석 프리팹
    private IObjectPool<ExpGem> gemPool; //  보석 전용 오브젝트 풀

    private Transform player;
    private float gameTimer = 0f;
    private float spawnTimer = 0f;
    private float currentSpawnInterval;
    private bool hasMidBossSpawned = false;
    private bool hasFinalBossSpawned = false;
    private bool isBossPhase = false;

    private Dictionary<Monster, IObjectPool<Monster>> poolDict = new Dictionary<Monster, IObjectPool<Monster>>();

    [Header("최적화 제한")]
    public int maxActiveMonsters = 80;
    private int currentActiveMonsters = 0;

    private void Awake()
    {
        // 1. 몬스터 종류별 풀 생성
        foreach (var data in spawnTable)
        {
            if (data.prefab != null && !poolDict.ContainsKey(data.prefab))
            {
                poolDict.Add(data.prefab, CreatePool(data.prefab));
            }
        }

        // 2.  경험치 보석 풀 생성
        if (expGemPrefab != null)
        {
            gemPool = new ObjectPool<ExpGem>(
                createFunc: () => Instantiate(expGemPrefab, transform),
                actionOnGet: (gem) => gem.gameObject.SetActive(true),
                actionOnRelease: (gem) => gem.gameObject.SetActive(false),
                actionOnDestroy: (gem) => Destroy(gem.gameObject),
                collectionCheck: false,
                defaultCapacity: 50,
                maxSize: 200
            );
        }

        currentSpawnInterval = initialInterval;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        gameTimer += Time.deltaTime;

        CheckBossEvents();
        if (isBossPhase) return;

        UpdateDifficulty();

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= currentSpawnInterval)
        {
            SpawnMonsterByTimeline();
            spawnTimer = 0f;
        }
    }

    private void SpawnMonsterByTimeline()
    {
        if (currentActiveMonsters >= maxActiveMonsters) return;

        List<MonsterSpawnData> availableList = new List<MonsterSpawnData>();
        int totalWeight = 0;

        foreach (var data in spawnTable)
        {
            if (gameTimer >= data.startTime && gameTimer < data.endTime)
            {
                availableList.Add(data);
                totalWeight += data.spawnWeight;
            }
        }

        if (availableList.Count == 0 || totalWeight == 0) return;

        // 가중치 추첨
        int randomVal = Random.Range(0, totalWeight);
        MonsterSpawnData selected = null;

        for (int i = 0; i < availableList.Count; i++)
        {
            if (randomVal < availableList[i].spawnWeight)
            {
                selected = availableList[i];
                break;
            }
            randomVal -= availableList[i].spawnWeight;
        }

        if (selected == null) selected = availableList[0];

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        Vector2 spawnPos = (Vector2)player.position + (randomDir * spawnRadius);

        // 풀에서 꺼내기
        IObjectPool<Monster> targetPool = poolDict[selected.prefab];
        Monster monster = targetPool.Get();

        currentActiveMonsters++; // 몬스터 소환 시 활성 카운트 +1

        //  몬스터 초기화 시 풀 반환 함수와 함께 보석 드랍 함수(SpawnExpGem)도 전달!
        monster.SpawnReset(spawnPos, (m) => ReturnToPool(m, targetPool), SpawnExpGem);
    }

    // 몬스터 사망 또는 화면 밖 이탈 시 호출되는 반환 함수
    private void ReturnToPool(Monster monster, IObjectPool<Monster> targetPool)
    {
        currentActiveMonsters--; // 풀로 돌아갈 때 활성 카운트 -1
        targetPool.Release(monster);
    }

    //  몬스터 사망 위치에 경험치 보석을 꺼내놓는 함수
    private void SpawnExpGem(Vector2 dropPos, int expReward)
    {
        if (gemPool == null || expReward <= 0) return;

        ExpGem gem = gemPool.Get();
        gem.SpawnSetup(dropPos, expReward, (g) => gemPool.Release(g));
    }

    private void CheckBossEvents()
    {
        if (!hasMidBossSpawned && gameTimer >= midBossTime)
        {
            if (midBossPrefab != null)
            {
                Vector2 bossPos = (Vector2)player.position + Vector2.up * 5f;
                Monster midBoss = Instantiate(midBossPrefab, bossPos, Quaternion.identity);
                // 중간 보스도 죽으면 보석을 드랍하도록 SpawnExpGem 연결
                midBoss.SpawnReset(bossPos, null, SpawnExpGem);
            }
            hasMidBossSpawned = true;
        }

        if (!hasFinalBossSpawned && gameTimer >= finalBossTime)
        {
            isBossPhase = true;
            if (finalBossPrefab != null)
            {
                Vector2 bossPos = (Vector2)player.position + Vector2.up * 6f;
                Monster finalBoss = Instantiate(finalBossPrefab, bossPos, Quaternion.identity);
                // 최종 보스도 죽으면 보석을 드랍하도록 SpawnExpGem 연결
                finalBoss.SpawnReset(bossPos, null, SpawnExpGem);
                finalBoss.OnMonsterDied += () => Debug.Log(" 게임 승리!");
            }
            hasFinalBossSpawned = true;
        }
    }

    private void UpdateDifficulty()
    {
        float reduction = Mathf.Floor(gameTimer / 10f) * intervalDecreaseRate;
        currentSpawnInterval = Mathf.Max(initialInterval - reduction, minInterval);
    }

    private IObjectPool<Monster> CreatePool(Monster prefab)
    {
        return new ObjectPool<Monster>(
            createFunc: () => Instantiate(prefab, transform),
            actionOnGet: (m) => m.gameObject.SetActive(true),
            actionOnRelease: (m) => m.gameObject.SetActive(false),
            actionOnDestroy: (m) => Destroy(m.gameObject),
            collectionCheck: false,
            defaultCapacity: 20,
            maxSize: 60
        );
    }
}