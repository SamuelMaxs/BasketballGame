using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TMP_Text scoreText; // Drag TextMeshPro UI Text ke sini melalui Inspector
    private int score = 0;

    void Start()
    {
        UpdateScoreText();
        Ball.OnBallScored += IncrementScore;
    }

    void OnDestroy()
    {
        Ball.OnBallScored -= IncrementScore;
    }

    void IncrementScore()
    {
        score += 1;
        UpdateScoreText();

        // Cek dan simpan skor tertinggi
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
        }

        // Simpan skor terakhir
        PlayerPrefs.SetInt("LastScore", score);
    }

    void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score: " + score.ToString();
        }
    }
}
