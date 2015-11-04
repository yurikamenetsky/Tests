using UnityEngine;
using UnityEngine.Networking;

using System.Collections;

namespace GUI
{
    public class GameMenu : MonoBehaviour
    {
        [SerializeField]
        UnityEngine.UI.Text scoreText;

        [SerializeField]
        GameObject gameOverPanel;

        public void SetScores(int count)
        {
            scoreText.text = string.Format("Scores: {0}", count);
        }

        public void ShowGameOver()
        {
            gameOverPanel.SetActive(true);
        }

        public void OnGameOver()
        {
            if (NetworkServer.active)
            {
                NetworkManager.singleton.StopHost();
                NetworkServer.Reset();
            }
            if (NetworkClient.active)
            {
                NetworkManager.singleton.StopClient();
            }
        }
    }
}
