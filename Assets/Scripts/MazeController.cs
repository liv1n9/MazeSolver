using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MazeController : MonoBehaviour {
    /// <summary>
    /// Lớp Model Maze
    /// </summary>
    private Maze maze;

    /// <summary>
    /// Các Object truyền từ GameScene
    /// </summary>
    public Camera myCamera;
    public GameObject cell;
    public GameObject pointerStart;
    public GameObject pointerFinish;
    public GameObject pointerFault;
    public Button mainMenuButton;

    void Start() {
        /// Khởi tạo mê cung
        maze = gameObject.AddComponent<Maze>();
        maze.Init(myCamera, cell, pointerStart, pointerFinish, pointerFault, mainMenuButton);
    }

    void Update() {
        /// Lấy ra trạng thái của mê cung
        /// Nếu state == 1 (đã đến đích) thì chuyển sang GameScene mới
        /// Nếu state == -1 (phạm luật) thì chuyển sang LoseScene
        int state = maze.UpdateKey();
        if (state == 1) {
            StartCoroutine(NewGameScene());
        } else if (state == -1) {
            StartCoroutine(NewLoseScene());
        }
    }

    IEnumerator NewGameScene() {
        yield return new WaitForSeconds(2);
        PlayerPrefs.DeleteKey(Maze.SEED);
        SceneManager.LoadScene("GameScene");
    }

    IEnumerator NewLoseScene() {
        yield return new WaitForSeconds(1);
        SceneManager.LoadScene("LoseScene");
    }
}
