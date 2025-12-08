using UnityEngine;
using System.Collections.Generic;

public class GridManager : MonoBehaviour
{
    [Header("Board")]
    public int width = 6;         // 棋盤寬
    public int height = 10;       // 棋盤高
    public float cellSize = 2f;   // 每格世界單位
    public GameObject cellPrefab; // 可選：格子預製（僅視覺化）

    // 佔用表：true = 被佔用 / 邊界外視為佔用
    private bool[,] occupied;

    public CameraController cameraController;

    [Header("Board Center")]
    public Transform boardCenter;   // 棋盤中心會在 GenerateBoard() 時生成


    void Start()
    {
        // 生成棋盤
        GenerateBoard();
    }
    void Awake()
    {
        occupied = new bool[width, height];
    }

    // === 位置/佔用 ===
    public Vector3 GetWorldPosition(int x, int z)
    {
        return new Vector3(x * cellSize, 0f, z * cellSize);
    }

    public bool IsCellOccupied(int x, int z)
    {
        if (x < 0 || x >= width || z < 0 || z >= height) return true; // 邊界外當作被佔用
        return occupied[x, z];
    }

    public void SetCellOccupied(int x, int z, bool value)
    {
        if (x < 0 || x >= width || z < 0 || z >= height) return;
        occupied[x, z] = value;
    }

    public void ClearAllOccupied()
    {
        if (occupied == null) return;
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                occupied[x, z] = false;
            }
        }
    }

    // === A* 尋路（4 向、Z 優先）===
    public List<Vector2Int> FindPath(int startX, int startZ, int targetX, int targetZ)
    {
        Vector2Int start = new Vector2Int(startX, startZ);
        Vector2Int target = new Vector2Int(targetX, targetZ);

        var openSet = new List<Vector2Int> { start };
        var cameFrom = new Dictionary<Vector2Int, Vector2Int>();
        var gScore = new Dictionary<Vector2Int, int>();
        var fScore = new Dictionary<Vector2Int, int>();

        gScore[start] = 0;
        fScore[start] = Heuristic(start, target);

        while (openSet.Count > 0)
        {
            // 取 fScore 最小；相同時用 Z 優先做 tie-break
            Vector2Int current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                Vector2Int n = openSet[i];

                int fN = fScore.TryGetValue(n, out var vN) ? vN : int.MaxValue;
                int fC = fScore.TryGetValue(current, out var vC) ? vC : int.MaxValue;

                if (fN < fC || (fN == fC && BetterZFirst(n, current, target)))
                    current = n;
            }

            if (current == target)
            {
                return ReconstructPath(cameFrom, start, current); // ✅ 不會把起點放進第一步
            }

            openSet.Remove(current);

            foreach (var neighbor in GetNeighbors(current, target))
            {
                // 不能穿過被佔用的格子；但終點可以是被佔用（例如你要撞到敵人旁格，或 UI 保證終點就是空的）
                if (IsCellOccupied(neighbor.x, neighbor.y) && neighbor != target)
                    continue;

                int tentativeG = gScore[current] + 1; // 單步成本
                bool hasG = gScore.TryGetValue(neighbor, out var gVal);

                if (!hasG || tentativeG < gVal)
                {
                    cameFrom[neighbor] = current;
                    gScore[neighbor] = tentativeG;
                    fScore[neighbor] = tentativeG + Heuristic(neighbor, target);

                    if (!openSet.Contains(neighbor))
                        openSet.Add(neighbor);
                }
            }
        }

        // 找不到路徑
        return new List<Vector2Int>();
    }

    // 曼哈頓距離：適用於上下左右移動
    int Heuristic(Vector2Int a, Vector2Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }

    // 取得鄰居：Z 軸優先（朝目標方向的 Z，反向 Z，然後 X，最後 X 反向）
    List<Vector2Int> GetNeighbors(Vector2Int node, Vector2Int target)
    {
        var neighbors = new List<Vector2Int>(4);

        int zDir = (target.y > node.y) ? 1 : -1; // 先朝目標 Z
        int xDir = (target.x > node.x) ? 1 : -1; // 再朝目標 X

        TryAdd(neighbors, node.x, node.y + zDir); // Z 朝目標
        TryAdd(neighbors, node.x, node.y - zDir); // Z 反向
        TryAdd(neighbors, node.x + xDir, node.y);        // X 朝目標
        TryAdd(neighbors, node.x - xDir, node.y);        // X 反向

        return neighbors;
    }

    void TryAdd(List<Vector2Int> list, int x, int z)
    {
        if (x >= 0 && x < width && z >= 0 && z < height)
            list.Add(new Vector2Int(x, z));
    }

    // fScore 相同時，誰「更減少與目標的 Z 差距」，誰優先；若 Z 一樣，再比 X
    bool BetterZFirst(Vector2Int a, Vector2Int b, Vector2Int target)
    {
        int dzA = Mathf.Abs(target.y - a.y);
        int dzB = Mathf.Abs(target.y - b.y);
        if (dzA != dzB) return dzA < dzB;

        int dxA = Mathf.Abs(target.x - a.x);
        int dxB = Mathf.Abs(target.x - b.x);
        return dxA < dxB;
    }

    // 反推路徑：回傳「從起點的下一格」開始，避免第一步是原地
    List<Vector2Int> ReconstructPath(Dictionary<Vector2Int, Vector2Int> cameFrom, Vector2Int start, Vector2Int current)
    {
        var path = new List<Vector2Int>();
        while (cameFrom.ContainsKey(current))
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();

        if (path.Count > 0 && path[0] == start)
            path.RemoveAt(0);

        return path;
    }

    // 產生棋盤（僅視覺化用）
    void GenerateBoard()
    {
        if (cellPrefab == null) return;

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 position = GetWorldPosition(x, z);
                GameObject cellObj = Instantiate(cellPrefab, position, Quaternion.identity, transform);
                cellObj.name = $"Cell_{x}_{z}";

                GridCellDrop drop = cellObj.GetComponent<GridCellDrop>();
                if (drop != null)
                {
                    // 覆寫每個實例的 grid 座標（非常重要）
                    drop.gridX = x;
                    drop.gridZ = z;
                }

                // 確保 occupied table 初始都是 false（若你有）
                if (occupied != null)
                    occupied[x, z] = false;
            }
        }
        CreateBoardCenter();
    }
    void CreateBoardCenter()
    {
        // 計算中心點座標
        Vector3 centerPos = new Vector3(
            (width - 1) * cellSize / 2f,
            0,
            (height - 1) * cellSize / 2f
        );

        // 如果之前已有 center，刪掉避免重複
        if (boardCenter != null)
        {
            DestroyImmediate(boardCenter.gameObject);
        }

        GameObject centerObj = new GameObject("BoardCenter");// 新建一個空物件當中心點
        centerObj.transform.SetParent(transform);
        centerObj.transform.position = centerPos;
        Debug.Log("棋盤中心座標: " + centerPos);

        boardCenter = centerObj.transform;

        cameraController.FindCenter(); // 通知 CameraController 更新中心點
    }



}
