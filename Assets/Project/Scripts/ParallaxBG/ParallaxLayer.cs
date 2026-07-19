using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;

namespace Game.Systems
{
    [System.Serializable]
    public sealed class ParallaxLayer : MonoBehaviour
    {
        [Header("Movement")]
        [Tooltip("0 = static, 1 = same speed as camera")]
        [Range(0f, 1f)]
        public float parallaxFactor = 0.5f;

        [Tooltip("Constant movement independent of player")]
        public float driftSpeed = 0f;

        [Header("Layer Content")]
        public List<Transform> items = new();

        private Vector3 lastCamPos;

        [SerializeField] private float verticalDriftAmplitude = 0.2f;
        [SerializeField] private float verticalDriftFrequency = 0.5f;


        private RectTransform rectTransform;
        private bool isUI;
        private Vector2 uiStartPosition;
        private float worldStartY;


        private float driftTime;

        public void Initialize(Vector3 camPos)
        {
            lastCamPos = camPos;

            rectTransform = GetComponent<RectTransform>();
            isUI = rectTransform != null;

            if (isUI)
            {
                uiStartPosition = rectTransform.anchoredPosition;
            }
            else
            {
                worldStartY = transform.localPosition.y;
            }
        }

        public void Tick(Vector3 camPos)
        {
            Vector3 delta = camPos - lastCamPos;

            if (isUI)
            {
                rectTransform.anchoredPosition += new Vector2(
                    delta.x * parallaxFactor,
                    0f
                );
            }
            else
            {
                transform.position += new Vector3(
                    delta.x * parallaxFactor,
                    0f,
                    0f
                );
            }

            if (verticalDriftAmplitude > 0f)
            {
                driftTime += Time.deltaTime;

                float yOffset =
                    Mathf.Sin(driftTime * verticalDriftFrequency)
                    * verticalDriftAmplitude;

                if (isUI)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        rectTransform.anchoredPosition.x,
                        uiStartPosition.y + yOffset
                    );
                }
                else
                {
                    transform.localPosition = new Vector3(
                        transform.localPosition.x,
                        worldStartY + yOffset,
                        transform.localPosition.z
                    );
                }
            }

            lastCamPos = camPos;
        }
    }

}