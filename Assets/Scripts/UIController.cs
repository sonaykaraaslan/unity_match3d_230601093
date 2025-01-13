using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Match.View
{
    /// <summary>
    /// Oyun UI sistemini kontrol eden ana sýnýf
    /// </summary>
    public class UIController : MonoBehaviour
    {
        [Header("Score UI")]
        [SerializeField] private TMP_Text scoreText;
        [SerializeField] private Image objectFillImage;

        [Header("Button References")]
        [SerializeField] private Button resetButton;
        [SerializeField] private Button zoomSkillButton;
        [SerializeField] private Button hintSkillButton;

        [Header("Button Texts")]
        [SerializeField] private TMP_Text zoomButtonText;
        [SerializeField] private TMP_Text hintButtonText;

        [Header("Skill Settings")]
        [SerializeField] private float zoomCooldown = 8f;
        [SerializeField] private float hintCooldown = 30f;
        [SerializeField] private float scorePopupDuration = 0.1f;
        [SerializeField] private float fillAnimationDuration = 0.3f;

        // Private variables
        private readonly string _scoreTextFormat = "Score: {0}";
        private ItemSpawner _itemSpawner;
        private int _score = 0;

        // Skill states
        private bool isZoomSkillReady = true;
        private bool isHintSkillReady = true;

        private void Start()
        {
            ValidateReferences();
            InitializeComponents();
            SetupButtonListeners();
        }

        private void ValidateReferences()
        {
            // Score UI kontrolleri
            if (scoreText == null)
                Debug.LogError($"{gameObject.name}: Score Text reference is missing!");
            if (objectFillImage == null)
                Debug.LogError($"{gameObject.name}: Object Fill Image reference is missing!");

            // Buton kontrolleri
            if (resetButton == null)
                Debug.LogError($"{gameObject.name}: Reset Button reference is missing!");
            if (zoomSkillButton == null)
                Debug.LogError($"{gameObject.name}: Zoom Skill Button reference is missing!");
            if (hintSkillButton == null)
                Debug.LogError($"{gameObject.name}: Hint Skill Button reference is missing!");

            // Buton text'lerini otomatik bul
            if (zoomButtonText == null && zoomSkillButton != null)
            {
                zoomButtonText = zoomSkillButton.GetComponentInChildren<TMP_Text>();
                if (zoomButtonText == null)
                    Debug.LogError($"{gameObject.name}: Zoom Button Text component is missing!");
            }

            if (hintButtonText == null && hintSkillButton != null)
            {
                hintButtonText = hintSkillButton.GetComponentInChildren<TMP_Text>();
                if (hintButtonText == null)
                    Debug.LogError($"{gameObject.name}: Hint Button Text component is missing!");
            }
        }

        private void InitializeComponents()
        {
            _itemSpawner = FindObjectOfType<ItemSpawner>();
            if (_itemSpawner == null)
            {
                Debug.LogError("ItemSpawner bulunamadý!");
                return;
            }

            Initialize(_itemSpawner);
        }

        private void SetupButtonListeners()
        {
            if (resetButton != null)
            {
                resetButton.onClick.RemoveAllListeners();
                resetButton.onClick.AddListener(ResetGame);
            }

            if (zoomSkillButton != null)
            {
                zoomSkillButton.onClick.RemoveAllListeners();
                zoomSkillButton.onClick.AddListener(OnZoomSkillButtonClick);
            }

            if (hintSkillButton != null)
            {
                hintSkillButton.onClick.RemoveAllListeners();
                hintSkillButton.onClick.AddListener(OnHintSkillButtonClick);
            }
        }

        public void Initialize(ItemSpawner itemSpawner)
        {
            if (itemSpawner == null)
            {
                Debug.LogError("ItemSpawner null olamaz!");
                return;
            }

            _itemSpawner = itemSpawner;
            _score = 0;
            SetupUI();

            // Event'lere abone ol
            GameEvents.OnItemMatched += OnItemMatched;
            GameEvents.OnItemsSpawned += SetupUI;
        }

        private void OnDestroy()
        {
            // Event aboneliklerini temizle
            GameEvents.OnItemMatched -= OnItemMatched;
            GameEvents.OnItemsSpawned -= SetupUI;

            // Button listener'larý temizle
            if (resetButton != null)
                resetButton.onClick.RemoveListener(ResetGame);
            if (zoomSkillButton != null)
                zoomSkillButton.onClick.RemoveListener(OnZoomSkillButtonClick);
            if (hintSkillButton != null)
                hintSkillButton.onClick.RemoveListener(OnHintSkillButtonClick);
        }

        private void SetupUI()
        {
            // Score text'ini ayarla
            if (scoreText != null)
                scoreText.text = string.Format(_scoreTextFormat, _score);

            // Fill image'ý sýfýrla
            if (objectFillImage != null)
                objectFillImage.fillAmount = 0;

            // Zoom butonunu ayarla
            if (zoomSkillButton != null)
            {
                zoomSkillButton.interactable = true;
                if (zoomButtonText != null)
                    zoomButtonText.text = "Zoom";
            }

            // Hint butonunu ayarla
            if (hintSkillButton != null)
            {
                hintSkillButton.interactable = true;
                if (hintButtonText != null)
                    hintButtonText.text = "Hint";
            }
        }

        private void OnItemMatched(ItemData data)
        {
            _score += data.itemScore;
            UpdateUI();
            StartCoroutine(ScorePopupAnimation());
        }

        private IEnumerator ScorePopupAnimation()
        {
            if (scoreText == null) yield break;

            Vector3 originalScale = scoreText.transform.localScale;

            yield return new WaitForSeconds(scorePopupDuration);

            scoreText.transform.localScale = originalScale;
        }

        private void UpdateUI()
        {
            // Score text'ini güncelle
            if (scoreText != null)
                scoreText.text = string.Format(_scoreTextFormat, _score);

            // Progress bar'ý güncelle
            if (_itemSpawner != null && objectFillImage != null)
            {
                float fillAmount = 1 - (_itemSpawner.CurrentItemCount / (float)_itemSpawner.SpawnedItemCount);
                StartCoroutine(SmoothFillAnimation(fillAmount));
            }
        }

        private IEnumerator SmoothFillAnimation(float targetFill)
        {
            if (objectFillImage == null) yield break;

            float startFill = objectFillImage.fillAmount;
            float elapsed = 0;

            while (elapsed < fillAnimationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fillAnimationDuration;
                objectFillImage.fillAmount = Mathf.Lerp(startFill, targetFill, t);
                yield return null;
            }

            objectFillImage.fillAmount = targetFill;
        }

        #region Skill Button Handlers

        public void OnZoomSkillButtonClick()
        {
            if (!isZoomSkillReady || zoomSkillButton == null) return;

            zoomSkillButton.interactable = false;
            GameEvents.InvokeZoomSkillUsed();

            StartCoroutine(ZoomSkillCooldown());
        }

        private IEnumerator ZoomSkillCooldown()
        {
            if (zoomSkillButton == null || zoomButtonText == null) yield break;

            isZoomSkillReady = false;

            // Aktif süre
            float remainingTime = 3f;
            while (remainingTime > 0)
            {
                zoomButtonText.text = $"Zoom\nActive ({Mathf.CeilToInt(remainingTime)}s)";
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            // Cooldown süresi
            remainingTime = zoomCooldown;
            while (remainingTime > 0)
            {
                zoomButtonText.text = $"Zoom\n({Mathf.CeilToInt(remainingTime)}s)";
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            isZoomSkillReady = true;
            zoomSkillButton.interactable = true;
            zoomButtonText.text = "Zoom";
        }

        public void OnHintSkillButtonClick()
        {
            if (!isHintSkillReady || hintSkillButton == null) return;

            hintSkillButton.interactable = false;
            GameEvents.InvokeHintSkillUsed();

            StartCoroutine(HintSkillCooldown());
        }

        private IEnumerator HintSkillCooldown()
        {
            if (hintSkillButton == null || hintButtonText == null) yield break;

            isHintSkillReady = false;

            // Aktif süre
            float remainingTime = 4f;
            while (remainingTime > 0)
            {
                hintButtonText.text = $"Hint\nActive ({Mathf.CeilToInt(remainingTime)}s)";
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            // Cooldown süresi
            remainingTime = hintCooldown;
            while (remainingTime > 0)
            {
                hintButtonText.text = $"Hint\n({Mathf.CeilToInt(remainingTime)}s)";
                remainingTime -= Time.deltaTime;
                yield return null;
            }

            isHintSkillReady = true;
            hintSkillButton.interactable = true;
            hintButtonText.text = "Hint";
        }

        #endregion

        #region Game Reset

        public void ResetGame()
        {
            StopAllCoroutines();

            // Score'u sýfýrla
            _score = 0;
            if (scoreText != null)
                scoreText.text = string.Format(_scoreTextFormat, _score);

            // Fill bar'ý sýfýrla
            if (objectFillImage != null)
                objectFillImage.fillAmount = 0;

            // Yetenekleri resetle
            ResetSkills();

            // ItemSpawner'ý resetle
            if (_itemSpawner != null)
            {
                ResetItems();
            }
            else
            {
                Debug.LogError("ItemSpawner is null!");
            }
        }

        private void ResetSkills()
        {
            // Zoom yeteneðini resetle
            isZoomSkillReady = true;
            if (zoomSkillButton != null)
            {
                zoomSkillButton.interactable = true;
                if (zoomButtonText != null)
                    zoomButtonText.text = "Zoom";
            }

            // Hint yeteneðini resetle
            isHintSkillReady = true;
            if (hintSkillButton != null)
            {
                hintSkillButton.interactable = true;
                if (hintButtonText != null)
                    hintButtonText.text = "Hint";
            }
        }

        private void ResetItems()
        {
            // Mevcut itemlarý temizle
            Transform[] currentObjects = new Transform[_itemSpawner.spawnedObjects.Count];
            _itemSpawner.spawnedObjects.CopyTo(currentObjects);

            foreach (var item in currentObjects)
            {
                if (item != null)
                {
                    StartCoroutine(DestroyWithAnimation(item.gameObject));
                }
            }

            _itemSpawner.spawnedObjects.Clear();

            // Yeni itemlarý spawn et
            StartCoroutine(SpawnWithDelay());
        }

        private IEnumerator DestroyWithAnimation(GameObject obj)
        {
            if (obj == null) yield break;

            // Küçülme animasyonu
            Vector3 startScale = obj.transform.localScale;
            float elapsed = 0;
            float duration = 0.2f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                obj.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
                yield return null;
            }

            Destroy(obj);
        }

        private IEnumerator SpawnWithDelay()
        {
            yield return new WaitForSeconds(0.3f); // Yok etme animasyonunun bitmesini bekle
            if (_itemSpawner != null)
                _itemSpawner.SpawnObjects();
        }

        #endregion
    }
}
