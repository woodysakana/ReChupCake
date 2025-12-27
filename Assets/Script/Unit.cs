using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{
    public enum UnitState { Idle, Move, Attack, Dead }
    private UnitState currentState = UnitState.Idle;

    [Header("屬性")]
    public string team;
    public int maxHealth = 10;   // 總血量
    public int health;           // 當前血量 (會變動)
    public int attack = 2;
    public float moveSpeed = 2f;

    [Header("能力")]
    // ===== Ability System (Unlock-based) =====
    public List<AbilityInstance> activeAbilities = new();
    public List<AbilitySO> unlockedUpgrades = new();

    public int level = 1;

    [Header("棋盤位置")]
    public int gridX;
    public int gridZ;

    [Header("戰鬥設定")]
    public float attackCooldown = 1f;
    public int attackRange = 1;
    private float lastAttackTime = 0f;

    private Unit target;
    private GridManager gridManager;
    public GameManager gameManager;

    private bool isRotating = false;
    public float rotateSpeed = 540f;

    public UnitData unitData; // 用來記錄這個單位的資料

    public HealthBar healthBar; // 單位綁定的血條


    void Start()
    {
        health = maxHealth;

        gridManager = FindFirstObjectByType<GridManager>();
        if (gameManager == null) gameManager = GameManager.Instance;

        if (gameManager == null) Debug.LogError("Unit 找不到 GameManager!");
        if (gridManager == null) Debug.LogError("Unit 找不到 GridManager!");

        // 初始化主技能
        if (unitData != null && unitData.mainAbility != null)
        {
            activeAbilities.Add(
                unitData.mainAbility.CreateInstance(this)
            );
        }

        // 初始化高級單位的解鎖能力（根據等級）
        for (int i = 1; i < level; i++)
        {
            if (i == 3 || i == 7 || i == 9)
            {
                // 假設升級選項有順序，並自動選擇第一個未解鎖的
                if (unitData.upgradeOptions.Count > unlockedUpgrades.Count)
                {
                    UnlockAbility(unitData.upgradeOptions[unlockedUpgrades.Count]);
                }
            }
        }

        // 刷新技能
        RefreshAbilities();
    }

    /// <summary>
    /// 刷新單位的技能（從 Inventory 獲取解鎖技能並應用）
    /// </summary>
    public void RefreshAbilities()
    {
        // 清空當前解鎖技能
        unlockedUpgrades.Clear();
        // 移除舊的 activeAbilities（保留主技能）
        activeAbilities.RemoveAll(abi => abi != activeAbilities[0]); // 假設第一個是主技能

        // 從 Inventory 獲取解鎖技能
        if (unitData != null && gameManager != null && gameManager.playerInventory != null)
        {
            List<AbilitySO> unlocked = gameManager.playerInventory.GetUnlockedAbilities(unitData);
            foreach (var abilitySO in unlocked)
            {
                UnlockAbility(abilitySO);
            }
        }

        Debug.Log($"{unitData.unitName} 技能已刷新。");
    }

    void Update()
    {
        if (currentState == UnitState.Dead) return;
        if (!gameManager.battleStarted) return;

        switch (currentState)
        {
            case UnitState.Idle: HandleIdle(); break;
            case UnitState.Move: break;
            case UnitState.Attack: break;
        }
    }

    void HandleIdle()
    {
        if (target == null) FindNewTarget();
        if (target == null) return;

        if (IsInValidAttackPosition(this, target))
        {
            if (Time.time - lastAttackTime >= attackCooldown)
                StartCoroutine(DoAttack());
        }
        else
        {
            if (currentState != UnitState.Idle) return;
            StartCoroutine(MoveTowardsTarget());
        }

    }

    IEnumerator MoveTowardsTarget()
    {
        currentState = UnitState.Move;

        if (target == null || gridManager == null)
        {
            currentState = UnitState.Idle;
            yield break;
        }

        if (IsInValidAttackPosition(this, target))
        {
            currentState = UnitState.Idle;
            yield break;
        }

        Vector2Int targetCell = FindAdjacentCellNearTarget();// 找到目標旁邊的格子
        if (targetCell == Vector2Int.one * -1)// 找不到可移動的格子
        {
            currentState = UnitState.Idle;
            yield break;
        }

        List<Vector2Int> path = gridManager.FindPath(gridX, gridZ, targetCell.x, targetCell.y);

        if (path.Count > 0)
        {
            var oneStep = new List<Vector2Int> { path[0] };
            yield return StartCoroutine(MoveAlongPath(oneStep));
        }

        currentState = UnitState.Idle;
    }

    IEnumerator DoAttack()
    {
        currentState = UnitState.Attack;
        
        yield return new WaitForSeconds(0.2f); // 模擬攻擊動畫

        if (target != null)
        {
            target.TakeDamage(attack);

            foreach (var ability in activeAbilities){
                ability.OnAttack(target);
            }
        }

        lastAttackTime = Time.time;

        float rest = Mathf.Max(0f, attackCooldown - 0.2f);
        yield return new WaitForSeconds(rest);

        currentState = UnitState.Idle;
    }

    void FindNewTarget()
    {
        Unit nearest = null;
        int minDist = int.MaxValue;
        foreach (Unit other in gameManager.allUnits)
        {
            if (other == null || other.health <= 0) continue;
            if (other.team == team) continue;

            int dist = Mathf.Abs(gridX - other.gridX) + Mathf.Abs(gridZ - other.gridZ);// 曼哈頓距離
            if (dist < minDist)
            {
                minDist = dist;
                nearest = other;
            }
        }
        if (nearest != null) SetTarget(nearest);
    }

    public void SetTarget(Unit enemy) => target = enemy;

    Vector2Int FindAdjacentCellNearTarget()
    {
        Vector2Int best = Vector2Int.one * -1;
        int bestDist = int.MaxValue;

        int[] dx = { 1, -1, 0, 0 };
        int[] dz = { 0, 0, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int nx = target.gridX + dx[i];
            int nz = target.gridZ + dz[i];

            if (gridManager.IsCellOccupied(nx, nz)) continue;

            int distToMe =
                Mathf.Abs(gridX - nx) + Mathf.Abs(gridZ - nz);

            if (distToMe < bestDist)
            {
                bestDist = distToMe;
                best = new Vector2Int(nx, nz);
            }
        }
        return best;
    }


    public void TakeDamage(int dmg)
    {
        if (currentState == UnitState.Dead) return;

        foreach (var ability in activeAbilities)
            ability.OnTakeDamage(ref dmg);

        health -= dmg;
        if (health <= 0) Die();
    }


    void Die()
    {
        Debug.Log($"{gameObject.name} 死亡！");
        currentState = UnitState.Dead;

        foreach (var ability in activeAbilities)
            ability.OnDeath();

        if (gameManager != null)
        {
            gameManager.UnitDied(this);
        }
        if (gridManager != null)
        {
            gridManager.SetCellOccupied(gridX, gridZ, false);
        }


        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        } // 刪除血條

        Destroy(gameObject);
    }

    IEnumerator RotateTowards(Vector3 direction)
    {
        isRotating = true;
        Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

        while (Quaternion.Angle(transform.rotation, targetRotation) > 1f)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
            yield return null;
        }

        transform.rotation = targetRotation;
        isRotating = false;
    }

    IEnumerator MoveAlongPath(List<Vector2Int> path)
    {
        foreach (var cell in path)
        {
            if (gridManager.IsCellOccupied(cell.x, cell.y)) yield break;

            gridManager.SetCellOccupied(cell.x, cell.y, true);

            int prevX = gridX;
            int prevZ = gridZ;

            Vector3 nextPos = gridManager.GetWorldPosition(cell.x, cell.y);

            Vector3 direction = (nextPos - transform.position).normalized;
            if (direction != Vector3.zero)
                yield return StartCoroutine(RotateTowards(direction));

            while (Vector3.Distance(transform.position, nextPos) > 0.01f)
            {
                transform.position = Vector3.MoveTowards(transform.position, nextPos, moveSpeed * Time.deltaTime);
                yield return null;
            }

            gridX = cell.x;
            gridZ = cell.y;
            gridManager.SetCellOccupied(prevX, prevZ, false);
        }
    }
    bool IsInValidAttackPosition(Unit self, Unit target)
    {
        int dx = Mathf.Abs(self.gridX - target.gridX);
        int dz = Mathf.Abs(self.gridZ - target.gridZ);

        // 切比雪夫距離內，且不是同格
        return Mathf.Max(dx, dz) <= attackRange && (dx + dz) > 0;
    }
    public void LevelUp()
    {
        level++;
        Debug.Log($"{unitData.unitName} 升級到等級 {level}！");

        // 基礎數值成長
        maxHealth += unitData.healthPerLevel;
        attack += unitData.attackPerLevel;
        health = maxHealth;

        // 特定等級觸發解鎖能力
        if (level == 3 || level == 7 || level == 9)
        {
            Debug.Log($"{unitData.unitName} 達到升級等級 {level}，準備顯示升級選項。");

            // 呼叫 UI 顯示可選能力
            List<AbilitySO> choices = new List<AbilitySO>();

            // 從 upgradeOptions 過濾掉已解鎖過的
            foreach (var option in unitData.upgradeOptions)
            {
                if (!unlockedUpgrades.Contains(option))
                    choices.Add(option);
            }

            Debug.Log($"{unitData.unitName} 可選升級數量：{choices.Count}");

            // 交給 UI 選擇
            UpgradeUI upgradeUI = FindFirstObjectByType<UpgradeUI>();
            if (upgradeUI != null)
            {
                Debug.Log($"{unitData.unitName} 找到 UpgradeUI，顯示升級選項。");
                upgradeUI.ShowUpgradeOptions(this, choices.ToArray());
            }
            else
            {
                Debug.LogError($"{unitData.unitName} 找不到 UpgradeUI！請檢查場景中是否有 UpgradeUI 腳本附加的 GameObject。");
            }
        }
        else
        {
            Debug.Log($"{unitData.unitName} 等級 {level} 不觸發升級選項。");
        }
    }
    /// <summary>
    /// 玩家選擇升級加成後呼叫
    /// </summary>
    public void UnlockAbility(AbilitySO abilitySO)
    {
        if (abilitySO == null) return;
        if (unlockedUpgrades.Contains(abilitySO)) return;

        unlockedUpgrades.Add(abilitySO);

        // 建立 Unit 專屬 Instance，加入 activeAbilities
        activeAbilities.Add(
            abilitySO.CreateInstance(this)
        );
        Debug.Log($"{unitData.unitName} 解鎖能力：{abilitySO.abilityName}");
    }

}
