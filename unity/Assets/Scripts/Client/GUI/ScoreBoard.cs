using System;
using UnityEngine;
using UnityEngine.UI;

namespace Client.GUI {
    /// <summary>
    /// Components to take information from players
    /// and display it on the HUD score
    /// </summary>
    public class ScoreBoard : MonoBehaviour {
        ScoreBoardLogic logic;

        [SerializeField] [Tooltip("Your score field")]
        Text yourScore;

        [SerializeField] [Tooltip("Your opponent's score field")]
        Text opponentScore;

        [SerializeField] [Tooltip("The center message box")]
        Text centerMessage;

        // --- Messages ---

        void Start() { logic = new ScoreBoardLogic(yourScore, opponentScore, centerMessage); }

        void OnValidate() {
            if (yourScore == null) {
                throw new Exception("Your score is null. It should not be");
            }

            if (opponentScore == null) {
                throw new Exception("Opponent Score is null. It should not be");
            }

            if (centerMessage == null) {
                throw new Exception("Center Message is null. It should not be");
            }
        }

        // --- Functions ---

        /// <summary>
        /// Delegate to ScoreBoardLogic.OnScore
        /// </summary>
        /// <param name="score"></param>
        /// <param name="isPlayerLocal"></param>
        public void OnScore(int score, bool isPlayerLocal) { logic.OnScore(score, isPlayerLocal); }
    }
}
