using Game.Systems;
using Game.Core;
using Game.Input;
using Game.Player;
using Game.AI;
using UnityEngine;
using System.Collections.Generic;

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

    private float driftTime;

        public void Initialize(Vector3 camPos)
        {
            lastCamPos = camPos;
        }

        public void Tick(Vector3 camPos)
    {
        Vector3 delta = camPos - lastCamPos;

        transform.position += new Vector3(
            delta.x * parallaxFactor,
            0f,
            0f
        );

        if (verticalDriftAmplitude > 0f)
        {
            driftTime += Time.deltaTime;
            float yOffset = Mathf.Sin(driftTime * verticalDriftFrequency) * verticalDriftAmplitude;
            transform.localPosition = new Vector3(
                transform.localPosition.x,
                yOffset,
                transform.localPosition.z
            );
        }

        lastCamPos = camPos;
    }
    }

}