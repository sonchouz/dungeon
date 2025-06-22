using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.Tilemaps;

public class GungeonGenerator : MonoBehaviour
{
    public GameObject[] roomPrefabs;
    public Tilemap tunnelTilemap;
    public TileBase tunnelFloorTile;
    public TileBase tunnelWallTile;
    private Dictionary<Vector2Int, RoomInstance> roomMap = new();
    private List<Vector2Int> directions = new() { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
    public int desiredRoomCount = 10;
    [SerializeField] private NavMeshSurface navMeshSurface;

    private RoomInstance CreateRoom(Vector2Int position, RoomType type)
    {
        var prefab = Random.value > 0.7f ? roomPrefabs[1] : roomPrefabs[0];
        var worldPos = new Vector3(position.x * 20f, position.y * 20f, 0);
        var roomObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        roomObj.name = $"{type} Room ({position.x},{position.y})";
        var room = roomObj.GetComponent<RoomInstance>();
        room.Initialize(position, type);
        return room;
    }

    private void DrawHorizontalTunnel(int xStart, int xEnd, int yCenter)
    {
        for (int x = Mathf.Min(xStart, xEnd); x <= Mathf.Max(xStart, xEnd); x++)
        {
            tunnelTilemap.SetTile(new Vector3Int(x, yCenter + 2, 0), tunnelWallTile);
            tunnelTilemap.SetTile(new Vector3Int(x, yCenter - 2, 0), tunnelWallTile);
            for (int dy = -1; dy <= 1; dy++)
            {
                Vector3Int pos = new Vector3Int(x, yCenter + dy, 0);
                tunnelTilemap.SetTile(pos, tunnelFloorTile);
            }
        }
    }

    private void DrawVerticalTunnel(int yStart, int yEnd, int xCenter)
    {
        for (int y = Mathf.Min(yStart, yEnd); y <= Mathf.Max(yStart, yEnd); y++)
        {
            tunnelTilemap.SetTile(new Vector3Int(xCenter + 2, y, 0), tunnelWallTile);
            tunnelTilemap.SetTile(new Vector3Int(xCenter - 2, y, 0), tunnelWallTile);
            for (int dx = -1; dx <= 1; dx++)
            {
                Vector3Int pos = new Vector3Int(xCenter + dx, y, 0);
                tunnelTilemap.SetTile(pos, tunnelFloorTile);
            }
        }
    }

    private void CreateTunnel(Vector3Int from, Vector3Int to)
    {
        if (from.x == to.x) // Вертикальный туннель
        {
            int xCenter = from.x;
            int yMin = Mathf.Min(from.y, to.y);
            int yMax = Mathf.Max(from.y, to.y);
            DrawVerticalTunnel(yMin, yMax, xCenter);
        }
        else if (from.y == to.y) // Горизонтальный туннель
        {
            int yCenter = from.y;
            int xMin = Mathf.Min(from.x, to.x);
            int xMax = Mathf.Max(from.x, to.x);
            DrawHorizontalTunnel(xMin, xMax, yCenter);
        }
    }

    public void GenerateDungeon()
    {
        var center = Vector2Int.zero;
        roomMap[center] = CreateRoom(center, RoomType.Start);
        var frontier = new Queue<Vector2Int>(new[] { center });

        // Генерация комнат
        while (roomMap.Count < desiredRoomCount && frontier.Count > 0)
        {
            var currentPos = frontier.Dequeue();
            foreach (var dir in directions)
            {
                var newPos = currentPos + dir;
                if (roomMap.ContainsKey(newPos))
                    continue;
                if (Random.value > 0.3f)
                    continue;

                var newRoom = CreateRoom(newPos, RoomType.Normal);
                roomMap[newPos] = newRoom;
                frontier.Enqueue(newPos);

                // Устанавливаем двери
                var dirIndex = directions.IndexOf(dir);
                var oppIndex = directions.IndexOf(-dir);
                roomMap[currentPos].doors[dirIndex] = true;
                newRoom.doors[oppIndex] = true;
                if (roomMap.Count >= desiredRoomCount)
                    break;
            }
            if (frontier.Count == 0)
            {
                frontier.Enqueue(currentPos);
            }
        }
        // Создание туннелей между комнатами
        foreach (var (pos, room) in roomMap)
        {
            for (var i = 0; i < directions.Count; i++)
            {
                if (!room.doors[i])
                    continue;

                var neighborPos = pos + directions[i];
                if (roomMap.TryGetValue(neighborPos, out var neighbor))
                {
                    // Расчет позиций для туннеля
                    Vector3Int from, to;
                    if (directions[i].x != 0) // Горизонтальный туннель
                    {
                        int yCenter = pos.y * 20 - 3; // Середина высоты комнаты (9/2 ≈ 4)
                        from = new Vector3Int(pos.x * 20 - 2, yCenter, 0);
                        to = new Vector3Int(neighborPos.x * 20 - 4 + directions[i].x, yCenter, 0);
                    }
                    else // Вертикальный туннель
                    {
                        int xCenter = pos.x * 20 - 8; // Середина ширины комнаты (13/2 ≈ 6)
                        from = new Vector3Int(xCenter, pos.y * 20, 0);
                        to = new Vector3Int(xCenter, neighborPos.y * 20 + directions[i].y, 0);
                    }
                    CreateTunnel(from, to);
                }
            }
            room.CutDoorways(); // Вырезаем дверные проемы
        }

        // Построение навигационной сетки после завершения генерации
        if (navMeshSurface != null)
        {
            navMeshSurface.BuildNavMesh();
            Debug.Log("NavMesh построен успешно.");
        }
        else
        {
            Debug.LogWarning("NavMeshSurface не привязан в инспекторе!");
        }
    }

    private void Awake()
    {
        GenerateDungeon();
    }
}
