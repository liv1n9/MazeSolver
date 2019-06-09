using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp Controller LoseController
/// </summary>
public class LoseController : MonoBehaviour {
    /// <summary>
    /// Lớp Model Lose
    /// </summary>
    private Lose lose;

    /// <summary>
    /// Các Object truyền vào từ LoseScene
    /// </summary>
    public Button restartButton, newGameButton, mainMenuButton;

    // Start is called before the first frame update
    void Start() {
        lose = gameObject.AddComponent<Lose>();
        lose.Init(restartButton, newGameButton, mainMenuButton);
    }
}
