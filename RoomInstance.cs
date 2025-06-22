using UnityEngine.Tilemaps;
using UnityEngine;

public class RoomInstance : MonoBehaviour
{
    public Vector2Int gridPosition;
    public RoomType roomType;
    public bool[] doors = new bool[4];

    public Tilemap wallTilemap;
    public TileBase wallTile;

    [Header("Настройки проёма")]
    public int doorwayWidth = 3;
    public int wallThickness = 2;

    public void Initialize(Vector2Int gridPosition, RoomType roomType)
    {
        this.gridPosition = gridPosition;
        this.roomType = roomType;

        //DecorateRoom();
    }

    public void CutDoorways()
    {
        if (wallTilemap == null)
            wallTilemap = GetComponentInChildren<Tilemap>();

        if (wallTilemap == null)
            return;

        var bounds = wallTilemap.cellBounds;

        var leftT = wallThickness;
        var rightT = wallThickness;
        var topT = wallThickness + 2;
        var bottomT = wallThickness + 2;

        var insideXMin = bounds.xMin + leftT;
        var insideXMax = bounds.xMax - 1 - rightT;
        var insideYMin = bounds.yMin + bottomT;
        var insideYMax = bounds.yMax - 1 - topT;

        var insideW = insideXMax - insideXMin + 1;
        var insideH = insideYMax - insideYMin + 1;

        var doorStartX = insideXMin + (insideW - doorwayWidth) / 2;
        var doorStartY = insideYMin + (insideH - doorwayWidth) / 2;

        if (doors[0])
            CreateDoor(Direction.Up, doorStartX, insideYMax + 1, doorwayWidth, topT);

        if (doors[1])
            CreateDoor(Direction.Down, doorStartX, insideYMin - 1, doorwayWidth, bottomT);

        if (doors[2])
            CreateDoor(Direction.Left, insideXMin - 1, doorStartY, doorwayWidth, leftT);

        if (doors[3])
            CreateDoor(Direction.Right, insideXMax + 1, doorStartY, doorwayWidth, rightT);
    }

    private void CreateDoor(Direction direction, int startX, int startY, int width, int depth)
    {
        for (int d = 0; d < depth; d++)
        {
            for (int i = 0; i < width; i++)
            {
                Vector3Int tilePos = direction switch
                {
                    Direction.Up => new Vector3Int(startX + i, startY + d, 0),
                    Direction.Down => new Vector3Int(startX + i, startY - d, 0),
                    Direction.Left => new Vector3Int(startX - d, startY + i, 0),
                    Direction.Right => new Vector3Int(startX + d, startY + i, 0),
                    _ => Vector3Int.zero
                };

                wallTilemap.SetTile(tilePos, null);
            }
        }
    }

    private enum Direction { Up, Down, Left, Right }
}
