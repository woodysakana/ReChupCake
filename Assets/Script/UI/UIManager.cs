using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("GameState UI")]
    public GameObject PlacePanel;
    public GameObject StartPanel;

    [Header("Map UI")]
    public GameObject MapPanel;

    [Header("Store UI")]
    public GameObject StorePanel;
    public GameObject StoreButton;

    [Header("Event UI")]

    public GameObject EventPanel;


    //public GameObject inventoryPanel;
    // public GameObject levelSelectPanel;
    // public GameObject resultPanel;

    void Awake()
    {
        Instance = this;
    }


    public void OpenPlaceUI()//選關結束，開始佈置
    {
        PlacePanel.SetActive(true);
    }

    public void ClosePlaceUI()//結束佈置，開始戰鬥
    {
        PlacePanel.SetActive(false);
    }

    public void CloseStartPanel()//關閉開始介面
    {
        StartPanel.SetActive(false);
    }

    public void OpenStorePanel()//開啟商店頁面
    {
        StorePanel.SetActive(true);
        StoreButton.SetActive(false);
    }

    public void CloseStorePanel()//關閉商店頁面
    {
        StorePanel.SetActive(false);
        StoreButton.SetActive(true);
    }

    public void CloseMapPanel()
    {
        MapPanel.SetActive(false);
    }
    public void OpenMapPanel()
    {
        MapPanel.SetActive(true);
    }


    public void OpenEventPanel()
    {
        EventPanel.SetActive(true);
    }


    public void CloseEventPanel()
    {
        EventPanel.SetActive(false);
    }

    // // 顯示指定面板
    // public void Something(GameObject panel)
    // {
    //     if (panel != null)
    //         panel.SetActive(true);
    // }

    // // 隱藏所有面板
    // public void HideAllPanels()
    // {
    //     if (inventoryPanel != null) inventoryPanel.SetActive(false);
    //     if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
    //     if (resultPanel != null) resultPanel.SetActive(false);
    // }

    // // 更新背包資訊（呼叫 InventoryUI 的 Refresh）
    // public void RefreshInventory()
    // {
    //     var inventoryUI = FindObjectOfType<InventoryUI>();
    //     if (inventoryUI != null)
    //         inventoryUI.Refresh();
    // }
}