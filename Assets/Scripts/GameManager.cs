using Match.View;
using UnityEngine;

namespace Match
{
    public class GameManager : MonoBehaviour
    {
        //singleton instance
        public static GameManager Instance;
        public ItemSpawner itemSpawner;
        public UIController uiController;
        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            //initialize UI 
            uiController.Initialize(itemSpawner);
            //initialize scene objects
            itemSpawner.SpawnObjects();

        }
    }
}