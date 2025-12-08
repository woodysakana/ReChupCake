using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// =================== 能力系統 ===================
public interface IAbility
{
    void OnAttack(Unit self, Unit target);           // 攻擊時觸發
    void OnTakeDamage(Unit self, ref int dmg);       // 受到傷害時觸發（可修改傷害值）
    void OnDeath(Unit self);                          // 死亡時觸發
}

// 範例能力：攻擊時額外造成 +2 傷害
public class BonusDamageAbility : IAbility
{
    public int bonusDamage = 2;
    public void OnAttack(Unit self, Unit target)
    {
        if (target != null) target.TakeDamage(bonusDamage);
        Debug.Log(self.name + " dealt an extra " + bonusDamage + " damage to " + target.name);
    }
    public void OnTakeDamage(Unit self, ref int dmg) { }
    public void OnDeath(Unit self) { }
}

// 新增技能 class，必須實作 IAbility
public class HealOnAttack : IAbility
{
    public int healAmount = 2;  // 每次攻擊回血數量

    // 攻擊時觸發
    public void OnAttack(Unit self, Unit target)
    {
        self.health = Mathf.Min(self.maxHealth, self.health + healAmount); // 攻擊後回血
        Debug.Log(self.name + " healed " + healAmount + " HP!");
    }

    // 受到傷害時觸發（可修改 dmg）
    public void OnTakeDamage(Unit self, ref int dmg) { }

    // 死亡時觸發
    public void OnDeath(Unit self) { }
}


// 範例能力：死亡時爆炸，對周圍敵人造成範圍傷害
public class DeathExplosionAbility : IAbility
{
    public int explosionDamage = 3;
    public int radius = 1;

    public void OnAttack(Unit self, Unit target) { }
    public void OnTakeDamage(Unit self, ref int dmg) { }
    public void OnDeath(Unit self)
    {
        GameManager gm = GameManager.Instance;
        List<Unit> targets = new List<Unit>();

        // 先收集要爆炸傷害的目標
        foreach (var unit in gm.allUnits)
        {
            if (unit == null || unit.team == self.team) continue;
            int dist = Mathf.Max(Mathf.Abs(self.gridX - unit.gridX), Mathf.Abs(self.gridZ - unit.gridZ));
            if (dist <= radius)
            {
                targets.Add(unit);
            }
        }

        // 再對收集到的目標造成傷害
        foreach (var unit in targets)
        {
            unit.TakeDamage(explosionDamage);
            Debug.Log(unit.name + " took " + explosionDamage + " explosion damage from " + self.name);
        }
    }
}

// ===================================================

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
    public List<IAbility> abilities = new List<IAbility>(); // 用程式化能力

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

        float gridDist = Mathf.Max(Mathf.Abs(gridX - target.gridX), Mathf.Abs(gridZ - target.gridZ));

        if (gridDist <= 1)
        {
            if (Time.time - lastAttackTime >= attackCooldown)
                StartCoroutine(DoAttack());
        }
        else
        {
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

        int dist = Mathf.Abs(gridX - target.gridX) + Mathf.Abs(gridZ - target.gridZ);
        if (dist <= 1)
        {
            currentState = UnitState.Idle;
            yield break;
        }

        Vector2Int targetCell = FindAdjacentCellNearTarget();
        if (targetCell == Vector2Int.one * -1)
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

            foreach (var ability in abilities)
                ability.OnAttack(this, target);
        }

        lastAttackTime = Time.time;

        yield return new WaitForSeconds(attackCooldown - 0.2f);

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

            int dist = Mathf.Abs(gridX - other.gridX) + Mathf.Abs(gridZ - other.gridZ);
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
        int[] dx = { 1, -1, 0, 0 };
        int[] dz = { 0, 0, 1, -1 };

        for (int i = 0; i < 4; i++)
        {
            int nx = target.gridX + dx[i];
            int nz = target.gridZ + dz[i];
            if (!gridManager.IsCellOccupied(nx, nz))
                return new Vector2Int(nx, nz);
        }
        return Vector2Int.one * -1;
    }

    public void TakeDamage(int dmg)
    {
        if (currentState == UnitState.Dead) return;

        foreach (var ability in abilities)
            ability.OnTakeDamage(this, ref dmg);

        health -= dmg;
        if (health <= 0) Die();
    }

    void Die()
    {
        Debug.Log($"{gameObject.name} 死亡！");
        currentState = UnitState.Dead;

        foreach (var ability in abilities)
            ability.OnDeath(this);

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
    // public void SetData(UnitData data) {
    //     this.unitName = data.unitName;
    //     this.maxHealth = data.maxHealth;
    //     this.health = data.maxHealth;
    //     this.attack = data.attack;
    //     // 其他屬性...
    //     }

}
