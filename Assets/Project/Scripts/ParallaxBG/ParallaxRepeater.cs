using UnityEngine;
using System.Collections.Generic;

public sealed class ParallaxRepeater : MonoBehaviour
{
    private Camera cam;
    private readonly List<Transform> tiles = new();
    private float tileWidth;

    private void Awake()
    {
        cam = Camera.main;

        tiles.Clear();
        foreach (Transform child in transform)
            tiles.Add(child);

        if (tiles.Count < 2)
        {
            Debug.LogWarning("[ParallaxRepeater] Need at least 2 tiles.");
            enabled = false;
            return;
        }

        var sr = tiles[0].GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogError("[ParallaxRepeater] Tile has no SpriteRenderer.");
            enabled = false;
            return;
        }

        tileWidth = sr.bounds.size.x;
    }

    private void LateUpdate()
    {
        float camX = cam.transform.position.x;

        Transform leftMost = GetLeftMostTile();

        if (camX - leftMost.position.x > tileWidth * 1.5f)
        {
            MoveTileToFront(leftMost);
        }
    }

    private void MoveTileToFront(Transform tile)
    {
        Transform rightMost = GetRightMostTile();

        tile.position = new Vector3(
            rightMost.position.x + tileWidth,
            tile.position.y,
            tile.position.z
        );
    }

    private Transform GetLeftMostTile()
    {
        Transform leftMost = tiles[0];

        foreach (var tile in tiles)
        {
            if (tile.position.x < leftMost.position.x)
                leftMost = tile;
        }

        return leftMost;
    }

    private Transform GetRightMostTile()
    {
        Transform rightMost = tiles[0];

        foreach (var tile in tiles)
        {
            if (tile.position.x > rightMost.position.x)
                rightMost = tile;
        }

        return rightMost;
    }
}
