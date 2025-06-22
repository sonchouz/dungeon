using UnityEngine;
using UnityEngine.AI;

public static class NavMeshUtils
{
    public static Vector3 GetRandomPointInRoom(Collider2D room, float padding
= 1f, int attempts = 10)
    {
        var bounds = room.bounds;
        for (var i = 0; i < attempts; i++)
        {
            var x = Random.Range(bounds.min.x + padding, bounds.max.x -
            padding);
            var y = Random.Range(bounds.min.y + padding, bounds.max.y -
            padding);
            var candidate = new Vector2(x, y);
            if (room.OverlapPoint(candidate))
            {
                if (NavMesh.SamplePosition(candidate, out NavMeshHit hit,
                0.5f, NavMesh.AllAreas))
                    return hit.position;
            }
        }
        return room.bounds.center;
    }
 }
