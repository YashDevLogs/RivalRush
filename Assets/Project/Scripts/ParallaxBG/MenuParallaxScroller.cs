using System.Collections.Generic;
using UnityEngine;

namespace Game.Systems
{
    public sealed class MenuParallaxScroller : MonoBehaviour
    {
        [Header("Scrolling")]
        [SerializeField]
        [Tooltip("Pixels per second.")]
        private float scrollSpeed = 30f;

        [SerializeField]
        private bool scrollLeft = true;

        [Header("Vertical Drift")]
        [SerializeField]
        private float verticalDriftAmplitude = 0f;

        [SerializeField]
        private float verticalDriftFrequency = 0f;

        private readonly List<RectTransform> _tiles = new();
        private readonly List<float> _startY = new();

        private float _driftTime;

        #region Unity

        private void Awake()
        {
            _tiles.Clear();
            _startY.Clear();

            foreach (Transform child in transform)
            {
                if (!child.TryGetComponent(out RectTransform rect))
                    continue;

                _tiles.Add(rect);
                _startY.Add(rect.anchoredPosition.y);
            }

            if (_tiles.Count == 0)
            {
                Debug.LogWarning($"[{name}] No UI children found.");
                enabled = false;
            }
        }

        private void Update()
        {
            float direction = scrollLeft ? -1f : 1f;
            float move = direction * scrollSpeed * Time.deltaTime;

            _driftTime += Time.deltaTime;

            for (int i = 0; i < _tiles.Count; i++)
            {
                RectTransform tile = _tiles[i];

                Vector2 pos = tile.anchoredPosition;
                pos.x += move;

                if (verticalDriftAmplitude > 0f)
                {
                    pos.y = _startY[i] +
                            Mathf.Sin((_driftTime + i) * verticalDriftFrequency)
                            * verticalDriftAmplitude;
                }

                tile.anchoredPosition = pos;
            }

            RecycleTiles();
        }

        #endregion

        #region Recycling

        private void RecycleTiles()
        {
            RectTransform leftMost = GetLeftMost();
            RectTransform rightMost = GetRightMost();

            float leftWidth =
                leftMost.rect.width * leftMost.lossyScale.x;

            if (leftMost.anchoredPosition.x <= -leftWidth)
            {
                float rightWidth =
                    rightMost.rect.width * rightMost.lossyScale.x;

                leftMost.anchoredPosition = new Vector2(
                    rightMost.anchoredPosition.x + rightWidth,
                    leftMost.anchoredPosition.y
                );
            }
        }

        private RectTransform GetLeftMost()
        {
            RectTransform result = _tiles[0];

            foreach (var tile in _tiles)
            {
                if (tile.anchoredPosition.x < result.anchoredPosition.x)
                    result = tile;
            }

            return result;
        }

        private RectTransform GetRightMost()
        {
            RectTransform result = _tiles[0];

            foreach (var tile in _tiles)
            {
                if (tile.anchoredPosition.x > result.anchoredPosition.x)
                    result = tile;
            }

            return result;
        }

        #endregion
    }
}