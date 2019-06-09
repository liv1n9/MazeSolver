using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Lớp Model Lose
/// </summary>
public class Lose : MonoBehaviour {
    private Button restartButton, newGameButton, mainMenuButton;

    /// <summary>
    /// Khởi tạo model
    /// </summary>
    /// <param name="restartButton">Button Restart</param>
    /// <param name="newGameButton">Button New game</param>
    public void Init(Button restartButton, Button newGameButton, Button mainMenuButton) {
        this.restartButton = restartButton;
        this.newGameButton = newGameButton;
        this.mainMenuButton = mainMenuButton;
        InitButton();
    }

    /// <summary>
    /// Khởi tạo onclick cho các Button
    /// </summary>
    private void InitButton() {
        restartButton.onClick.AddListener(RestartGame);
        newGameButton.onClick.AddListener(NewGame);
        mainMenuButton.onClick.AddListener(MainMenu);
    }

    private void RestartGame() {
        SceneManager.LoadScene("GameScene");
    }
    /// <summary>
    /// Nếu chọn Button New game thì không giữ lại SEED trong PlayerPrefs
    /// </summary>
    private void NewGame() {
        PlayerPrefs.DeleteKey(Maze.SEED);
        RestartGame();
    }

    private void MainMenu() {
        SceneManager.LoadScene("MenuScene");
    }
}
