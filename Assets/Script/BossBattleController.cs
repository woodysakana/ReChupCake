using System.Collections;
using UnityEngine;

public class BossBattleController : MonoBehaviour
{
    [Header("References")]
    public BattleManager battleManager;
    public BossConfig bossConfig;
    public RoomData bossRoom; // Boss 房間的 RoomData（給 StartBattle 用）

    private int playerLives;
    private int bossLives;
    private int currentWaveIndex;

    public static BossBattleController Instance { get; private set; }
    public static bool IsBossBattleActive => Instance != null && Instance.isActiveAndEnabled && Instance.isInBossBattle;

    private bool isInBossBattle = false;

    private void Awake()
    {
        Instance = this;
    }

    public void StartBossBattle()
    {
        isInBossBattle = true;
        playerLives = bossConfig.initialLives;
        bossLives = bossConfig.initialLives;
        currentWaveIndex = 0;

        Debug.Log("[BossBattle] 開始 Boss 戰鬥！");
        StartCoroutine(BossWaveLoop());
    }

    private IEnumerator BossWaveLoop()
    {
        while (playerLives > 0 && bossLives > 0)
        {
            Debug.Log($"[BossBattle] 波次開始！Boss 血量 {bossLives} / 玩家血量 {playerLives}");

            // 1. 清空戰場
            battleManager.gameManager.ClearBoard();

            // 2. 進入佈陣階段
            UIManager.Instance.LetPlaceUI(); // ShowUI
            battleManager.gameManager.SetState(GameState.Placement);

            // 3. 等待玩家佈陣完成（玩家按下開始戰鬥）
            yield return new WaitUntil(() => battleManager.gameManager.currentState == GameState.Battle);

            // 4. 取得本波敵人模板
            EnemyTemplate template = GetBossTemplate(currentWaveIndex);

            // 5. 開始戰鬥（生成敵人並進入戰鬥）
            battleManager.StartBattle(bossRoom, template);

            // 6. 等待 GameManager 判定勝負並呼叫 OnWaveEnded
            yield return new WaitUntil(() => !isInBossBattle || waveEndedFlag);

            // 7. 等待結算動畫或提示
            yield return new WaitForSeconds(2f);

            // waveEndedFlag 會在 OnWaveEnded 被設為 true
            waveEndedFlag = false;
        }

        // 結算
        isInBossBattle = false;
        if (bossLives <= 0)
            Debug.Log("[BossBattle] 玩家擊敗 Boss！");
            
        else
            Debug.Log("[BossBattle] 玩家戰敗！");
        GameManager.Instance.EndBattle();
        RoomManager.Instance.ShowNextRoomChoices();
    }

    // 用於協程等待
    private bool waveEndedFlag = false;

    // GameManager 判定勝負後呼叫這個方法
    public void OnWaveEnded(bool playerWin)
    {
        if (!isInBossBattle) return;

        waveEndedFlag = true;

        if (playerWin)
        {
            bossLives--;
            currentWaveIndex++;
            Debug.Log("[BossBattle] 玩家贏了這一波！");
        }
        else
        {
            playerLives--;
            Debug.Log("[BossBattle] Boss 贏了這一波！");
        }

        // 判斷是否結束
        if (playerLives <= 0 || bossLives <= 0)
        {
            isInBossBattle = false;
        }
        // 若還沒結束，BossWaveLoop 會自動進入下一波
    }

    private EnemyTemplate GetBossTemplate(int waveIndex)
    {
        if (waveIndex >= bossConfig.waveSets.Count)
        {
            Debug.LogWarning("[BossBattle] 超出波次範圍，使用最後一組模板！");
            waveIndex = bossConfig.waveSets.Count - 1;
        }

        var set = bossConfig.waveSets[waveIndex];
        return set.templates[Random.Range(0, set.templates.Count)];
    }
}
