using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BEKStudio{
    public class JoystickControl : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler{
        public RectTransform background;
        public RectTransform handle;
        public float moveRange = 100f;
        private Vector2 inputVector = Vector2.zero;

        public Vector2 InputVector => inputVector;

        public void OnPointerDown(PointerEventData eventData){
            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData){
            Vector2 position = Vector2.zero;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background, eventData.position, eventData.pressEventCamera, out position);

            position = position / (background.sizeDelta / 2);
            inputVector = (position.magnitude > 1.0f) ? position.normalized : position;
            
            handle.anchoredPosition = (inputVector * moveRange);
        }

        public void OnPointerUp(PointerEventData eventData){
            inputVector = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
        }
    }
}