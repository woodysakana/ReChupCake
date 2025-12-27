using System.Collections.Generic;

using UnityEngine;
public enum Rarity { Common, Rare, Epic, Legendary }


[CreateAssetMenu(fileName = "NewUnitData", menuName = "AutoChess/UnitData")]
public class UnitData : ScriptableObject
{
    //資料
    public string unitName;
    public Sprite unitIcon;         // 新增角色圖片
    public string team = "Player"; // 預設隊伍為 Player

    // 基礎數值
    public int maxHealth;
    public int healthPerLevel;     // 每升一級增加的血量
    public int attackPerLevel;     // 每升一級增加的攻擊力
    public int attack;
    public float moveSpeed;
    public int attackRange;

    // 額外狀態
    public int shield;             // 护盾
    public float bonusAttackSpeed; // 額外攻速加成
    // 狀態布林
    public bool canMove = true;
    public bool canAttack = true;
    public bool canBeSeen = true;
    public bool canBeDamaged = true;

    

    public GameObject prefab;      // 這個棋子的預設 prefab
    public AbilitySO mainAbility;

    public List<AbilitySO> upgradeOptions; 
    // A, B, C 三選項


    public Rarity rarity;

    public int Cost
    {
        get
        {
            switch (rarity)
            {
                case Rarity.Common: return 2;
                case Rarity.Rare: return 3;
                case Rarity.Epic: return 4;
                case Rarity.Legendary: return 6;
                default: return 2;
            }
        }
    }
}
