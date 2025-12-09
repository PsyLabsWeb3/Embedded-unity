using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEKStudio {
    public class CameraController : MonoBehaviour {
        public static CameraController Instance;
        public Transform target;
        public float speed;
        public float distance;
        float startDistance;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
        }

        void Start() {
            startDistance = distance;
        }
    
        void FixedUpdate(){
            if(target == null) return;
        
            Vector3 backOffset = transform.rotation * Vector3.forward * -distance;
            Vector3 desiredPosition = target.position + backOffset;

            transform.position = Vector3.Lerp(transform.position, desiredPosition, speed * Time.fixedDeltaTime);
        }

        public void IncreaseDistance() {
            distance = startDistance + (GameController.Instance.playerController.mergeCount * 0.15f);
        }
    
    }
}
