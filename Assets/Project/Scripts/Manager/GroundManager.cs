using System.Collections.Generic;
using UnityEngine;

public class GroundManager : MonoBehaviour
{
    public List<Transform> groundSegments;
    public float segmentLength = 20f;
    public Transform player;

    [Header("Obstacle Settings")]
    public Transform obstacleParent;
    public List<GameObject> obstaclePrefabs;
    public float minObstacleSpacing = 4f;
    public float maxObstacleSpacing = 10f;

    private float lastGroundEndX = 0f;

    void Start()
    {
        // Initialize lastGroundEndX
        lastGroundEndX = groundSegments[groundSegments.Count - 1].position.x + segmentLength;
    }

    void Update()
    {
        foreach (var segment in groundSegments)
        {
            if (segment.position.x + segmentLength < player.position.x)
            {
                float newX = lastGroundEndX;
                segment.position = new Vector3(newX, segment.position.y, segment.position.z);

                // Spawn obstacles on this new segment
                GenerateObstaclesForSegment(newX);

                lastGroundEndX += segmentLength;
            }
        }
    }

    void GenerateObstaclesForSegment(float startX)
    {
        float xPos = startX + 2f; // slight offset from edge
        float endX = startX + segmentLength - 2f;

        while (xPos < endX)
        {
            // Randomly decide if we place an obstacle or a gap
            float chance = Random.Range(0f, 1f);

            if (chance < 0.2f)
            {
                // GAP (pit)
                xPos += Random.Range(3f, 6f);

                continue;
            }
            else
            {
                // OBSTACLE
                GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
                Instantiate(prefab, new Vector3(xPos, -1f, 0), Quaternion.identity, obstacleParent);
            }

            // Next obstacle spacing
            xPos += Random.Range(minObstacleSpacing, maxObstacleSpacing);
        }
    }
}
