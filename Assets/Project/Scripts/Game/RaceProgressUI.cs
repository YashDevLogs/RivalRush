using System.Collections.Generic;
using UnityEngine;
using Game.Core;
using Game.Player;

namespace Game.Systems
{
    public sealed class RaceProgressUI : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RaceProgressManager raceProgressManager;
        [SerializeField] private RectTransform track;
        [SerializeField] private RectTransform markerContainer;
        [SerializeField] private RectTransform markerPrefab;

        private readonly Dictionary<IPlayerEntity, RectTransform> markers = new();

        private float trackWidth;

        [SerializeField]
        private AvatarDatabase avatarDatabase;

        #region Unity

        private void Start()
        {

            if (raceProgressManager == null)
                raceProgressManager = FindFirstObjectByType<RaceProgressManager>();

            trackWidth = track.rect.width;
        }

        private void Update()
        {
            UpdateMarkers();
        }

        #endregion

        #region Marker Creation

        #endregion

        #region Marker Update

        private void UpdateMarkers()
        {
            foreach (RaceProgressData racer in raceProgressManager.ProgressData)
            {
                // Create marker if it doesn't exist yet
                if (!markers.TryGetValue(racer.Entity, out RectTransform marker))
                {
                    marker = Instantiate(markerPrefab, markerContainer);
                    marker.gameObject.SetActive(true);

                    RaceProgressMarker markerUI = marker.GetComponent<RaceProgressMarker>();

                    if (markerUI == null)
                    {
                        Debug.LogError("RaceProgressMarker component is missing on the prefab!");
                        return;
                    }

                    PlayerIdentity identity = racer.Entity.Transform.GetComponent<PlayerIdentity>();

                    if (identity == null)
                    {
                        Debug.LogError($"PlayerIdentity not found on {racer.Entity.Transform.name}");
                        return;
                    }

                    markers.Add(racer.Entity, marker);

                    string playerName = identity.DisplayName;

                    Sprite avatar =
                        avatarDatabase.GetAvatar(identity.AvatarId);

                    markerUI.Setup(
                        avatar,
                        playerName);
                }

                Vector2 pos = marker.anchoredPosition;

                pos.x = racer.Progress * trackWidth;

                marker.anchoredPosition = pos;
            }
        }

        #endregion
    }
}