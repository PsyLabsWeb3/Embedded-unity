using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BEKStudio {
    public class AiSpawner : MonoBehaviour {
        public static AiSpawner Instance;
        public GameObject aiPrefab;
        public int minX;
        public int maxX;
        public int minZ;
        public int maxZ;
        public List<AiController> spawnedAi;

        private List<int> numberDistribution;

        void Awake() {
            if (Instance == null) {
                Instance = this;
            }
        }

        void OnEnable() {
            GameEvents.OnPlayerEaten += OnPlayerEaten;
            GameEvents.OnFoodEaten += OnFoodEaten;
        }

        void OnDisable() {
            GameEvents.OnPlayerEaten -= OnPlayerEaten;
            GameEvents.OnFoodEaten -= OnFoodEaten;
        }

        void Start() {
            spawnedAi = new List<AiController>();
        }

        public void Spawn() {
            numberDistribution = GenerateNumberDistribution(GameController.Instance.playerController.currentNumber);

            for (int i = 0; i < GameController.Instance.bekso.gameSettings.maxAiCount; i++) {
                SpawnAi();
            }
        }

        void OnPlayerEaten(string killer, GameObject player) {
            GameController.Instance.ShowDeathMessage(killer, player.name);
            if (player.CompareTag("Bot")) {
                AiController ai = player.GetComponent<AiController>();
                if (spawnedAi.Contains(ai)) {
                    spawnedAi.Remove(ai);
                }
                
                PlayerList.RemovePlayer(player.name);
                
                numberDistribution = GenerateNumberDistribution(GameController.Instance.playerController.currentNumber);
                SpawnAi();
            }
        }
        
        void OnFoodEaten(Food food){
            SortAiByNumber();
        }

        void SpawnAi() {
            if (spawnedAi.Count >= GameController.Instance.bekso.gameSettings.maxAiCount) return;

            Vector3 position;
            bool validPosition;
            int maxAttempts = 10;
            int attempts = 0;

            do {
                float x = Random.Range(minX, maxX);
                float z = Random.Range(minZ, maxZ);
                position = new Vector3(x, -0.5f, z);

                validPosition = true;
                foreach (AiController ai in spawnedAi) {
                    float distToAi = Vector3.Distance(position, ai.transform.position);
                    float distToPlayer = Vector3.Distance(position, GameController.Instance.playerController.transform.position);
                    if (distToAi < 4f || distToPlayer < 4f) {
                        validPosition = false;
                        break;
                    }
                }
                attempts++;
            } while (!validPosition && attempts < maxAttempts);

            GameObject aiObject = Instantiate(aiPrefab, position, Quaternion.identity);
            AiController aiController = aiObject.GetComponent<AiController>();

            if (aiController != null) {
                int number = aiController.currentNumber;
                aiController.currentNumber = GetNextNumber(number);
                aiController.name = GetRandomName();

                aiController.LevelUpForStart();
                PlayerList.AddPlayer(aiController.name, aiController.currentNumber);
            }

            spawnedAi.Add(aiController);
            SortAiByNumber();
        }

        public string GetRandomName() {
            string name = GameController.Instance.bekso.gameSettings.botNames[Random.Range(0, GameController.Instance.bekso.gameSettings.botNames.Length)];
            AiController aiController = spawnedAi.FirstOrDefault(ai => ai.name == name);

            while (aiController != null) {
                name = GameController.Instance.bekso.gameSettings.botNames[Random.Range(0, GameController.Instance.bekso.gameSettings.botNames.Length)];
                aiController = spawnedAi.FirstOrDefault(ai => ai.name == name);
            }
            
            return name;
        }

        public void SortAiByNumber() {
            GameController.Instance.UpdateScoreboard();
        }

        List<int> GenerateNumberDistribution(int playerNumber) {
            List<int> numbers = new List<int>();
            int minValue = Mathf.Max(2, playerNumber / 8);
            int maxValue = playerNumber * 8;
            List<int> possibleNumbers = new List<int>();

            for (int num = minValue; num > 0 && num <= maxValue && num <= (int.MaxValue / 2); num *= 2) {
                possibleNumbers.Add(num);
            }

            int[] distributionPattern = { 6, 4, 3, 3, 2, 1 }; 
            int index = 0;
            int remaining = GameController.Instance.bekso.gameSettings.maxAiCount;

            while (remaining > 0 && index < possibleNumbers.Count) {
                int amount = distributionPattern[Mathf.Min(index, distributionPattern.Length - 1)];
                for (int i = 0; i < amount && remaining > 0; i++) {
                    numbers.Add(possibleNumbers[index]);
                    remaining--;
                }
                index++;
            }

            return numbers;
        }

        int GetNextNumber(int number) {
            if (numberDistribution.Count == 0) {
                numberDistribution = GenerateNumberDistribution(GameController.Instance.playerController.currentNumber);
            }

            if (numberDistribution.Count == 0) {
                return number;
            }
            int index = Random.Range(0, numberDistribution.Count);
            int selectedNumber = numberDistribution[index];
            numberDistribution.RemoveAt(index);
            return selectedNumber;
        }
    }
}