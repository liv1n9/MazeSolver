using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Lớp Model Maze
/// </summary>
public class Maze : MonoBehaviour {
    /// <summary>
    /// Điều hướng di chuyển:
    /// 0 - sang trái
    /// 1 - lên trên
    /// 2 - sang phải
    /// 3 - xuống dưới
    /// </summary>
    private static readonly int[] PX = { 0, -1, 0, 1 };
    private static readonly int[] PY = { -1, 0, 1, 0 };

    /// <summary>
    /// Offset để cân đối vị trí player vào giữa các ô mê cung 
    /// </summary>
    private static readonly Vector3 playerOffset = new Vector3(0.05f, 0, 0);

    /// <summary>
    /// Các màu đánh dấu cho các ô trên mê cung:
    /// RED - ô phạm luật
    /// WHITE - ô chưa đi tới
    /// GREEN - tất cả các ô trên đường đi sau khi đã đi đến điểm kết thúc
    /// YELLOW - tất cả các ô trên đường đi trong quá trình di chuyển
    /// </summary>
    private static readonly Color RED = new Color(1, 0, 0);
    private static readonly Color WHITE = new Color(1, 1, 1);
    private static readonly Color GREEN = new Color(0, 1, 0);
    private static readonly Color YELLOW = new Color(1, 1, 0);


    /// <summary>
    ///  Hắng số cho thuật toán Prim
    /// </summary>
    private const int OUTSIDE = 0;
    private const int FRONTIER = 1;
    private const int INSIDE = 2;


    /// <summary>
    /// matrixCell[x, y] tương ứng với GameObject Cell hàng x cột y của mê cung
    /// </summary>
    private GameObject[,] matrixCell;

    /// <summary>
    /// GameObject của người chơi
    /// </summary>
    private GameObject player;

    /// <summary>
    /// GameObject ở điểm kết thúc
    /// </summary>
    private GameObject destination;

    /// <summary>
    /// state[x, y] là trạng thái của ô mê cung (x, y):
    /// OUTSIDE - Chưa nằm trong cây khung
    /// FRONTIER - Chưa nằm trong cây khung và kề với một đỉnh trong cây khung
    /// INSIDE - Đã nằm trong cây khung
    /// </summary>
    private int[,] state;

    /// <summary>
    /// avail[x, y, direction] là từ ô (x, y) có đi được sang hướng direction (0, 1, 2, 3) không?
    /// true - có
    /// false - không
    /// </summary>
    private bool[,,] avail;

    /// <summary>
    /// (sx, sy) là điểm bắt đầu của người chơi
    /// (fx, fy) là điểm kết thúc của trò chơi
    /// </summary>
    private int sx, sy, fx, fy;

    /// <summary>
    /// Các Object truyền từ GameScene
    /// </summary>
    private Camera myCamera;
    private GameObject cell;
    private GameObject pointerStart;
    private GameObject pointerFinish;
    private GameObject pointerFault;
    private Button mainMenuButton;
    
    /// <summary>
    /// Mê cung có kích thước n x n
    /// </summary>
    private int n = 10;

    /// <summary>
    /// flag là trạng thái của mê cung:
    /// 0 - chưa tìm được đường đi đến điểm kết thúc
    /// -1 - phạm luật
    /// </summary>
    private int flag = 0;

    /// <summary>
    /// Key cho random seed trong PlayerPrefs
    /// </summary>
    public const string SEED = "SEED";

    /// <summary>
    /// Khởi tạo GameScene
    /// </summary>
    /// <param name="myCamera">Camera của Scene</param>
    /// <param name="cell">GameObject cho mỗi ô của mê cung</param>
    /// <param name="pointerStart">GameObject cho điểm bắt đầu (người chơi)</param>
    /// <param name="pointerFinish"> GameObject cho điểm kết thúc</param>
    /// <param name="pointerFault">GameObject khi bị phạm luật</param>
    /// <param name="mainMenu">Button trở lại menu</param>
    public void Init(Camera myCamera, GameObject cell, GameObject pointerStart, GameObject pointerFinish, GameObject pointerFault, Button mainMenu) {
        this.myCamera = myCamera;
        this.cell = cell;
        this.pointerStart = pointerStart;
        this.pointerFinish = pointerFinish;
        this.pointerFault = pointerFault;
        mainMenuButton = mainMenu;
        InitMaze();
        InitPlayer();
        InitDestination();
        mainMenuButton.onClick.AddListener(StartMenuScene);
    }

    /// <summary>
    /// Bắt đầu MenuScene
    /// </summary>
    private void StartMenuScene() {
        SceneManager.LoadScene("MenuScene");
    }

    /// <summary>
    /// Loại bỏ cạnh nối giữa Cell u và Cell v theo hướng dir
    /// </summary>
    /// <param name="u">Cell u</param>
    /// <param name="v">Cell v</param>
    /// <param name="dir">Hướng đi từ Cell u đến Cell v (0, 1, 2, 3)</param>
    private void RemoveEdge(GameObject u, GameObject v, int dir) {
        /// Object Ceil gồm 4 Object con là WallLeft, WallRight, WallUp, WallDown
        /// Theo thứ tự 0, 1, 2, 3
        switch (dir) {
            /// Hướng đi sang trái, xoá cạnh trái của u và cạnh phải của v
            case 0:
                Destroy(u.transform.GetChild(0).gameObject);
                Destroy(v.transform.GetChild(1).gameObject);
                break;
            /// Hướng đi lên trên, xoá cạnh trên của u và cạnh dưới của v
            case 1:
                Destroy(u.transform.GetChild(2).gameObject);
                Destroy(v.transform.GetChild(3).gameObject);
                break;
            /// Hướng đi sang phải, xoá cạnh phải của u và cạnh trái của v
            case 2:
                Destroy(u.transform.GetChild(1).gameObject);
                Destroy(v.transform.GetChild(0).gameObject);
                break;
            /// Hướng đi xuống dưới, xoá cạnh dưới của u và cạnh trên của v
            case 3:
                Destroy(u.transform.GetChild(3).gameObject);
                Destroy(v.transform.GetChild(2).gameObject);
                break;
        }
    }

    /// <summary>
    /// Lấy ra hướng đi từ ô (x, y) sang ô (u, v) (0, 1, 2, 3)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="u"></param>
    /// <param name="v"></param>
    /// <returns></returns>
    private int GetDir(int x, int y, int u, int v) {
        if (u - x == -1) return 1;
        if (u - x == 1) return 3;
        if (v - y == -1) return 0;
        return 2;
    }

    /// <summary>
    /// Kiểm tra ô (x, y) có nằm trong mê cung không
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private bool Inside(int x, int y) {
        return x >= 0 && x < n && y >= 0 && y < n;
    }

    /// <summary>
    /// Thuật toán xây mê cung Prim bắt đầu từ ô (x, y)
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    private void Prim(int x, int y) {
        state[x, y] = INSIDE;
        List<KeyValuePair<int, int>> queue = new List<KeyValuePair<int, int>>();
        for (int i = 0; i < 4; i++) {
            int u = x + PX[i];
            int v = y + PY[i];
            if (Inside(u, v)) {
                state[u, v] = FRONTIER;
                queue.Add(new KeyValuePair<int, int>(u, v));
            }
        }
        while (queue.Count > 0) {
            int j = Random.Range(0, queue.Count);
            x = queue[j].Key;
            y = queue[j].Value;
            state[x, y] = INSIDE;
            queue.RemoveAt(j);
            List<KeyValuePair<int, int>> adj = new List<KeyValuePair<int, int>>();
            for (int i = 0; i < 4; i++) {
                int u = x + PX[i];
                int v = y + PY[i];
                if (Inside(u, v)) {
                    if (state[u, v] == INSIDE) {
                        adj.Add(new KeyValuePair<int, int>(u, v));
                    } else if (state[u, v] == OUTSIDE) {
                        state[u, v] = FRONTIER;
                        queue.Add(new KeyValuePair<int, int>(u, v));
                    }
                }
            }
            if (adj.Count > 0) {
                int i = Random.Range(0, adj.Count);
                int u = adj[i].Key;
                int v = adj[i].Value;
                RemoveEdge(matrixCell[x, y], matrixCell[u, v], GetDir(x, y, u, v));
                avail[x, y, GetDir(x, y, u, v)] = true;
                avail[u, v, GetDir(u, v, x, y)] = true;
            }
        }
    }

    /// <summary>
    /// Thuật toán Dfs
    /// Sau khi hoàn thành Prim, state[x, y] có thể dùng để lưu độ sâu trong Dfs
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="depth"></param>
    private void Dfs(int x, int y, int depth) {
        state[x, y] = depth;
        for (int i = 0; i < 4; i++) {
            int u = x + PX[i];
            int v = y + PY[i];
            if (Inside(u, v) && state[u, v] == 0 && avail[x, y, i]) {
                Dfs(u, v, depth + 1);
            }
        }
    }

    /// <summary>
    /// Khởi tạo (sx, sy) và (fx, fy) là cặp điểm có khoảng cách xa nhất trong số các cặp điểm trên mê cung
    /// </summary>
    private void LongestPath() {
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                state[i, j] = 0;
            }
        }
        Dfs(Random.Range(0, n), Random.Range(0, n), 1);
        fx = 0;
        fy = 0;
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                if (state[i, j] > state[fx, fy]) {
                    fx = i;
                    fy = j;
                }
            }
        }
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                state[i, j] = 0;
            }
        }
        Dfs(fx, fy, 1);
        sx = 0;
        sy = 0;
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                if (state[i, j] > state[sx, sy]) {
                    sx = i;
                    sy = j;
                }
            }
        }
        Debug.Log(state[sx, sy]);
    }

    /// <summary>
    /// Khởi tạo n x n GameObject Cell cho mê cung
    /// </summary>
    private void InitMaze() {
        int seed;
        /// Nếu tồn tại Key SEED trong PlayerPrefs có nghĩa là GameScene được bắt đầu từ nút Restart
        /// Ngược lại gán seed là một số nguyên bất kỳ thuộc [0, 1e9)
        if (PlayerPrefs.HasKey(SEED)) {
            seed = PlayerPrefs.GetInt(SEED);
        } else {
            seed = Random.Range(0, 1000000000);
        }
        Random.InitState(seed);
        PlayerPrefs.SetInt(SEED, seed);

        /// Lấy ra độ khó được chọn từ MenuScene
        int difficult = PlayerPrefs.GetInt(Menu.DIFFICULT);
        switch (difficult) {
            case Menu.EASY:
                n = 10;
                break;
            case Menu.MEDIUM:
                n = 20;
                break;
            case Menu.HARD:
                n = 30;
                break;
            case Menu.VERY_HARD:
                n = 60;
                break;
        }

        /// Thiết lập orthographicSize cho camera (orthorgraphic là kiểu camera 2D)
        myCamera.orthographicSize = n / 1.5f;

        /// Khởi tạo các matrixCell[i, j]
        /// Do hệ trục toạ độ của mê cung là Ox từ trên xuống dưới, Oy từ trái sang phải, trong khi
        /// hệ trục toạ độ của Unity là Ox từ trái sang phải, Oy từ dưới lên trên, do đó matrixCell[i, j]
        /// sẽ tương ứng với Cell có toạ độ (j, n - i - 1) trong Unity
        matrixCell = new GameObject[n, n];
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                matrixCell[i, j] = Instantiate(cell, new Vector3(j, n - i - 1, 0), Quaternion.identity);
            }
        }
        /// Offset để tịnh tiến các ô trong mê cung sao cho toàn bộ mê cung vào
        /// chính giữa màn hình
        Vector3 offset;
        if (n % 2 == 0) {
            offset = new Vector3(-0.5f, 0.5f, -0.1f);
        } else {
            offset = new Vector3(0, 0, -0.1f);
        }
        offset -= matrixCell[(n - 1) / 2, (n - 1) / 2].transform.position;
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                matrixCell[i, j].transform.position += offset;
            }
        }

        /// Khởi tạo Prim
        state = new int[n, n];
        avail = new bool[n, n, 4];
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                state[i, j] = OUTSIDE;
                for (int k = 0; k < 4; k++) {
                    avail[i, j, k] = false;
                }
            }
        }
        Prim(Random.Range(0, n), Random.Range(0, n));
        LongestPath();
    }

    /// <summary>
    /// Đặt màu color cho ô matrixCell[x, y]
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="color">Màu muốn đặt</param>
    private void setCeilColor(int x, int y, Color color) {
        matrixCell[x, y].gameObject.GetComponent<Renderer>().material.color = color;
    }

    /// <summary>
    /// Khởi tạo player
    /// </summary>
    private void InitPlayer() {
        player = Instantiate(pointerStart, matrixCell[sx, sy].transform.position + playerOffset, Quaternion.identity);
        /// Sau khi khởi tạo mê cung, state[,] có thể sử dụng để đánh dấu đường đi
        /// state[i, j] = {OUTSIDE, INSIDE}
        /// OUTSIDE - chưa được đi qua (màu trắng)
        /// INSIDE - đã đi qua (màu vàng)
        for (int i = 0; i < n; i++) {
            for (int j = 0; j < n; j++) {
                state[i, j] = OUTSIDE;
            }
        }
        state[sx, sy] = INSIDE;
        setCeilColor(sx, sy, YELLOW);
    }

    /// <summary>
    /// Khởi tạo điểm kết thúc
    /// </summary>
    private void InitDestination() {
        destination = Instantiate(pointerFinish, matrixCell[fx, fy].transform.position + playerOffset, Quaternion.identity);
    }

    /// <summary>
    /// Cập nhật trạng thái mê cung mỗi lần Update() từ MazeController 
    /// </summary>
    /// <returns>
    /// -1 - Phạm luật
    /// 0 - Chưa đến đích
    /// 1 - Đã đến đích
    /// </returns>
    public int UpdateKey() {
        // Nếu như đã ở điểm kết thúc rồi thì return 1
        if (sx == fx && sy == fy) return 1;
        /// Nếu phạm luật thì return -1
        if (flag == -1) return -1;

        /// (nx, ny) là điểm di chuyển dự kiến
        int nx, ny;
        nx = sx;
        ny = sy;

        /// Chỉnh giá trị cho (nx, ny) tuỳ theo phím được ấn
        if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow)) {
            nx = sx - 1;
        } else if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)) {
            nx = sx + 1;
        } else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)) {
            ny = sy - 1;
        } else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) {
            ny = sy + 1;
        }
        /// Nếu như (nx, ny) != (sx, sy) và (nx, ny) ở trong mê cung và từ (sx, sy) đi được sang (nx, ny)
        if ((nx != sx || ny != sy) && Inside(nx, ny) && avail[sx, sy, GetDir(sx, sy, nx, ny)]) {
            /// Nếu như đi vào một ô chưa đi qua
            if (state[nx, ny] == OUTSIDE) {
                setCeilColor(nx, ny, YELLOW);
                state[nx, ny] = INSIDE;
            } else {
                /// Nếu đi vào một ô đã đi qua (phạm luật)
                flag = -1;
                state[sx, sy] = OUTSIDE;
                setCeilColor(sx, sy, WHITE);
                setCeilColor(nx, ny, RED);
                Destroy(player);
                player = Instantiate(pointerFault, matrixCell[nx, ny].transform.position, Quaternion.identity);
            }
            /// Chuyển Object của player sang vị trí mới
            player.transform.position = matrixCell[nx, ny].transform.position + playerOffset;
            sx = nx;
            sy = ny;
            /// Nếu như đã tới đích thì đổi màu hết các ô đã đi qua thành màu xanh
            if (sx == fx && sy == fy) {
                for (int i = 0; i < n; i++) {
                    for (int j = 0; j < n; j++) {
                        if (state[i, j] == INSIDE) {
                            setCeilColor(i, j, GREEN);
                        }
                    }
                }
                return 1;
            }
        }
        return flag;
    }
}
