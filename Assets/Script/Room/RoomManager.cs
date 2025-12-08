// RoomManager.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }

    [Header("關卡設定")]
    public int playerLives = 3;
    public List<List<RoomData>> currentFloor; // 節點式地圖，每列是 row
    public RoomData currentRoom;

    [Header("測試/設定")]
    public int rows = 4; // 生成列數（不包含 Boss 那列）
    public int minNodesPerRow = 1;
    public int maxNodesPerRow = 3;
    public GridManager gridManager;
    public RoomDataLibrary roomLibrary;

    public BossBattleController bossBattleController;

    [Header("遊戲進度")]
    public int totalFloors = 3; // 總層數
    private int currentFloorIndex = 0; // 目前第幾層（從0開始）

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (BattleManager.Instance != null)
        {
            BattleManager.Instance.OnBattleFinished += OnBattleEnd;
        }
        else
        {
            Debug.LogWarning("[RoomManager] Awake: BattleManager.Instance is null");
        }
    }
    private void OnDisable()
    {
        if (BattleManager.Instance != null)
            BattleManager.Instance.OnBattleFinished -= OnBattleEnd;
    }

    public void MapStart()
    {
        // 測試：啟動一個 floor
        currentFloor = MapGenerator.GenerateFloor(rows, minNodesPerRow, maxNodesPerRow, roomLibrary);
        EnterFirstRoom();
    }

    #region 地圖/路線相關（非常簡單的節點地圖）
    // 取得下一列節點（如果有）
    public List<RoomData> GetNextRow(int currentRowIndex)
    {
        if (currentFloor == null) return null;
        int next = currentRowIndex + 1;
        if (next < currentFloor.Count) return currentFloor[next];
        return null;
    }

    // 取得目前房間在 floor 的座標 (row, col)
    public (int row, int col) FindRoomPosition(RoomData room)
    {
        for (int r = 0; r < currentFloor.Count; r++)
        {
            for (int c = 0; c < currentFloor[r].Count; c++)
            {
                if (currentFloor[r][c] == room) return (r, c);
            }
        }
        return (-1, -1);
    }
    #endregion

    #region 進入與切換房間
    public void EnterFirstRoom()
    {
        if (currentFloor == null || currentFloor.Count == 0)
        {
            Debug.LogError("[RoomManager] currentFloor is null or empty");
            return;
        }
        currentRoom = currentFloor[0][0];
        EnterRoom(currentRoom);
    }

    public void EnterRoom(RoomData room)
    {
        currentRoom = room;
        Debug.Log($"[RoomManager]<color=aqua>進入房間{room.id}:{room.description} 種類:{room.type}</color>");
        
        // 根據房間類型選擇處理
        switch (room.type)
        {
            case RoomType.Shop:
                OpenShop(room);
                break;
            case RoomType.Reward:
                GrantReward(room);
                break;
            case RoomType.Event:
                OpenEvent(room);
                break;
            case RoomType.Normal:
                UIManager.Instance.CloseMapPanel();
                StartBattleRoom(room);
                break;
            case RoomType.Hard:
                UIManager.Instance.CloseMapPanel();
                StartBattleRoom(room);
                break;
            case RoomType.Elite:
                UIManager.Instance.CloseMapPanel();
                StartBattleRoom(room);
                break;
            case RoomType.Boss:
                UIManager.Instance.CloseMapPanel();
                // bossBattleController.StartBossBattle(FindRoomPosition(room).row);
                bossBattleController.StartBossBattle();
                break;
            default:
                Debug.LogWarning($"[RoomManager] 未處理的房間類型: {room.type}");
                break;
        }
    }


    private void OpenShop(RoomData room)
    {
        Debug.Log("[RoomManager] Open Shop UI (placeholder)");
        // TODO: 開商店 UI，購買後再 ShowNextRoomChoices()
        ShowNextRoomChoices();
    }

    private void GrantReward(RoomData room)
    {
        Debug.Log("[RoomManager] Grant simple reward (placeholder)");
        // TODO: 回復血量、給道具等
        room.cleared = true;
        ShowNextRoomChoices();
    }
    #endregion

    #region 戰鬥結果處理
    // BattleManager 會在戰鬥結束時呼叫此方法（註冊於 OnEnable）
    public void OnBattleEnd(bool playerWin)
    {
        Debug.Log($"[RoomManager] OnBattleEnd playerWin:{playerWin} room:{currentRoom.id}");
        if (playerWin)
        {
            currentRoom.cleared = true;
            GiveReward(currentRoom);
            ShowNextRoomChoices();
        }
        else
        {
            playerLives--;
            Debug.Log($"[RoomManager] Player lost! Remaining lives: {playerLives}");
            if (playerLives <= 0)
            {
                GameOver();
                return;
            }

            // 如果當前房間是 Boss，強化 Boss
            if (currentRoom is BossRoomData boss)
            {
                boss.Strengthen();
                Debug.Log("[RoomManager] Boss strengthened due to failure.");
            }

            // 回到本層起點（此處簡化為第一個 node）
            MoveToFloorStart();
            // 更新 UI -> 這裡你可以刷新節點地圖顯示
        }
    }
    #endregion

    #region 獎勵與下一步
    private void GiveReward(RoomData room)
    {
        // TODO: 根據房間類型與難度給予獎勵（貨幣、道具、抽卡、棋子等）
        Debug.Log($"[RoomManager] Give reward for room {room.id} (type:{room.type})");
    }

    public void ShowNextRoomChoices()
    {
        var pos = FindRoomPosition(currentRoom);
        int currentRow = pos.row;
        List<RoomData> nextRow = (currentRow >= 0) ? GetRow(currentRow + 1) : null;

        if (nextRow == null || nextRow.Count == 0)
        {
            Debug.Log("[RoomManager]<color=orange> 本層已結束，準備進入下一層</color>");
            
            EnterNextFloor();
            return;
        }

        // 從 nextRow 中選 2~3 個可選房間（範例：隨機取 2 個）
        List<RoomData> options = new List<RoomData>();
        System.Random rnd = new System.Random();
        int count = Mathf.Min(2, nextRow.Count);
        while (options.Count < count)
        {
            var candidate = nextRow[rnd.Next(0, nextRow.Count)];
            if (!options.Contains(candidate) && !candidate.cleared) options.Add(candidate);
            // 若 nextRow 裡全部 cleared 就直接加回（為避免無法選）
            if (options.Count == 0 && nextRow.Count > 0) options.Add(nextRow[0]);
        }

        // TODO: 顯示 UI，讓玩家選擇 options 其中一個
        Debug.Log("[RoomManager]<color=yellow>下個房間選項</color>:");
        foreach (var o in options)
        {
            Debug.Log($" - {o.id} ({o.type})");
        }

        // 為了測試，我們直接自動進入第一個選項（你要改成 UI 選擇）
        EnterRoom(options[0]);
    }
    #endregion

    #region 幫助方法
    public List<RoomData> GetRow(int rowIndex)
    {
        if (currentFloor == null) return null;
        if (rowIndex < 0 || rowIndex >= currentFloor.Count) return null;
        return currentFloor[rowIndex];
    }

    private void MoveToFloorStart()
    {
        if (currentFloor == null || currentFloor.Count == 0) return;
        currentRoom = currentFloor[0][0];
        Debug.Log("[RoomManager] Move player back to floor start.");
        // 你可以保留/清除玩家在場上的臨時狀態（例如臨時 Buff）
        // 直接進入起點房（或顯示選房 UI）
        EnterRoom(currentRoom);
    }

    private void GameOver()
    {
        Debug.Log("[RoomManager] Game Over - player has no lives left.");
        // TODO: 跳到 GameOver 畫面或顯示結算
    }
    #endregion

    private void OpenEvent(RoomData room)
    {
        Debug.Log("[RoomManager] Trigger Event (placeholder)");
        UIManager.Instance.OpenEventPanel();
        room.cleared = true;

    }

    public void CloseEvent(){
        Debug.Log("[RoomManager] Close Event UI (placeholder)");
        UIManager.Instance.CloseEventPanel();
        ShowNextRoomChoices();
    }

    private void StartBattleRoom(RoomData room)
    {
        if (room.enemyTemplates == null || room.enemyTemplates.Count == 0)
        {
            Debug.LogError($"[RoomManager] 房間 {room.id} 沒有 EnemyTemplate，無法開始戰鬥！");
            return;
        }

        // 隨機挑一個模板
        EnemyTemplate chosen = room.enemyTemplates[UnityEngine.Random.Range(0, room.enemyTemplates.Count)];
        Debug.Log($"[RoomManager] 房間 {room.id} 使用模板 {chosen.templateId}");

        // 丟給 BattleManager
        BattleManager.Instance.StartBattle(room, chosen);
    }

    public void EnterNextFloor()
    {
        currentFloorIndex++;
        if (currentFloorIndex >= totalFloors)
        {
            Debug.Log("[RoomManager]<color=lime>已通過所有樓層，遊戲結束！</color>");
            GameOver(); // 跳到結算或遊戲結束畫面
            return;
        }

        Debug.Log($"[RoomManager]<color=lime> 進入第 {currentFloorIndex + 1} 層！</color>");
        currentFloor = MapGenerator.GenerateFloor(rows, minNodesPerRow, maxNodesPerRow, roomLibrary);
        EnterFirstRoom();
    }

}
