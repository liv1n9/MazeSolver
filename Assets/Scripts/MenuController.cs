using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lớp Controller MenuController
/// </summary>
public class MenuController : MonoBehaviour {
    /// <summary>
    /// Lớp Model Menu
    /// </summary>
    private Menu menu;

    /// <summary>
    /// Các Object truyền vào từ MenuScene
    /// </summary>
    public Button playButton, exitButton;
    public Toggle easyToggle, mediumToggle, hardToggle, veryHardToggle;

    // Start is called before the first frame update
    void Start() {
        menu = gameObject.AddComponent<Menu>();
        menu.Init(playButton, exitButton, easyToggle, mediumToggle, hardToggle, veryHardToggle);
    }
}
