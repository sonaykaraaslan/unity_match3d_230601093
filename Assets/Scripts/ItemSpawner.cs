using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Match
{
    public class ItemSpawner : MonoBehaviour
    {
        [Header("Core Settings")]
        public ItemRepository itemRepository;
        [Range(1, 8)] public int spawnCount = 8;

        [Header("Spawn Area")]
        public Vector3 spawnArea = new Vector3(5, 1, 5);
        [Range(1, 10)] public float spawnDistance = 1.7f;

        [Header("Animation Settings")]
        [SerializeField] private float bounceHeight = 0.3f;
        [SerializeField] private float bounceSpeed = 3f;
        [SerializeField] private float bounceAnimDuration = 4f;

        // Spawn edilen objeler listesi
        public List<Transform> spawnedObjects = new List<Transform>();

        // Properties
        public int CurrentItemCount => spawnedObjects.Count(x => x != null && x.gameObject.activeSelf);
        public int SpawnedItemCount => spawnCount * 2;

        private void Update()
        {
            // Tüm objeler eþleþtiðinde yeni level baþlat
            if (spawnedObjects.Count > 0 && spawnedObjects.All(x => x == null || !x.gameObject.activeSelf))
            {
                Debug.Log("All objects are matched - Starting new level");
                SpawnObjects();
            }
        }

        /// <summary>
        /// Aktif olan tüm itemlarý döndürür
        /// </summary>
        public List<Item> GetItems()
        {
            return spawnedObjects
                .Where(x => x != null && x.gameObject.activeSelf)
                .Select(x => x.GetComponent<Item>())
                .ToList();
        }

        /// <summary>
        /// Hint sistemi için rastgele bir çift eþleþen item döndürür
        /// </summary>
        public (Item, Item) GetRandomMatchingPair()
        {
            var activeItems = GetItems();

            // Eþleþen item gruplarýný bul
            var matchGroups = activeItems
                .GroupBy(x => x.matchID)
                .Where(g => g.Count() >= 2) // En az 2 aktif item olan gruplarý al
                .ToList();

            if (matchGroups.Count == 0)
                return (null, null);

            // Rastgele bir grup (tür) seç
            var randomGroup = matchGroups[UnityEngine.Random.Range(0, matchGroups.Count)];
            var pair = randomGroup.Take(2).ToList();

            return (pair[0], pair[1]);
        }

        /// <summary>
        /// Hint yeteneði için zýplama animasyonunu yönetir
        /// </summary>
        public void HandleHintAnimation()
        {
            var (item1, item2) = GetRandomMatchingPair();
            if (item1 == null || item2 == null) return;

            StartCoroutine(BounceAnimation(item1.transform));
            StartCoroutine(BounceAnimation(item2.transform));
        }

        private IEnumerator BounceAnimation(Transform itemTransform)
        {
            if (itemTransform == null) yield break;

            Vector3 startPos = itemTransform.position;
            float elapsed = 0f;

            while (elapsed < bounceAnimDuration && itemTransform != null && itemTransform.gameObject.activeSelf)
            {
                elapsed += Time.deltaTime;
                float verticalOffset = Mathf.Sin(elapsed * bounceSpeed) * bounceHeight;
                itemTransform.position = startPos + Vector3.up * verticalOffset;
                yield return null;
            }

            if (itemTransform != null && itemTransform.gameObject.activeSelf)
            {
                itemTransform.position = startPos;
            }
        }

        [ContextMenu("Spawn Objects")]
        public void SpawnObjects()
        {
            const int pairCount = 2;

            // Mevcut objeleri temizle
            ClearSpawnedObjects();

            // Repository'den rastgele itemlarý al
            var itemData = itemRepository.GetRandomItems(spawnCount);
            if (itemData.Count == 0)
            {
                Debug.LogError("No items found in the repository!");
                return;
            }

            // Her item için çift oluþtur
            for (int i = 0; i < spawnCount; i++)
            {
                for (int j = 0; j < pairCount; j++)
                {
                    SpawnSingleItem(itemData[i], i);
                }
            }

            // Event'i tetikle
            GameEvents.InvokeItemsSpawned();
        }

        /// <summary>
        /// Tek bir item spawn eder
        /// </summary>
        private void SpawnSingleItem(ItemData itemData, int matchId)
        {
            Vector3 spawnPosition = GetValidSpawnPosition();
            var instance = Instantiate(itemData.itemPrefab, spawnPosition, Quaternion.identity);

            var item = instance.GetComponent<Item>();
            if (item != null)
            {
                item.itemData = itemData;
                item.matchID = matchId;
            }

            spawnedObjects.Add(instance.transform);
        }

        /// <summary>
        /// Spawn edilmiþ tüm objeleri temizler
        /// </summary>
        private void ClearSpawnedObjects()
        {
            foreach (var obj in spawnedObjects)
            {
                if (obj != null)
                {
                    Destroy(obj.gameObject);
                }
            }
            spawnedObjects.Clear();
        }

        /// <summary>
        /// Geçerli bir spawn pozisyonu bulur
        /// </summary>
        private Vector3 GetValidSpawnPosition()
        {
            Vector3 spawnPosition;
            const int maxTries = 100;
            int currentTryCount = 0;
            bool isValid = false;

            do
            {
                spawnPosition = transform.position + GetRandomPos();
                currentTryCount++;

                // Pozisyonun diðer objelerle mesafesini kontrol et
                isValid = !spawnedObjects.Any(x =>
                    x != null &&
                    Vector3.Distance(x.position, spawnPosition) < spawnDistance
                );

            } while (!isValid && currentTryCount <= maxTries);

            if (currentTryCount > maxTries)
            {
                Debug.LogWarning("Max spawn position tries reached - Using last position");
            }

            return spawnPosition;
        }

        /// <summary>
        /// Spawn alaný içinde rastgele bir pozisyon döndürür
        /// </summary>
        private Vector3 GetRandomPos()
        {
            return new Vector3(
                UnityEngine.Random.Range(-spawnArea.x * 0.5f, spawnArea.x * 0.5f),
                UnityEngine.Random.Range(-spawnArea.y * 0.5f, spawnArea.y * 0.5f),
                UnityEngine.Random.Range(-spawnArea.z * 0.5f, spawnArea.z * 0.5f)
            );
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, spawnArea);
        }
    }
}