using System;
using System.Collections;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static BattleManager Instance { get; private set; }

    public event Action<bool> OnBattleFinished;

    private Coroutine runningBattle;

    [Header("References")]
    public GameManager gameManager;   // 控制單位生成 / 棋盤
    public GameObject defaultEnemyPrefab; // 備用的敵人 prefab
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 開始戰鬥：傳入房間資料，並挑一個 EnemyTemplate 生成敵人
    /// </summary>
    public void StartBattle(RoomData room, EnemyTemplate template = null)
    {
        if (room == null)
        {
            Debug.LogError("[BattleManager] StartBattle: room is null");
            return;
        }

        // 1. 選 template
        if (template == null)
        {
            if (room.enemyTemplates == null || room.enemyTemplates.Count == 0)
            {
                Debug.LogError($"[BattleManager] 房間 {room.id} 沒有可用的 EnemyTemplate，無法開始戰鬥！");
                return;
            }
            template = room.enemyTemplates[UnityEngine.Random.Range(0, room.enemyTemplates.Count)];
        }

        Debug.Log($"[BattleManager] StartBattle開始戰鬥{room.id} 使用模板 {template.templateId}");

        // 2. 生成敵人
        SpawnEnemiesFromTemplate(template);

        Debug.Log("進入選關階段");
        UIManager.Instance.OpenPlaceUI(); // ShowUI

        // 3. 啟動自走棋戰鬥
        gameManager.SetState(GameState.Placement); // 先進入佈陣階段
    }

    /// <summary>
    /// 使用 EnemyTemplate + EnemySpawnInfo 生成敵人
    /// </summary>
    public void SpawnEnemiesFromTemplate(EnemyTemplate template)
    {
        foreach (var info in template.enemies)
        {
            if (info == null || info.enemyData == null)
            {
                Debug.LogWarning("[BattleManager] 遇到空的 EnemySpawnInfo 或 enemyData，跳過");
                continue;
            }

            // prefab：優先用 info.prefab，否則用 defaultEnemyPrefab
            GameObject prefabToUse = info.enemyData.prefab != null ? info.enemyData.prefab : defaultEnemyPrefab;
            if (prefabToUse == null)
            {
                Debug.LogError("[BattleManager] 找不到可用的 enemy prefab（info.prefab 與 defaultEnemyPrefab 均為 null），跳過該 spawn");
                continue;
            }

            // grid 座標
            int gridX = info.position.x;
            int gridZ = info.position.y;
            string team = string.IsNullOrEmpty(info.enemyData.team) ? "Enemy" : info.enemyData.team;

            // 呼叫 GameManager 的 SpawnUnit（你原本的邏輯）
            gameManager.SpawnUnit(info.enemyData, gridX, gridZ, team);
        }
    }

    public void StopBattle()
    {
        if (runningBattle != null)
        {
            StopCoroutine(runningBattle);
            runningBattle = null;
        }
    }

    // ---------------- 戰鬥流程 ----------------
    private IEnumerator BattleRoutine(RoomData room)
    {
        Debug.Log($"[BattleManager] Start battle for Room: {room.id}, Type: {room.type}, Difficulty: {room.difficulty}");

        // 生成敵人 (改用 EnemyTemplate)
        if (room.enemyTemplates != null && room.enemyTemplates.Count > 0)
        {
            StartBattle(room); // 預設挑一個 template
        }

        // 等待戰鬥結束
        yield return StartCoroutine(WaitForBattleEnd());

        // 結算勝負
        bool playerWin = CheckPlayerWin();

        Debug.Log($"[BattleManager] Battle ended. PlayerWin = {playerWin}");

        runningBattle = null;
        OnBattleFinished?.Invoke(playerWin);
    }

    /// <summary>
    /// 等待直到戰鬥系統判定結束
    /// </summary>
    private IEnumerator WaitForBattleEnd()
    {
        while (gameManager.playerAliveCount > 0 && gameManager.enemyAliveCount > 0)
        {
            yield return null;
        }
    }

    private bool CheckPlayerWin()
    {
        return gameManager.playerAliveCount > 0 && gameManager.enemyAliveCount <= 0;
    }
}
