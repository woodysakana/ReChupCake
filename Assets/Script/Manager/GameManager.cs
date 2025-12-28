using UnityEngine; // 引用 Unity 引擎功能
using System.Collections.Generic; // 引用泛型集合


public enum GameState // 遊戲狀態枚舉
{
    LevelSelect,   // 新增：選擇關卡階段
    Placement,     // 佈置棋子階段
    Battle,        // 戰鬥階段
    Result         // 結果顯示
}

public class GameManager : MonoBehaviour // 遊戲管理器，負責控制遊戲流程
{
    public static GameManager Instance { get; private set; } // 單例模式，方便其他腳本取得 GameManager

    public GridManager gridManager; // 棋盤管理器，負責格子狀態

    public GameState currentState = GameState.LevelSelect; // 當前遊戲狀態，預設為選關階段
    public bool battleStarted = false; // 是否已開始戰鬥

    public List<Unit> allUnits = new List<Unit>(); // 所有棋子單位的列表

    // ----------------攝影機------------------
    public float cameraHeight = 9.5f;           // Y軸高度
    public float cameraZOffset = -3.5f;           // Z軸偏移（靠近我方）
    public Vector3 cameraRotation = new Vector3( 45f, 0f, 0f); // 俯視角度

    private CameraController cameraController;


    public GameObject enemyPrefab; // 在 Inspector 設定敵人預製物件
    public PlayerInventory playerInventory; // 在 Inspector 指定

    public UnitData enemyUnitData; // 在 Inspector 指定敵人資料
    public GameObject healthBarPrefab;// 血條預製物件
    public Canvas uiCanvas;// UI 畫布

    //---------------------存活計數器-------------------
    public int playerAliveCount = 0;
    public int enemyAliveCount = 0;
    private void Awake()
    {

        if (uiCanvas == null)
        {
            uiCanvas = FindFirstObjectByType<Canvas>();
        }
        // 確保只有一個 GameManager 實例（單例模式）
        if (Instance == null)
            Instance = this; // 設定自己為唯一實例
        else
            Destroy(gameObject); // 如果已存在，銷毀重複的物件
    }

    void Start()
    {
        // 遊戲開始時，進入佈置階段
        Debug.Log("<color=green>------------遊戲開始------------</color>"); 
        SetState(GameState.LevelSelect);
        // 找到攝影機上的 CameraController
        if (Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();

    }

    void Update()
    {
        switch (currentState)
        {
            case GameState.LevelSelect:
                // 顯示選關 UI，等待玩家選擇
                SetCameraToBoard();
                if (cameraController != null) cameraController.enabled = false;
                break;

            case GameState.Placement:
                // 玩家擺棋邏輯
                // 布陣階段：相機鎖定棋盤
                SetCameraToBoard();
                if (cameraController != null) cameraController.enabled = false;
                break;

            case GameState.Battle:
                // 戰鬥進行時
                // 戰鬥階段：啟用玩家控制
                if (cameraController != null) cameraController.enabled = true;
                break;

            case GameState.Result:
                // 顯示勝敗結果
                // 結算階段：相機還原固定
                SetCameraToBoard();
                if (cameraController != null) cameraController.enabled = false;
                break;
        }
    }

    // 設定遊戲狀態
    public void SetState(GameState newState)
    {
        currentState = newState; // 更新狀態


        if (newState == GameState.LevelSelect)
        {
            Debug.Log("<color=green>------------選擇關卡階段------------</color>");
            battleStarted = false; // 非戰鬥狀態
        }


        else if (newState == GameState.Placement)
        {
            Debug.Log("<color=green>------------佈置棋子階段------------</color>");
        }


        else if (newState == GameState.Battle)
        {
            battleStarted = true; // 標記戰鬥開始
            Debug.Log("<color=green>------------戰鬥開始------------</color>"); // 輸出訊息

            // 初始化存活計數器
            playerAliveCount = allUnits.FindAll(u => u != null && u.team == "Player" && u.health > 0).Count;
            enemyAliveCount = allUnits.FindAll(u => u != null && u.team == "Enemy" && u.health > 0).Count;
        }
        else
        {
            battleStarted = false; // 非戰鬥狀態
        }
    }


    public void SpawnUnit(UnitData unitData, int gridX, int gridZ, string team)
    {
        if (gridManager == null)
            gridManager = FindFirstObjectByType<GridManager>();

        //=======領地檢查=======
        if (team == "Player" && (gridZ < 0 || gridZ > 4))
        {
            Debug.LogWarning($"座標({gridX},{gridZ})不在我方領地，無法召喚角色！");
            return;
        }
        if (team == "Enemy" && (gridZ < 5 || gridZ > 9))
        {
            Debug.LogWarning($"座標({gridX},{gridZ})不在敵方領地，無法生成敵人！");
            return;
        }

        //=======格子佔用檢查=======
        if (gridManager.IsCellOccupied(gridX, gridZ))
        {
            Debug.LogWarning($"格子 ({gridX},{gridZ}) 已被佔用，無法生成單位。");
            return;
        }

        //=======背包檢查（只允許玩家放置自己擁有的單位）=======
        if (team == "Player" && playerInventory != null)
        {
            if (playerInventory.GetLevel(unitData) == 0)
            {
                Debug.LogWarning("背包沒有該單位，不能放置！");
                return;
            }
        }

        //=======同類單位檢查（玩家隊伍只能有一個同類單位）=======
        if (team == "Player")
        {
            // 🟢 檢查場上是否已存在同種類單位
            Unit existingUnit = allUnits.Find(u => u != null && u.team == team && u.unitData == unitData);
            if (existingUnit != null)
            {
                // 👉 已存在 → 移動到新位置
                gridManager.SetCellOccupied(existingUnit.gridX, existingUnit.gridZ, false); // 釋放舊格

                existingUnit.gridX = gridX;
                existingUnit.gridZ = gridZ;
                existingUnit.transform.position = gridManager.GetWorldPosition(gridX, gridZ);

                gridManager.SetCellOccupied(gridX, gridZ, true); // 佔用新格

                Debug.Log($"已存在同類單位，移動到新地點 ({gridX},{gridZ})");
                return;
            }
        }

        //=======重點生成部分=======
        Vector3 pos = gridManager.GetWorldPosition(gridX, gridZ);//得到位置
        GameObject obj = Instantiate(unitData.prefab, pos, Quaternion.identity);// 生成Unit
        Unit unit = obj.GetComponent<Unit>();// 給他 Unit 腳本
        //=========================
        if (unit != null)
        {
            // 設定單位屬性
            unit.team = team;
            unit.unitData = unitData;
            unit.gridX = gridX;
            unit.gridZ = gridZ;
            unit.gameManager = this;

            // 根據背包等級決定數值
            int level = (team == "Player" && playerInventory != null) ? playerInventory.GetLevel(unitData) : 1;
            unit.level = level; // 設定單位等級
            unit.maxHealth = unitData.maxHealth + (level - 1) * unitData.healthPerLevel;// 血量隨等級提升
            unit.attack = unitData.attack + (level - 1) * unitData.attackPerLevel;// 攻擊力隨等級提升
            unit.health = unit.maxHealth;// 初始血量等於最大血量
            unit.moveSpeed = unitData.moveSpeed;
            unit.attackRange = unitData.attackRange;// 攻擊範圍

            // 建立血條
            GameObject healthBarObj = Instantiate(healthBarPrefab, uiCanvas.transform);
            HealthBar healthBar = healthBarObj.GetComponent<HealthBar>();
            healthBar.Setup(unit);
            unit.healthBar = healthBar;


            // 登錄單位
            allUnits.Add(unit);
            gridManager.SetCellOccupied(gridX, gridZ, true);

            // ===== 套用玩家背包解鎖技能 =====
            if (team == "Player" && playerInventory != null)
            {
                List<AbilitySO> unlocked = playerInventory.GetUnlockedAbilities(unitData);
                foreach (var abilitySO in unlocked)
                {
                    // Unit 套用能力（CreateInstance）
                    unit.activeAbilities.Add(abilitySO.CreateInstance(unit));
                }
            }

            // 如果 Unit 有 mainAbility，套用它
            if (unitData.mainAbility != null)
            {
                unit.activeAbilities.Add(unitData.mainAbility.CreateInstance(unit));
            }


            Debug.Log($"生成 {unitData.prefab.name} 於格子 ({gridX},{gridZ}) world {pos} 等級 {level}");
        }
    }


    public void AssignTargets()
    {
        foreach (Unit unit in allUnits)
        {
            // 只分配存活單位
            if (unit == null || unit.health <= 0) continue;

            Unit nearest = null;
            int minDist = int.MaxValue;

            foreach (Unit other in allUnits)
            {
                // 只找敵隊且存活的單位
                if (other == null || other.health <= 0) continue;
                if (unit.team == other.team) continue;

                int dist = Mathf.Abs(unit.gridX - other.gridX) + Mathf.Abs(unit.gridZ - other.gridZ);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = other;
                }
            }

            if (nearest != null)
                unit.SetTarget(nearest);
        }
    }

    // UI 按鈕呼叫，開始戰鬥
    public void StartBattle()
    {
        if (currentState == GameState.Placement)
        {

            SetState(GameState.Battle); // 切換到戰鬥狀態
            UIManager.Instance.ClosePlaceUI(); // 隱藏佈置 UI
            AssignTargets();            // 分配目標
            Debug.Log("戰鬥開始！");
        }
    }

    // 結束戰鬥，進入結果狀態
    public void EndBattle()
    {
        SetState(GameState.Result); // 切換到結果狀態
        Debug.Log("戰鬥結束！"); // 輸出訊息

        ClearBoard(); // 新增：清空戰場
        UIManager.Instance.ClosePlaceUI(); // 隱藏佈置 UI
    }

    // 清空戰場
    public void ClearBoard()
    {
        var unitsToRemove = new List<Unit>(allUnits);

        foreach (var unit in unitsToRemove)
        {
            if (unit != null)
            {
                // 移除血條
                if (unit.healthBar != null)
                    Destroy(unit.healthBar.gameObject);

                // 移除單位物件
                Destroy(unit.gameObject);
            }
        }

        allUnits.Clear();

        // 清空棋盤格佔用狀態
        if (gridManager != null)
            gridManager.ClearAllOccupied();

        // 更新背包 UI（可選）
        FindFirstObjectByType<InventoryUI>()?.Refresh();
    }


    // 單位死亡時呼叫，移除棋子並檢查是否結束
    public void UnitDied(Unit unit)
    {
        if (allUnits.Contains(unit))
        {
            allUnits.Remove(unit);
            gridManager.SetCellOccupied(unit.gridX, unit.gridZ, false);
        }

        // 存活計數器遞減
        if (unit.team == "Player")
            playerAliveCount--;
        else if (unit.team == "Enemy")
            enemyAliveCount--;

        // 判斷勝負
        if (playerAliveCount <= 0 || enemyAliveCount <= 0)
        {
            bool playerWin = playerAliveCount > 0;

            if (BossBattleController.IsBossBattleActive)
            {
                BossBattleController.Instance.OnWaveEnded(playerWin);
                
            }
            else
            {
                // 一般關卡：結束戰鬥並進入下一關
                EndBattle();
                Debug.Log(playerWin ? "敵人全滅，戰鬥結束！" : "玩家全滅，戰鬥結束！");
                RoomManager.Instance.ShowNextRoomChoices();
            }
        }
    }

    // 設定攝影機到棋盤上方
    public void SetCameraToBoard()
    {
        if (Camera.main != null && gridManager != null)
        {
            // 修正 X 座標計算方式
            float camX = (gridManager.width - 1) * gridManager.cellSize / 2f;
            float camZ = cameraZOffset;
            float camY = cameraHeight;

            Camera.main.transform.position = new Vector3(camX, camY, camZ);
            Camera.main.transform.rotation = Quaternion.Euler(cameraRotation);
        }
    }

    // 升級指定UnitData的單位
    public void LevelUpUnit(UnitData unitData)
    {
        Unit unit = allUnits.Find(u => u != null && u.team == "Player" && u.unitData == unitData);
        if (unit != null)
        {
            Debug.Log($"GameManager: 找到單位 {unitData.unitName}，調用 LevelUp。");
            unit.LevelUp();
            unit.RefreshAbilities();
        }
    }
}
