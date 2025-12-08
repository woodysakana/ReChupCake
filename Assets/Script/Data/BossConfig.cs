using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/BossConfig")]
public class BossConfig : ScriptableObject
{
    public int initialLives = 3;
    public List<WaveSet> waveSets;

    [System.Serializable]
    public class WaveSet
    {
        public List<EnemyTemplate> templates;
    }
}