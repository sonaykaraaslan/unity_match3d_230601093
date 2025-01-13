using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Match
{
    /// <summary>
    /// Item davranýþlarýný kontrol eden ana sýnýf
    /// </summary>
    public class Item : MonoBehaviour
    {
        [Header("Core Settings")]
        public int matchID = -1;
        public ItemData itemData;

        [Header("Physics References")]
        public Rigidbody selfRigidbody;

        [Header("State Variables")]
        public bool isDragged = false;
        public bool isPlaced = false;

        // Özel ayarlar
        private readonly float zoomMultiplier = 1.5f;
        private readonly float bounceHeight = 0.3f;
        private readonly float bounceSpeed = 3f;
        private readonly float fallLimit = -5f;

        // Component referanslarý
        private List<Collider> _colliders = new List<Collider>();
        private Vector3 originalScale;
        private Vector3 originalPosition;
        private bool isAnimating = false;

        private void Awake()
        {
            // Component referanslarýný al
            InitializeComponents();

            // Event'lere abone ol
            SubscribeToEvents();

            // Ýlk deðerleri kaydet
            SaveInitialValues();
        }

        private void InitializeComponents()
        {
            selfRigidbody = GetComponent<Rigidbody>();
            _colliders.AddRange(GetComponentsInChildren<Collider>());
        }

        private void SubscribeToEvents()
        {
            GameEvents.OnZoomSkillUsed += OnZoomActivated;
            GameEvents.OnHintSkillUsed += OnHintActivated;
        }

        private void SaveInitialValues()
        {
            originalScale = transform.localScale;
            originalPosition = transform.position;
        }

        private void OnDestroy()
        {
            // Event aboneliklerini temizle
            GameEvents.OnZoomSkillUsed -= OnZoomActivated;
            GameEvents.OnHintSkillUsed -= OnHintActivated;
        }

        /// <summary>
        /// Item'ýn eþleþme kontrolü
        /// </summary>
        public bool IsMatching(Item otherItem)
        {
            return this != otherItem && matchID == otherItem.matchID;
        }

        /// <summary>
        /// Collider'larý aktif/pasif yapar
        /// </summary>
        public void SetCollidersActive(bool isActive)
        {
            foreach (var col in _colliders)
            {
                if (col != null)
                    col.enabled = isActive;
            }
        }

        private void FixedUpdate()
        {
            // Düþme limitini kontrol et
            CheckFallLimit();
        }

        private void CheckFallLimit()
        {
            if (transform.position.y < fallLimit)
            {
                ReCenterObject();
            }
        }

        /// <summary>
        /// Item'ý baþlangýç pozisyonuna döndürür
        /// </summary>
        private void ReCenterObject()
        {
            if (selfRigidbody != null)
            {
                selfRigidbody.velocity = Vector3.zero;
                selfRigidbody.angularVelocity = Vector3.zero;
                selfRigidbody.isKinematic = true;
            }

            transform.position = Vector3.up * 2f;

            if (selfRigidbody != null)
            {
                selfRigidbody.isKinematic = false;
            }
        }

        #region Skill Effects

        /// <summary>
        /// Zoom yeteneði aktifleþtiðinde çaðrýlýr
        /// </summary>
        private void OnZoomActivated()
        {
            if (!isAnimating)
                StartCoroutine(ZoomCoroutine());
        }

        private IEnumerator ZoomCoroutine()
        {
            isAnimating = true;

            // Büyüt
            transform.localScale = originalScale * zoomMultiplier;

            yield return new WaitForSeconds(3f);

            // Normal boyuta dön
            if (gameObject.activeSelf)
                transform.localScale = originalScale;

            isAnimating = false;
        }

        /// <summary>
        /// Ýpucu yeteneði aktifleþtiðinde çaðrýlýr
        /// </summary>
        private void OnHintActivated()
        {
            var spawner = FindObjectOfType<ItemSpawner>();
            if (spawner == null) return;

            var (item1, item2) = spawner.GetRandomMatchingPair();
            if (item1 == null || item2 == null) return;

            // Eþleþen itemlarý zýplat
            if (!item1.isAnimating)
                StartCoroutine(HintBounceAnimation(item1));
            if (!item2.isAnimating)
                StartCoroutine(HintBounceAnimation(item2));
        }

        private IEnumerator HintBounceAnimation(Item item)
        {
            if (item == null) yield break;

            item.isAnimating = true;
            Vector3 startPos = item.transform.position;
            float duration = 4f;
            float elapsed = 0f;

            while (elapsed < duration && item != null && item.gameObject.activeSelf)
            {
                elapsed += Time.deltaTime;
                float verticalOffset = Mathf.Sin(elapsed * bounceSpeed) * bounceHeight;
                item.transform.position = startPos + Vector3.up * verticalOffset;
                yield return null;
            }

            if (item != null && item.gameObject.activeSelf)
            {
                item.transform.position = startPos;
                item.isAnimating = false;
            }
        }

        #endregion

        #region Optional Debug Methods

        private void OnValidate()
        {
            if (matchID < -1)
                matchID = -1;
        }

        #endregion
    }
}