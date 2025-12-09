using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BEKStudio {
    public class FoodSpawner : MonoBehaviour{
        public static FoodSpawner Instance;
        public GameObject foodPrefab;
        public int minX;
        public int maxX;
        public int minZ;
        public int maxZ;
        public List<GameObject> spawnedFoods;


        void Awake(){
            if (Instance == null){
                Instance = this;
            }
        }
        
        void OnEnable() {
            GameEvents.OnFoodEaten += OnFoodEaten;
        }

        void OnDisable(){
            GameEvents.OnFoodEaten -= OnFoodEaten;
        }
        
        void OnFoodEaten(Food food){
            if(!spawnedFoods.Contains(food.gameObject)) return;
            
            spawnedFoods.Remove(food.gameObject);

            if (spawnedFoods.Count < GameController.Instance.bekso.gameSettings.maxFoodCount){
                SpawnFood(true);
            }
        }
        
        void Start(){
            spawnedFoods = new List<GameObject>();
        }

        public void Spawn() {
            for (int i = 0; i < GameController.Instance.bekso.gameSettings.maxFoodCount; i++){
                SpawnFood();
            }
        }

        void SpawnFood(bool scaleAnimation = false){
            Vector3 position;
            bool validPosition;
            int maxAttempts = 10;
            int attempts = 0;

            do {
                float x = Random.Range(minX, maxX);
                float z = Random.Range(minZ, maxZ);
                position = new Vector3(x, -0.5f, z);
            
                validPosition = true;
                foreach (GameObject f in spawnedFoods) {
                    if (Vector3.Distance(position, f.transform.position) < 3f) {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            } while (!validPosition && attempts < maxAttempts);

            GameObject food = Instantiate(foodPrefab, position, Quaternion.Euler(0, Random.Range(0, 360), 0));
            spawnedFoods.Add(food);
            
            if (scaleAnimation){
                food.transform.localScale = Vector3.zero;
                LeanTween.scale(food, Vector3.one, 0.5f).setEase(LeanTweenType.easeOutBounce);
            }
        }

        public GameObject GetClosestFood(Vector3 position){
            if (spawnedFoods.Count == 0) return null;
        
            var nearbyFoods = spawnedFoods.Where(f => Vector3.Distance(f.transform.position, position) <= 20f).ToList();
            if (nearbyFoods.Count > 0) {
                return nearbyFoods[Random.Range(0, nearbyFoods.Count)];
            }
        
            return spawnedFoods[Random.Range(0, spawnedFoods.Count)];
        }
    }
}
