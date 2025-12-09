using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using Random = UnityEngine.Random;

namespace BEKStudio {
    public class AiController : MonoBehaviour{
        public float speed = 5f;
        public List<Food> foods;
        public TextMeshPro numberText;
        public int currentNumber = 2;
        public float boostTime = 2;
        public bool boostActive;
        public int mergeCount;
        Rigidbody rb;
        public GameObject targetFood;
        public GameObject targetEnemy;
        public List<GameObject> enemies;
        public float catchTime;
        public bool waitForNewCatch;
        public GameObject enemyDetector;
        bool boostRefillWait;
        float randTimeForBoost;


        void OnEnable() {
            GameEvents.OnFoodEaten += OnFoodEaten;
            GameEvents.OnPlayerEaten += OnPlayerEaten;
        }

        void OnDisable(){
            GameEvents.OnFoodEaten -= OnFoodEaten;
            GameEvents.OnPlayerEaten -= OnPlayerEaten;
        }

        void OnFoodEaten(Food food){
            if(targetFood == null) return;

            if (targetFood == food.gameObject) {
                targetFood = null;
            }
        }
        
        void OnPlayerEaten(string killer, GameObject player){
            if (enemies.Contains(player)) {
                enemies.Remove(player);
            }

            if (targetEnemy != null) {
                if (targetEnemy == player) {
                    targetEnemy = null;
                }
            }
        }
        
        void Start(){
            rb = GetComponent<Rigidbody>();
            
            foods = new List<Food>();
            enemies = new List<GameObject>();
            
            UpdateNumberText();

            targetFood = FoodSpawner.Instance.GetClosestFood(rb.position);
            randTimeForBoost = Random.Range(3, 10);
        }

        void Update(){
            RotateTowardsToFood();
            RotateTowardsToEnemy();
            BoostSpeed();
            
            if (targetFood == null && targetEnemy == null){
                targetFood = FoodSpawner.Instance.GetClosestFood(rb.position);
            }

            if (!waitForNewCatch && targetEnemy == null){
                if (enemies.Count == 0) {
                    targetEnemy = null;
                    return;
                }
                
                if (enemies[0].CompareTag("Bot")){
                    if (currentNumber <= enemies[0].GetComponent<AiController>().currentNumber){
                        targetEnemy = null;
                        return;
                    }
                } else if (enemies[0].CompareTag("Player")){
                    if (currentNumber <= enemies[0].GetComponent<PlayerController>().currentNumber){
                        targetEnemy = null;
                        return;
                    }
                }
                
                targetEnemy = enemies[0];
                targetFood = null;
                catchTime = Random.Range(2, 4);
            }
        }

        void FixedUpdate(){
            rb.linearVelocity = Vector3.zero;
            MoveForward();
        }

        void MoveForward(){
            rb.MovePosition(rb.position + transform.forward * speed * Time.fixedDeltaTime);
        }

        void RotateTowardsToFood(){
            if(targetFood == null) return;
            
            Vector3 direction = targetFood.transform.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero){
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5);
            }
        }
        
        void RotateTowardsToEnemy(){
            if(targetEnemy == null) return;
            
            Vector3 direction = targetEnemy.transform.position - transform.position;
            direction.y = 0;

            if (direction != Vector3.zero){
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5);
            }

            if (!waitForNewCatch){
                if (catchTime > 0){
                    catchTime -= 1 * Time.deltaTime;
                } else {
                    waitForNewCatch = true;
                    targetEnemy = null;
                    StartCoroutine(WaitCatchDelay());
                }
            }
        }

        IEnumerator WaitCatchDelay() {
            targetFood = FoodSpawner.Instance.GetClosestFood(rb.position);
            yield return new WaitForSeconds(Random.Range(2, 4));
            waitForNewCatch = false;
        }

        void BoostSpeed(){
            if (!boostActive && !boostRefillWait){
                if (randTimeForBoost > 0) {
                    randTimeForBoost -= 1 * Time.deltaTime;
                } else {
                    randTimeForBoost = 0;
                    boostActive = true;
                }
            }
            
            if (boostActive && !boostRefillWait){
                speed = 10;
                boostTime -= 1 * Time.deltaTime;

                if (boostTime <= 0) {
                    speed = 5;
                    boostRefillWait = true;
                    boostActive = false;
                    randTimeForBoost = Random.Range(3, 7);
                }
            }
            
            if (boostRefillWait){
                boostTime += 1 * Time.deltaTime;
                if (boostTime >= 2){
                    boostRefillWait = false;
                }
            }
        }

        void OnCollisionEnter(Collision col){
            if (col.gameObject.CompareTag("Food")){
                if(currentNumber < col.gameObject.GetComponent<Food>().currentNumber) return;
                
                if (targetFood == col.gameObject){
                    targetFood = null;
                }
                
                CollectFood(col.gameObject.GetComponent<Food>());
            }
            
            if (col.gameObject.CompareTag("Bot")) {
                int enemyNumber = col.gameObject.GetComponent<AiController>().currentNumber;
                if (currentNumber > enemyNumber) {
                    GameEvents.OnPlayerEaten?.Invoke(gameObject.name, col.gameObject);
                    col.gameObject.GetComponent<AiController>().SetItFree();
                    CollectFood(col.gameObject.GetComponent<Food>());
                }
            }
        }
        
        void OnTriggerEnter(Collider col){
            if (col.gameObject.CompareTag("Food")) {
                if(foods.Contains(col.GetComponent<Food>())) return;
                if(currentNumber < col.GetComponent<Food>().currentNumber) return;
                
                col.GetComponent<Food>().Reset();
                CollectFood(col.gameObject.GetComponent<Food>());
                
                if (targetFood == col.gameObject){
                    targetFood = null;
                }
            }

            if (col.gameObject.CompareTag("Bot") || col.gameObject.CompareTag("Player")){
                if (!enemies.Contains(col.gameObject)){
                    enemies.Add(col.gameObject);
                }
            }
        }

        void OnTriggerExit(Collider col){
            if (col.gameObject.CompareTag("Bot") || col.gameObject.CompareTag("Player")){
                if (targetEnemy == col.gameObject) {
                    targetEnemy = null;
                    waitForNewCatch = true;
                }
                
                if (enemies.Contains(col.gameObject)){
                    enemies.Remove(col.gameObject);
                }
            }
        }

        void CollectFood(Food food){
            food.GetComponent<BoxCollider>().isTrigger = true;
            food.aiController = this;
            foods.Add(food);
            
            if (foods.Count == 1){
                food.target = transform;
            } else if (foods.Count > 1){
                food.target = foods[foods.Count - 1].transform;
            }
            
            GameEvents.OnFoodEaten?.Invoke(food);
            SortFoods();
        }

        public void SortFoods(){
            foods = foods.Where(f => f != null).OrderByDescending(f => f.currentNumber).ToList();
            
            RearrangeTargets();
            MergeFoods();
        }

        void RearrangeTargets(){
            foods = foods.Where(f => f != null).OrderByDescending(f => f.currentNumber).ToList();
            
            for (int i = 0; i < foods.Count; i++){
                if (i == 0){
                    foods[i].target = transform;
                } else {
                    foods[i].target = foods[i - 1].transform;
                }
            }
        }

        void MergeFoods(){
            for (int i = foods.Count - 1; i >= 0; i--){
                Food f = foods[i];
                if (f.target != transform){
                    if (foods[i].GetComponent<Food>().target != null) {
                        Food fTarget = foods[i].GetComponent<Food>().target.GetComponent<Food>();
                        if (fTarget.currentNumber == f.currentNumber){
                            foods.Remove(f);
                            f.MoveToTargetForMerge();
                            RearrangeTargets();
                        }
                    }
                } else {
                    if (currentNumber == f.currentNumber){
                        foods.Remove(f);
                        f.MoveToTargetForMerge();
                        RearrangeTargets();
                    }
                }
                
            }
        }
        
        public void UpdateNumberText(){
            numberText.text = GameController.Instance.FormatPowerOfTwo(currentNumber);
            GetComponent<MeshRenderer>().material.color = GameController.Instance.GetColor(mergeCount);
        }
        
        public void IncreaseNumber(){
            if (currentNumber == 1073741824){
                return;
            }
            
            currentNumber *= 2;
            mergeCount = Mathf.RoundToInt(Mathf.Log(currentNumber, 2) - 1);
            PlayerList.UpdateScore(transform.name, currentNumber);
            
            UpdateNumberText();
            Vector3 newScale = Vector3.one;
            newScale.x += (mergeCount / 10f);
            newScale.y += (mergeCount / 10f);
            newScale.z += (mergeCount / 10f);
            transform.localScale = newScale;
            
            SortFoods();
        }

        public void LevelUpForStart() {
            mergeCount = Mathf.RoundToInt(Mathf.Log(currentNumber, 2)) - 1;
            
            UpdateNumberText();
            Vector3 newScale = Vector3.one;
            newScale.x += (mergeCount / 10f);
            newScale.y += (mergeCount / 10f);
            newScale.z += (mergeCount / 10f);
            transform.localScale = newScale;
        }
        
        public void RemoveFood(Food f) {
            if (foods.Contains(f)) {
                foods.Remove(f);

                if (f.target != null){
                    SortFoods();
                }
            }
        }

        public void SetItFree() {
            GetComponent<BoxCollider>().isTrigger = true;
            Destroy(enemyDetector);

            gameObject.AddComponent<Food>();
            GetComponent<Food>().currentNumber = currentNumber;
            transform.tag = "Food";
            gameObject.layer = LayerMask.NameToLayer("Default");
            
            for (int i = 0; i < foods.Count; i++){
                foods[i].Reset();
            }
            
            Destroy(GetComponent<AiController>());
        }
    }
}