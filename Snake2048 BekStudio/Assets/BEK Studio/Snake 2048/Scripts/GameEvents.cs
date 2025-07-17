using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BEKStudio {
    public static class GameEvents {
        public static Action<Food> OnFoodEaten;
        public static Action<string, GameObject> OnPlayerEaten;
    }
}