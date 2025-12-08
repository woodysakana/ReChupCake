// MapGenerator.cs
using System.Collections.Generic;
using UnityEngine;

public static class MapGenerator
{
    // 產生簡單 floor：給定 rows (不含 boss row)，每列隨機 1~maxNodes nodes，最後一列為 boss
    public static List<List<RoomData>> GenerateFloor(
        int rows,
        int minNodes,
        int maxNodes,
        RoomDataLibrary roomLibrary // 新增參數
    ) {
        List<List<RoomData>> floor = new List<List<RoomData>>();
        System.Random r = new System.Random();

        for (int i = 0; i < rows; i++) {
            int nodes = r.Next(minNodes, maxNodes + 1);
            List<RoomData> row = new List<RoomData>();
            for (int j = 0; j < nodes; j++) {
                RoomData template = null;
                float p = (float)r.NextDouble();
                if (p < 0.1f && roomLibrary.shopRooms.Count > 0)
                    template = roomLibrary.shopRooms[r.Next(roomLibrary.shopRooms.Count)];
                else if (p < 0.25f && roomLibrary.rewardRooms.Count > 0)
                    template = roomLibrary.rewardRooms[r.Next(roomLibrary.rewardRooms.Count)];
                else if (p < 0.35f && roomLibrary.eliteRooms.Count > 0)
                    template = roomLibrary.eliteRooms[r.Next(roomLibrary.eliteRooms.Count)];
                else if (roomLibrary.normalRooms.Count > 0)
                    template = roomLibrary.normalRooms[r.Next(roomLibrary.normalRooms.Count)];

                if (template != null) {
                    RoomData room = UnityEngine.Object.Instantiate(template);
                    room.id = $"R_{i}_{j}";
                    room.difficulty = 1 + i;
                    row.Add(room);
                }
            }
            floor.Add(row);
        }

        // Boss row
        List<RoomData> bossRow = new List<RoomData>();
        if (roomLibrary.bossRooms.Count > 0) {
            BossRoomData bossTemplate = roomLibrary.bossRooms[r.Next(roomLibrary.bossRooms.Count)];
            BossRoomData boss = UnityEngine.Object.Instantiate(bossTemplate);
            boss.id = "Boss";
            boss.difficulty = rows + 1;
            bossRow.Add(boss);
        }
        floor.Add(bossRow);

        return floor;
    }
}
