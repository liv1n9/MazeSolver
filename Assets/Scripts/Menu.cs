using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Lớp Model Menu
/// </summary>
public class Menu : MonoBehaviour {
    /// <summary>
    /// Key DIFFICULT trong PlayerPrefs
    /// </summary>
    public const string DIFFICULT = "DIFFICULT";

    /// <summary>
    /// Value của DIFFICULT tương ứng với các độ khó
    /// </summary>
    public const int EASY = 0;
    public const int MEDIUM = 1;
    public const int HARD = 2;
    public const int VERY_HARD = 3;

    /// <summary>
    /// Các Object truyền từ MenuScene
    /// </summary>
    private Button playButton, exitButton;
    private Toggle easyToggle, mediumToggle, hardToggle, veryHardToggle;

    /// <summary>
    /// Khởi tạo model
    /// </summary>
    /// <param name="play">Button Play</param>
    /// <param name="exit">Button Exit</param>
    /// <param name="easy">Toggle Easy</param>
    /// <param name="medium">Toggle Medium</param>
    /// <param name="hard">Toggle Hard</param>
    /// <param name="veryHard">Toggle Very Hard</param>
    public void Init(Button play, Button exit, Toggle easy, Toggle medium, Toggle hard, Toggle veryHard) {
        playButton = play;
        exitButton = exit;
        easyToggle = easy;
        mediumToggle = medium;
        hardToggle = hard;
        veryHardToggle = veryHard;

        InitToggle();
        InitButton();
    }

    /// <summary>
    /// Khởi tạo cho các Toggle
    /// </summary>
    private void InitToggle() {
        /// Nếu như đã có key DIFFICULT thì bật luôn cho Toggle tương ứng
        if (PlayerPrefs.HasKey(DIFFICULT)) {
            int difficult = PlayerPrefs.GetInt(DIFFICULT);
            switch (difficult) {
                case EASY:
                    easyToggle.isOn = true;
                    break;
                case MEDIUM:
                    mediumToggle.isOn = true;
                    break;
                case HARD:
                    hardToggle.isOn = true;
                    break;
                case VERY_HARD:
                    veryHardToggle.isOn = true;
                    break;
            }
        } else {
            // Nếu chưa có key DIFFICULT thì bật mặc định Toggle Easy
            easyToggle.isOn = true;
        }
    }

    /// <summary>
    /// Khởi tạo onClick cho các Button
    /// </summary>
    private void InitButton() {
        playButton.onClick.AddListener(StartGameScene);
        exitButton.onClick.AddListener(ExitGame);
    }

    /// <summary>
    /// Khởi động GameScene
    /// </summary>
    private void StartGameScene() {
        /// Gán giá trị cho key DIFFICULT tương ứng với Toggle được bật
        if (easyToggle.isOn) {
            PlayerPrefs.SetInt(DIFFICULT, EASY);
        } else if (mediumToggle.isOn) {
            PlayerPrefs.SetInt(DIFFICULT, MEDIUM);
        } else if (hardToggle.isOn) {
            PlayerPrefs.SetInt(DIFFICULT, HARD);
        } else {
            PlayerPrefs.SetInt(DIFFICULT, VERY_HARD);
        }
        /// Xoá seed đi nếu có
        PlayerPrefs.DeleteKey(Maze.SEED);
        SceneManager.LoadScene("GameScene");
    }

    /// <summary>
    /// Rời bỏ cuộc chơi
    /// </summary>
    private void ExitGame() {
        Application.Quit();
    }
}
