using System;

namespace Match
{
    /// <summary>
    /// Oyun içi event sistemi
    /// </summary>
    public static class GameEvents
    {
        // Item eþleþmesi gerçekleþtiðinde tetiklenir
        public static event Action<ItemData> OnItemMatched;

        // Yeni itemlar spawn edildiðinde tetiklenir
        public static event Action OnItemsSpawned;

        // Zoom yeteneði kullanýldýðýnda tetiklenir
        public static event Action OnZoomSkillUsed;

        // Ýpucu yeteneði kullanýldýðýnda tetiklenir
        public static event Action OnHintSkillUsed;

        // Rüzgar yeteneði kullanýldýðýnda tetiklenir
        public static event Action OnWindSkillUsed;

        /// <summary>
        /// Item eþleþme event'ini tetikler
        /// </summary>
        /// <param name="itemData">Eþleþen item verisi</param>
        public static void InvokeItemMatched(ItemData itemData)
        {
            OnItemMatched?.Invoke(itemData);
        }

        /// <summary>
        /// Item spawn event'ini tetikler
        /// </summary>
        public static void InvokeItemsSpawned()
        {
            OnItemsSpawned?.Invoke();
        }

        /// <summary>
        /// Zoom yeteneði event'ini tetikler
        /// </summary>
        public static void InvokeZoomSkillUsed()
        {
            OnZoomSkillUsed?.Invoke();
        }

        /// <summary>
        /// Ýpucu yeteneði event'ini tetikler
        /// </summary>
        public static void InvokeHintSkillUsed()
        {
            OnHintSkillUsed?.Invoke();
        }

        /// <summary>
        /// Rüzgar yeteneði event'ini tetikler
        /// </summary>
        public static void InvokeWindSkillUsed()
        {
            OnWindSkillUsed?.Invoke();
        }
    }
}