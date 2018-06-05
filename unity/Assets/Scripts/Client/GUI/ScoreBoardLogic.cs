using Game;
using UnityEngine;
using UnityEngine.UI;

namespace Client.GUI {
    /// <summary>
    /// Logic delegate for ScoreBoard
    /// </summary>
    public class ScoreBoardLogic {
        const string WinMessage = "YOU WIN!";
        const string LoseMessage = "YOU LOSE!";

        Text yourScore;
        Text opponentScore;
        Text centerMessage;

        public ScoreBoardLogic(Text yourScore, Text opponentScore, Text centerMessage) {
            this.yourScore = yourScore;
            this.opponentScore = opponentScore;
            this.centerMessage = centerMessage;
        }

        public void OnScore(int score, bool isPlayerLocal) {
            Debug.LogFormat("[ScoreBoard] I gots a score! {0}, is local player: {1}", score, isPlayerLocal);
            if (isPlayerLocal) {
                yourScore.text = string.Format("You: {0}/{1}", score, PlayerScore.WinningScore);
                if (score >= PlayerScore.WinningScore) {
                    centerMessage.text = WinMessage;
                }
            } else {
                opponentScore.text = string.Format("Them: {0}/{1}", score, PlayerScore.WinningScore);
                if (score >= PlayerScore.WinningScore) {
                    centerMessage.text = LoseMessage;
                }
            }
        }
    }
}
