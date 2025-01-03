using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_Text highScoreText; // Referensi ke TextMeshPro untuk skor tertinggi
    public TMP_Text lastScoreText; // Referensi ke TextMeshPro untuk skor terakhir

    void Start()
    {
        // Tampilkan skor tertinggi
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (highScoreText != null)
        {
            highScoreText.text = "High Score: " + highScore.ToString();
        }

        // Tampilkan skor terakhir
        int lastScore = PlayerPrefs.GetInt("LastScore", 0);
        if (lastScoreText != null)
        {
            lastScoreText.text = "Last Score: " + lastScore.ToString();
        }
    }

    public void StartGame()
    {
        Debug.Log("StartGame button clicked!"); // Tambahkan ini untuk memverifikasi klik
        SceneManager.LoadScene("Gameplay");
    }

    public void QuitGame()
    {
        // Keluar dari aplikasi
        Application.Quit();
        Debug.Log("Game Quit"); // Untuk testing di editor
    }
}
