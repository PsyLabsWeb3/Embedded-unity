using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;

namespace BEKStudio {
    public class PlayerController : MonoBehaviour{
        public float speed = 5f;
        public List<Food> foods;
        public TextMeshPro numberText;
        public int currentNumber = 2;
        public float boostTime = 2;
        public bool boostActive;
        public bool boostRefillWait;
        public int mergeCount;
        public int killCount;
        Rigidbody rb;
        
        void Start(){
            rb = GetComponent<Rigidbody>();
            
            foods = new List<Food>();
            
            UpdateNumberText();
            
            PlayerList.AddPlayer(GameController.Instance.username, currentNumber);
        }

        void Update(){
            if(GameController.Instance.gameStatus != GameController.GameStatus.PLAYING) return;
            
            BoostSpeed();
            RotateTowardsMouse();
        }

        void FixedUpdate() {
            rb.linearVelocity = Vector3.zero;
            if(GameController.Instance.gameStatus != GameController.GameStatus.PLAYING) return;
            
            MoveForward();
        }

        void MoveForward(){
            rb.MovePosition(rb.position + transform.forward * speed * Time.deltaTime);
        }

        void RotateTowardsMouse(){
            Vector3 mousePosition = Input.mousePosition;
            float angle = 0;
            
            #if UNITY_ANDROID || UNITY_IPHONE
            angle = Mathf.Atan2(GameController.Instance.joystickControl.InputVector.x / 1, GameController.Instance.joystickControl.InputVector.y / 1) * Mathf.Rad2Deg;
            #else
            angle = Mathf.Atan2(mousePosition.x - Screen.width / 2, mousePosition.y - Screen.height / 2) * Mathf.Rad2Deg;
            #endif
            
            transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        void BoostSpeed(){
            if (Input.GetKeyDown(KeyCode.Space)) {
                boostActive = true;
            }

            if (Input.GetKeyUp(KeyCode.Space)) {
                boostActive = false;
                boostRefillWait = true;
                speed = 5;
            }
            
            if (boostActive && !boostRefillWait){
                speed = 10;
                boostTime -= 1 * Time.deltaTime;

                if (boostTime <= 0) {
                    speed = 5;
                    boostRefillWait = true;
                    boostActive = false;
                }
            }
            
            if (boostRefillWait){
                boostTime += 1 * Time.deltaTime;
                if (boostTime >= 2){
                    boostRefillWait = false;
                }
            }

            GameController.Instance.gameBoostImg.fillAmount = boostTime / 2;
        }

        void OnCollisionEnter(Collision col){
            if (col.gameObject.CompareTag("Food")){
                if(currentNumber < col.gameObject.GetComponent<Food>().currentNumber) return;
                
                CollectFood(col.gameObject.GetComponent<Food>());
            }

            if (col.gameObject.CompareTag("Bot")) {
                int enemyNumber = col.gameObject.GetComponent<AiController>().currentNumber;
                if (currentNumber > enemyNumber) {
                    killCount++;
                    
                    GameEvents.OnPlayerEaten?.Invoke(GameController.Instance.username, col.gameObject);
                    col.gameObject.GetComponent<AiController>().SetItFree();
                    CollectFood(col.gameObject.GetComponent<Food>());
                } else if(currentNumber < enemyNumber){
                    GameEvents.OnPlayerEaten?.Invoke(col.gameObject.name, gameObject);
                    GameController.Instance.GameOver();
                    Die();
                }
            }
        }
        
        void OnTriggerEnter(Collider col){
            if (col.gameObject.CompareTag("Food")) {
                if(foods.Contains(col.GetComponent<Food>())) return;
                if(col.GetComponent<Food>().currentNumber > currentNumber) return;
                
                col.GetComponent<Food>().Reset();
                CollectFood(col.gameObject.GetComponent<Food>());
            }
        }

        void Die() {
            PlayerList.RemovePlayer(GameController.Instance.username);
            GetComponent<BoxCollider>().enabled = false;
            rb.isKinematic = true;
            for (int i = 0; i < foods.Count; i++){
                foods[i].Reset();
            }
            
            gameObject.SetActive(false);
        }

        void CollectFood(Food food){
            food.playerController = this;
            foods.Add(food);
            
            if (foods.Count == 1){
                food.target = transform;
            } else {
                food.target = foods[foods.Count - 2].transform;
            }

            GameController.Instance.PlayFoodSound();
            food.GetComponent<BoxCollider>().isTrigger = true;
            GameEvents.OnFoodEaten?.Invoke(food);
            SortFoods();
        }

        public void SortFoods(){
            foods = foods.OrderByDescending(f => f.currentNumber).ToList();
            
            RearrangeTargets();
            MergeFoods();
        }

        void RearrangeTargets(){
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
                    Food fTarget = foods[i].GetComponent<Food>().target.GetComponent<Food>();
                    if (fTarget.currentNumber == f.currentNumber){
                        foods.Remove(f);
                        f.MoveToTargetForMerge();
                        RearrangeTargets();
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
        
        void UpdateNumberText(){
            numberText.text = GameController.Instance.FormatPowerOfTwo(currentNumber);
            GetComponent<MeshRenderer>().material.color = GameController.Instance.GetColor(mergeCount);
        }
        
        public void IncreaseNumber(){
            if (currentNumber == 1073741824){
                return;
            }
            
            currentNumber *= 2;
            mergeCount = Mathf.RoundToInt(Mathf.Log(currentNumber, 2)) - 1;
            PlayerList.UpdateScore(GameController.Instance.username, currentNumber);
            CameraController.Instance.IncreaseDistance();
            
            UpdateNumberText();
            Vector3 newScale = Vector3.one;
            newScale.x += (mergeCount / 10f);
            newScale.y += (mergeCount / 10f);
            newScale.z += (mergeCount / 10f);
            transform.localScale = newScale;
            
            SortFoods();
        }

        public void RemoveFood(Food f) {
            if (foods.Contains(f)) {
                foods.Remove(f);
                SortFoods();
            }
        }
        
        public void LevelUpForStart() {
            mergeCount = Mathf.RoundToInt(Mathf.Log(currentNumber, 2)) - 1;
            
            UpdateNumberText();
            Vector3 newScale = Vector3.one;
            newScale.x += (mergeCount / 10f);
            newScale.y += (mergeCount / 10f);
            newScale.z += (mergeCount / 10f);
            transform.localScale = newScale;
            PlayerList.UpdateScore(GameController.Instance.username, currentNumber);
            CameraController.Instance.IncreaseDistance();
        }
    }
}