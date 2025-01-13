using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Match;
namespace Match.View
{
    public class ZoomSkillButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text buttonText;

        private float cooldownDuration = 8f;  // Bekleme süresi
        private float activeDuration = 3f;    // Aktif kalma süresi
        private bool isReady = true;

        private void Start()
        {
            ValidateReferences();
            SetupButton();
        }
        private void ValidateReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
                if (button == null)
                {
                    Debug.LogError($"{gameObject.name}: Button component is missing!");
                    return;
                }
            }
            if (buttonText == null)
            {
                buttonText = GetComponentInChildren<TMP_Text>();
                if (buttonText == null)
                {
                    Debug.LogError($"{gameObject.name}: TMP_Text component is missing!");
                }
            }
        }
        private void SetupButton()
        {
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnButtonClick);

                // Baþlangýç metni
                if (buttonText != null)
                {
                    buttonText.text = "Zoom";
                }
            }
        }
        private void OnButtonClick()
        {
            if (!isReady || button == null) return;
            button.interactable = false;
            GameEvents.InvokeZoomSkillUsed();
            StartCoroutine(HandleZoomSkill());
        }
        private IEnumerator HandleZoomSkill()
        {
            if (button == null || buttonText == null) yield break;

            isReady = false;
            // Aktif süre
            float remainingTime = activeDuration;
            while (remainingTime > 0)
            {
                buttonText.text = $"Zoom\nActive ({Mathf.CeilToInt(remainingTime)}s)";
                remainingTime -= Time.deltaTime;
                yield return null;
            }
            // Cooldown süresi
            remainingTime = cooldownDuration;
            while (remainingTime > 0)
            {
                buttonText.text = $"Zoom\n({Mathf.CeilToInt(remainingTime)}s)";
                remainingTime -= Time.deltaTime;
                yield return null;
            }
            // Yetenek hazýr
            isReady = true;
            button.interactable = true;
            buttonText.text = "Zoom";
        }
        public void ResetSkill()
        {
            StopAllCoroutines();
            isReady = true;

            if (button != null)
            {
                button.interactable = true;
            }

            if (buttonText != null)
            {
                buttonText.text = "Zoom";
            }
        }
        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnButtonClick);
            }
        }
    }
}