using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace BEKStudio {
    public class Food : MonoBehaviour{
        public Transform target;
        public PlayerController playerController;
        public AiController aiController;
        public TextMeshPro numberText;
        public float moveSpeed = 5f;
        public float rotationSpeed = 360f;
        public int currentNumber = 2;
        public bool waitingForMerge;
        public int mergeCount;
        public Vector3 offset;

        void OnEnable(){
            if (numberText == null){
                numberText = transform.GetChild(0).gameObject.GetComponent<TextMeshPro>();
            }
        }
        
        void Start(){
            mergeCount = Mathf.RoundToInt(Mathf.Log(currentNumber, 2)) - 1;
            
            UpdateNumberText();
            Vector3 newScale = Vector3.one;
            newScale.x += (mergeCount / 10f);
            newScale.y += (mergeCount / 10f);
            newScale.z += (mergeCount / 10f);
            transform.localScale = newScale;
        }

        void FixedUpdate(){
            FollowParent();
        }

        void FollowParent(){
            if (target == null) {
                if (waitingForMerge){
                    Destroy(gameObject);
                }
                
                return;
            }

            if (!waitingForMerge){
                offset = target.forward * (target.localScale.x / 2);
                if (playerController != null) {
                    Debug.DrawLine(transform.position, target.position - offset, Color.green);
                    moveSpeed = playerController.speed;
                } else if (aiController != null) {
                    moveSpeed = aiController.speed;
                }
            }
            
            offset.y = 0;
            transform.position = Vector3.Lerp(transform.position, target.position - offset, Time.fixedDeltaTime * moveSpeed);
            
            Vector3 direction = target.position - transform.position;
            float yRotation = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, yRotation, 0), Time.fixedDeltaTime * rotationSpeed);

            if (waitingForMerge && (transform.position - target.position).magnitude < 0.2f){
                if (playerController != null){
                    if (target == playerController.transform){
                        playerController.IncreaseNumber();
                    } else {
                        target.GetComponent<Food>().IncreaseNumber();
                    }
                } else if (aiController != null){
                    if (target == aiController.transform){
                        aiController.IncreaseNumber();
                    } else {
                        target.GetComponent<Food>().IncreaseNumber();
                    }
                } else {
                    target.GetComponent<Food>().IncreaseNumber();
                }
                Destroy(gameObject);
            }
        }

        void UpdateNumberText(){
            if(numberText == null) return;
            numberText.text = GameController.Instance.FormatPowerOfTwo(currentNumber);
            GetComponent<MeshRenderer>().material.color = GameController.Instance.GetColor(mergeCount);
        }

        void IncreaseNumber(){
            if (currentNumber == 1073741824){ //Max 1B score
                return;
            }
            
            currentNumber *= 2;
            mergeCount = Mathf.RoundToInt(Mathf.Log(currentNumber, 2) - 1);
            
            UpdateNumberText();
            Vector3 newScale = Vector3.one;
            newScale.x += (mergeCount / 10f);
            newScale.y += (mergeCount / 10f);
            newScale.z += (mergeCount / 10f);
            transform.localScale = newScale;

            if (playerController != null){
                playerController.SortFoods();
            } else if (aiController != null){
                aiController.SortFoods();
            }
        }

        public void MoveToTargetForMerge(){
            waitingForMerge = true;
            offset = Vector3.zero;
            GetComponent<BoxCollider>().enabled = false;
            moveSpeed *= 10;
            rotationSpeed *= 10;
        }

        public void Reset() {
            moveSpeed = 5;
            target = null;
            offset = Vector3.zero;
            
            if (playerController != null) {
                playerController.RemoveFood(this);
            }
            
            if (aiController != null) {
                aiController.RemoveFood(this);
            }

            playerController = null;
            aiController = null;
            
            if (waitingForMerge) {
                Destroy(gameObject);
            }
        }

        void OnDestroy() {
            if (LeanTween.isTweening(gameObject)){
                LeanTween.cancel(gameObject);
            }
        }
    }
}