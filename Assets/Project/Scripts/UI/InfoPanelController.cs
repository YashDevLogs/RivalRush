using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace Game.UI
{
    public sealed class InfoPanelController : MonoBehaviour
    {
        #region Inspector

        [Header("Content")]
        [SerializeField] private RectTransform content;

        [Header("Navigation")]
        [SerializeField] private Button leftButton;
        [SerializeField] private Button rightButton;

        [Header("Page Indicators")]
        [SerializeField] private Image[] pageIndicators;

        [Header("Indicator Colors")]
        [SerializeField] private Color selectedColor = Color.white;
        [SerializeField] private Color unselectedColor = Color.gray;

        [Header("Animation")]
        [SerializeField] private float pageWidth = 800f;
        [SerializeField] private float slideDuration = 0.3f;

        #endregion

        #region Private Fields

        private int currentPage = 0;
        private bool isSliding;

        #endregion

        #region Unity

        private void Awake()
        {
            leftButton.onClick.AddListener(PreviousPage);
            rightButton.onClick.AddListener(NextPage);

            UpdateNavigation();
            UpdateIndicators();
        }

        private void OnDestroy()
        {
            leftButton.onClick.RemoveListener(PreviousPage);
            rightButton.onClick.RemoveListener(NextPage);
        }

        #endregion

        #region Navigation

        private void NextPage()
        {
            if (isSliding)
                return;

            if (currentPage >= pageIndicators.Length - 1)
                return;

            currentPage++;

            StartCoroutine(SlideToCurrentPage());
        }

        private void PreviousPage()
        {
            if (isSliding)
                return;

            if (currentPage <= 0)
                return;

            currentPage--;

            StartCoroutine(SlideToCurrentPage());
        }

        #endregion

        #region Animation

        private IEnumerator SlideToCurrentPage()
        {
            isSliding = true;

            UpdateNavigation();

            Vector2 start = content.anchoredPosition;
            Vector2 target = new Vector2(-currentPage * pageWidth, start.y);

            float elapsed = 0f;

            while (elapsed < slideDuration)
            {
                elapsed += Time.deltaTime;

                float t = Mathf.Clamp01(elapsed / slideDuration);

                // Ease Out Cubic
                float eased = 1f - Mathf.Pow(1f - t, 3f);

                content.anchoredPosition =
                    Vector2.Lerp(start, target, eased);

                yield return null;
            }

            content.anchoredPosition = target;

            isSliding = false;

            UpdateNavigation();
            UpdateIndicators();
        }

        #endregion

        #region UI

        private void UpdateNavigation()
        {
            leftButton.interactable = !isSliding && currentPage > 0;
            rightButton.interactable = !isSliding && currentPage < pageIndicators.Length - 1;
        }

        private void UpdateIndicators()
        {
            for (int i = 0; i < pageIndicators.Length; i++)
            {
                pageIndicators[i].color =
                    i == currentPage
                    ? selectedColor
                    : unselectedColor;
            }
        }

        #endregion

        #region Public

        /// <summary>
        /// Opens the info panel on the first page.
        /// </summary>
        public void ResetPages()
        {
            StopAllCoroutines();

            currentPage = 0;
            isSliding = false;

            content.anchoredPosition = Vector2.zero;

            UpdateNavigation();
            UpdateIndicators();
        }

        #endregion
    }
}