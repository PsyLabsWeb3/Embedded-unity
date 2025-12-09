using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BEKStudio {
    public static class PlayerList {
        [System.Serializable]
        public class Player {
            public int order;
            public string username;
            public int score;
        }
    
        public static List<Player> players = new List<Player>();

        public static void AddPlayer(string username, int score) {
            Player p = new Player();
            p.username = username;
            p.score = score;
        
            players.Add(p);

            UpdateList();
        }

        public static void RemovePlayer(string username) {
            Player p = players.Find(x => x.username == username);
            if (p != null){
                players.Remove(p);
            }
        
            UpdateList();
        }

        public static void UpdateScore(string username, int score) {
            Player p = players.Find(x => x.username == username);
            if (p != null){
                int index = players.IndexOf(p);
                players[index].score = score;
            }
        
            UpdateList();
        }

        static void UpdateList(){
            players = players.OrderByDescending(p => p.score).ToList();
            for (int i = 0; i < players.Count; i++) {
                players[i].order = i + 1;
            }
            GameController.Instance.UpdateScoreboard();
        }
    }
}