using UnityEngine;

[System.Serializable]
public class EnemySpawnInfo
{
    public UnitData enemyData;   // 這隻敵人的資料
    public Vector2Int position;  // 生成座標（格子座標）
    public int level = 1;        // (可選) 初始等級

    // public GameObject prefab;    // (可選) 這隻敵人的專用 prefab

    // public string team = "Enemy";      // (可選) 所屬隊伍，預設 "Enemy"
    public bool isElite = false; // (可選) 是否為精英敵人
}
