using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

namespace Match.View
{
    public class HintSkillButton : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text buttonText;
        [SerializeField] private Image cooldownFill;
        [SerializeField] private GameObject electricEffectPrefab; // CFXR Electrified 3 prefab referansý
        private ItemSpawner itemSpawner;

        [Header("Hint Position Settings")]
        [SerializeField] private Vector3 targetPosition = new Vector3(0f, 1f, 0f);
        [SerializeField] private float itemSpacing = 0.5f;
        [SerializeField] private float hoverHeight = 0.8f;
        [SerializeField] private float bounceSpeed = 3f;

        [Header("Visual Settings")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color activeColor = Color.green;
        [SerializeField] private Color cooldownColor = Color.gray;

        [Header("Timing Settings")]
        [SerializeField] private float activeDuration = 1f;
        [SerializeField] private float cooldownDuration = 10f;

        private bool isReady = true;
        private bool isActive = false;
        private Vector3 originalScale;
        private Transform[] activeHintItems;
        private GameObject[] activeEffects;

        private void Awake()
        {
            InitializeComponents();
            SetupReferences();
        }

        private void InitializeComponents()
        {
            if (button == null)
                button = GetComponent<Button>();

            if (buttonText == null && button != null)
                buttonText = GetComponentInChildren<TMP_Text>();

            if (cooldownFill == null && button != null)
                cooldownFill = transform.Find("CooldownFill")?.GetComponent<Image>();

            itemSpawner = FindObjectOfType<ItemSpawner>();

            ValidateComponents();
        }

        private void ValidateComponents()
        {
            if (button == null)
                Debug.LogError($"{gameObject.name}: Button component is missing!");
            if (buttonText == null)
                Debug.LogError($"{gameObject.name}: Button Text component is missing!");
            if (cooldownFill == null)
                Debug.LogError($"{gameObject.name}: Cooldown Fill Image component is missing!");
            if (itemSpawner == null)
                Debug.LogError($"{gameObject.name}: ItemSpawner not found in scene!");
            if (electricEffectPrefab == null)
                Debug.LogError($"{gameObject.name}: Electric Effect Prefab is missing!");
        }

        private void SetupReferences()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClick);
            }

            originalScale = transform.localScale;
            ResetButton();
        }

        private void OnButtonClick()
        {
            if (!isReady || isActive || itemSpawner == null) return;
            ActivateHintSkill();
        }

        private void ActivateHintSkill()
        {
            isReady = false;
            isActive = true;

            var (item1, item2) = itemSpawner.GetRandomMatchingPair();
            if (item1 != null && item2 != null)
            {
                activeHintItems = new Transform[] { item1.transform, item2.transform };
                activeEffects = new GameObject[2];

                TeleportAndAnimateItems(item1, item2);
                CreateElectricEffects();
            }

            UpdateButtonState(isActive: true);
            StartCoroutine(HandleCooldown());
        }

        private void CreateElectricEffects()
        {
            if (activeHintItems == null || electricEffectPrefab == null) return;

            for (int i = 0; i < activeHintItems.Length; i++)
            {
                if (activeHintItems[i] != null)
                {
                    // Elektrik efektini oluþtur
                    Vector3 spawnPosition = activeHintItems[i].position;
                    GameObject effectObj = Instantiate(electricEffectPrefab, spawnPosition, Quaternion.identity);
                    effectObj.transform.SetParent(activeHintItems[i]);
                    effectObj.transform.localPosition = Vector3.zero;
                    activeEffects[i] = effectObj;
                }
            }
        }

        private void CleanupEffects()
        {
            if (activeEffects != null)
            {
                foreach (var effect in activeEffects)
                {
                    if (effect != null)
                    {
                        Destroy(effect);
                    }
                }
                activeEffects = null;
            }
        }

        private void TeleportAndAnimateItems(Item item1, Item item2)
        {
            Vector3 leftPosition = targetPosition + new Vector3(-itemSpacing, 0, 0);
            Vector3 rightPosition = targetPosition + new Vector3(itemSpacing, 0, 0);

            SetItemPhysics(item1, false);
            SetItemPhysics(item2, false);

            item1.transform.position = leftPosition;
            item2.transform.position = rightPosition;

            StartCoroutine(BounceAnimation(item1.transform, leftPosition));
            StartCoroutine(BounceAnimation(item2.transform, rightPosition));
        }

        private void SetItemPhysics(Item item, bool enabled)
        {
            if (item.selfRigidbody != null)
            {
                item.selfRigidbody.isKinematic = !enabled;
                item.selfRigidbody.useGravity = enabled;
            }
        }

        private IEnumerator BounceAnimation(Transform itemTransform, Vector3 basePosition)
        {
            float elapsed = 0f;

            while (elapsed < activeDuration && itemTransform != null)
            {
                elapsed += Time.deltaTime;
                float verticalOffset = Mathf.Sin(elapsed * bounceSpeed) * hoverHeight;
                itemTransform.position = basePosition + Vector3.up * verticalOffset;
                yield return null;
            }

            if (itemTransform != null)
            {
                SetItemPhysics(itemTransform.GetComponent<Item>(), true);
            }
        }

        private void UpdateButtonState(bool isActive)
        {
            if (button != null)
                button.interactable = !isActive;

            Color targetColor = isActive ? activeColor : (isReady ? normalColor : cooldownColor);
            if (buttonText != null)
                buttonText.color = targetColor;
        }

        private void UpdateButtonText(string state, float remainingTime = 0)
        {
            if (buttonText == null) return;

            if (remainingTime > 0)
                buttonText.text = $"{state}\n({Mathf.CeilToInt(remainingTime)}s)";
            else
                buttonText.text = state;
        }

        private void UpdateCooldownFill(float fillAmount)
        {
            if (cooldownFill != null)
                cooldownFill.fillAmount = fillAmount;
        }

        private IEnumerator HandleCooldown()
        {
            float elapsed = 0f;
            while (elapsed < activeDuration)
            {
                elapsed += Time.deltaTime;
                UpdateButtonText("ACTIVE", activeDuration - elapsed);
                UpdateCooldownFill(1 - (elapsed / activeDuration));
                yield return null;
            }

            CleanupEffects();
            isActive = false;
            UpdateButtonState(isActive: false);

            elapsed = 0f;
            while (elapsed < cooldownDuration)
            {
                elapsed += Time.deltaTime;
                UpdateButtonText("COOLDOWN", cooldownDuration - elapsed);
                UpdateCooldownFill(elapsed / cooldownDuration);
                yield return null;
            }

            ResetButton();
            ResetHintItems();
        }

        private void ResetButton()
        {
            isReady = true;
            isActive = false;
            UpdateButtonState(isActive: false);
            UpdateButtonText("HINT");
            UpdateCooldownFill(0f);
        }

        private void ResetHintItems()
        {
            if (activeHintItems != null)
            {
                foreach (var item in activeHintItems)
                {
                    if (item != null)
                    {
                        var itemComponent = item.GetComponent<Item>();
                        if (itemComponent != null)
                        {
                            SetItemPhysics(itemComponent, true);
                        }
                    }
                }
                activeHintItems = null;
            }
        }

        private void OnDestroy()
        {
            if (button != null)
                button.onClick.RemoveListener(OnButtonClick);
            CleanupEffects();
            StopAllCoroutines();
        }

        public void EnableSkill()
        {
            gameObject.SetActive(true);
            ResetButton();
        }

        public void DisableSkill()
        {
            StopAllCoroutines();
            ResetButton();
            ResetHintItems();
            CleanupEffects();
            gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (button == null)
                button = GetComponent<Button>();
            if (buttonText == null && button != null)
                buttonText = GetComponentInChildren<TMP_Text>();
            if (cooldownFill == null && button != null)
                cooldownFill = transform.Find("CooldownFill")?.GetComponent<Image>();
        }
#endif
    }
}