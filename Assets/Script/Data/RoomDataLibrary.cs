using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Game/RoomDataLibrary")]
public class RoomDataLibrary : ScriptableObject
{
    public List<RoomData> normalRooms;
    public List<RoomData> eliteRooms;
    public List<RoomData> shopRooms;
    public List<RoomData> rewardRooms;
    public List<BossRoomData> bossRooms;
}