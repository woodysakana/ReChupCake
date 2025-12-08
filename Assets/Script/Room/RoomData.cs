using System.Collections.Generic;
using UnityEngine;

public enum RoomType { Normal, Hard, Elite, Event, Shop, Reward, Boss }

// 改成 ScriptableObject，才能在 Unity 中建立資產
[CreateAssetMenu(fileName = "NewRoomData", menuName = "Game/Room Data")]
public class RoomData : ScriptableObject
{
    public string id;

    public string description;
    public RoomType type = RoomType.Normal;
    public int difficulty = 1; // 影響敵人等級或數量
    public List<UnitData> enemyUnits = new List<UnitData>(); // 直接連到你專案的 UnitData
    public bool cleared = false;

    // ✨ 改成：戰鬥用 EnemyTemplate
    public List<EnemyTemplate> enemyTemplates = new List<EnemyTemplate>();
}

// Boss 房間資料
[CreateAssetMenu(fileName = "NewBossRoomData", menuName = "Game/Boss Room Data")]
public class BossRoomData : RoomData {
    public int failCount = 0;

    // 每次失敗時呼叫以強化 Boss
    public void Strengthen() {
        failCount++;
        foreach (var u in enemyUnits) {
            u.attack += 2 * failCount;
            u.maxHealth += 10 * failCount;
        }
    }
}
