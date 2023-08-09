using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour
{
    public class Node
    {
        public int bombsTouched;
        public bool isOpened, isFlagged, isBomb;
        public Node right, left, up, down, upLeft, upRight, downLeft, downRight;
        public GameObject tile;
        public Sprite realSprite;
        public Node()
        {
            bombsTouched = 0;
            isOpened = false;
            isFlagged = false;
            isBomb = false;
        }
    }

    Node head = new();

    [Header("GamePlay")]
    int difficulty;
    public string[] difficulties;
    int gridSize;
    int bombAmount;
    float density;
    int tileAmount;
    int openedTileAmount;
    int openedTileCount = 0;
    int flagAmount;
    int flagCount = 0;
    public GameObject tile;
    bool gameEnd = false;
    bool gameStart = false;
    bool gamePaused = false;
    bool firstTouch = true;

    [Header("Sprites")]
    public Sprite unopenedTile;
    public Sprite bombClicked;
    public Sprite otherBomb;
    public Sprite rightFlag;
    public Sprite wrongFlag;
    public Sprite[] sprites;

    [Header("UI")]
    public TMP_Text flagCountText;
    public TMP_Text difficultyText;
    public GameObject endGamePanel;
    public Animator endGamePanelAnim;
    public GameObject winPanel;
    public GameObject losePanel;
    public TMP_Text gameEndTime;
    public TMP_Text gameEndBestTime;
    public GameObject newBestText;
    public GameObject exitEndGameButton;
    public GameObject pausePanel;

    [Header("Timer")]
    float timeValue;
    public TMP_Text currentTimerText;
    public TMP_Text bestTimerText;
    float timeToBeat;

    [Header("Sound Effect")]
    public AudioSource ac;
    public AudioClip bombSound;
    public AudioClip flagSound;
    public AudioClip winSound;

    Ray ray;
    RaycastHit hit;

    [Header("Camera")]
    public Camera mainCam;

    void Start()
    {
        SetUpBoard();
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            PauseButton();
        }

        if(openedTileCount == openedTileAmount && !gameEnd)
        {
            WinGame();
        }

        if(!gameEnd && !gamePaused && gameStart)
        {
            timeValue += Time.deltaTime;

            float minutes = Mathf.FloorToInt(timeValue / 60);
            float seconds = Mathf.FloorToInt(timeValue % 60);

            currentTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit) && !gameEnd && !gamePaused)
        {
            if(hit.collider.CompareTag("Tile"))
            {
                if(Input.GetMouseButtonDown(0))
                {
                    string coords = hit.collider.name;
                    string[] splitted = coords.Split(' ');
                    int coorX, coorY;
                    int.TryParse(splitted[0], out coorX);
                    int.TryParse(splitted[1], out coorY);
                    Node temp = head;
                    for (int i = 0; i < coorX; i++) temp = temp.right;
                    for (int i = 0; i < coorY; i++) temp = temp.up;
                    if (temp.isOpened && !temp.isFlagged)
                    {
                        OpenOpened(temp);
                    }
                    else if (!temp.isOpened && !temp.isFlagged)
                    {
                        if (firstTouch)
                        {
                            gameStart = true;
                            firstTouch = false;
                            PlantBombs(temp);
                            CalculateBoard();
                        }

                        if (temp.isBomb)
                        {
                            LoseGame(temp);
                        }
                        else
                        {
                            OpenAdjacent(temp);
                        }
                    }
                }

                if(Input.GetMouseButtonDown(1))
                {
                    string coords = hit.collider.name;
                    string[] splitted = coords.Split(' ');
                    int coorX, coorY;
                    int.TryParse(splitted[0], out coorX);
                    int.TryParse(splitted[1], out coorY);
                    Node temp = head;
                    for (int i = 0; i < coorX; i++) temp = temp.right;
                    for (int i = 0; i < coorY; i++) temp = temp.up;
                    if(!temp.isOpened)
                    {
                        if(!temp.isFlagged && flagCount > 0)
                        {
                            flagCount--;
                            temp.isFlagged = true;
                            temp.tile.GetComponent<SpriteRenderer>().sprite = rightFlag;
                            flagCountText.text = flagCount.ToString();
                            ac.PlayOneShot(flagSound);
                        }
                        else if(temp.isFlagged)
                        {
                            flagCount++;
                            temp.isFlagged = false;
                            temp.tile.GetComponent<SpriteRenderer>().sprite = unopenedTile;
                            flagCountText.text = flagCount.ToString();
                        }
                    }
                }
            }
        }
    }

    void SetUpBoard()
    {
        if(PlayerPrefs.HasKey("difficulty"))
        {
            difficulty = PlayerPrefs.GetInt("difficulty");
        }
        else
        {
            difficulty = 1;
        }

        switch (difficulty)
        {
            case 1:
                gridSize = 10;
                density = 0.12f;
                break;
            case 2:
                gridSize = 15;
                density = 0.15f;
                break;
            case 3:
                gridSize = 20;
                density = 0.18f;
                break;
        }
        tileAmount = gridSize * gridSize;
        bombAmount = (int)(tileAmount * density);
        openedTileAmount = tileAmount - bombAmount;
        flagAmount = bombAmount;
        flagCount = flagAmount;        

        Node temp = head;
        for (int i = 1; i < gridSize; i++)
        {
            Node temp2 = new();
            temp.right = temp2;
            temp2.left = temp;
            temp = temp2;
        }

        temp = head;
        for (int i = 1; i < gridSize; i++)
        {
            Node anchor = new();
            Node temp2 = anchor;
            for (int j = 1; j < gridSize; j++)
            {
                Node temp3 = new();
                temp2.right = temp3;
                temp3.left = temp2;
                temp2 = temp3;
            }

            temp2 = anchor;

            Node hold = temp;
            for (int j = 0; j < gridSize; j++)
            {
                hold.up = temp2;
                temp2.down = hold;
                hold.upRight = temp2.right;
                temp2.downRight = hold.right;
                hold.upLeft = temp2.left;
                temp2.downLeft = hold.left;
                hold = hold.right;
                temp2 = temp2.right;
            }

            temp = temp.up;
        }

        Node row = head;
        Node col;
        for (int y = 0; y < gridSize; y++)
        {
            col = row;
            for (int x = 0; x < gridSize; x++)
            {
                Vector2 spawnPos = new(x, y);
                GameObject currTile = Instantiate(tile, spawnPos, Quaternion.identity);
                currTile.name = x.ToString() + " " + y.ToString();
                col.tile = currTile;
                col = col.right;
            }
            row = row.up;
        }

        UISetUp();

        mainCam.transform.position = new Vector3(((float)gridSize - 1) / 2, ((float)gridSize - 1) / 2, mainCam.transform.position.z);
        mainCam.orthographicSize = (float)gridSize / 2 + 1;
    }

    void PlantBombs(Node firstTouched)
    {
        int x, y, bombCount = 0;
        Node temp;

        while(bombCount < bombAmount)
        {
            do
            {
                temp = head;
                x = Random.Range(1, 1000) % gridSize;
                y = Random.Range(1, 1000) % gridSize;
                for (int i = 1; i < x; i++) temp = temp.right;
                for (int i = 1; i < y; i++) temp = temp.up;
            } while (temp.isBomb || temp == firstTouched);

            temp.isBomb = true;
            bombCount++;
        }
    }

    void CalculateBoard()
    {
        Node temp = head;
        Node temp2;
        
        for(int i = 0; i < gridSize; i++)
        {
            temp2 = temp;
            for(int j = 0; j < gridSize; j++)
            {
                if(!temp2.isBomb)
                {
                    if (temp2.up != null && temp2.up.isBomb) temp2.bombsTouched++;
                    if (temp2.down != null && temp2.down.isBomb) temp2.bombsTouched++;
                    if (temp2.left != null && temp2.left.isBomb) temp2.bombsTouched++;
                    if (temp2.right != null && temp2.right.isBomb) temp2.bombsTouched++;
                    if (temp2.upLeft != null && temp2.upLeft.isBomb) temp2.bombsTouched++;
                    if (temp2.upRight != null && temp2.upRight.isBomb) temp2.bombsTouched++;
                    if (temp2.downLeft != null && temp2.downLeft.isBomb) temp2.bombsTouched++;
                    if (temp2.downRight != null && temp2.downRight.isBomb) temp2.bombsTouched++;

                    temp2.realSprite = sprites[temp2.bombsTouched];
                }

                temp2 = temp2.right;
            }
            temp = temp.up;
        }
    }

    void OpenAdjacent(Node temp)
    {
        if (temp == null || temp.isBomb || temp.isFlagged || temp.isOpened) return;
        OpenTile(temp);
        if(temp.bombsTouched == 0)
        {
            OpenAdjacent(temp.up);
            OpenAdjacent(temp.down);
            OpenAdjacent(temp.left);
            OpenAdjacent(temp.right);
            OpenAdjacent(temp.upLeft);
            OpenAdjacent(temp.upRight);
            OpenAdjacent(temp.downLeft);
            OpenAdjacent(temp.downRight);
        }
        else
        {
            if (temp.up != null && temp.up.bombsTouched == 0) OpenAdjacent(temp.up);
            if (temp.down != null && temp.down.bombsTouched == 0) OpenAdjacent(temp.down);
            if (temp.left != null && temp.left.bombsTouched == 0) OpenAdjacent(temp.left);
            if (temp.right != null && temp.right.bombsTouched == 0) OpenAdjacent(temp.right);
        }
    }
    
    void SpecialOpenAdjacent(Node temp)
    {
        if(temp.isFlagged) return;

        if (temp.up != null) OpenAdjacent(temp.up);
        if (temp.down != null) OpenAdjacent(temp.down);
        if (temp.left != null) OpenAdjacent(temp.left);
        if (temp.right != null) OpenAdjacent(temp.right);
        if (temp.upLeft != null) OpenAdjacent(temp.upLeft);
        if (temp.upRight != null) OpenAdjacent(temp.upRight);
        if (temp.downLeft != null) OpenAdjacent(temp.downLeft);
        if (temp.downRight != null) OpenAdjacent(temp.downRight);
    }

    void OpenOpened(Node temp)
    {
        int flagsTouched = 0;
        if (temp.up != null && temp.up.isFlagged) flagsTouched++;
        if (temp.down != null && temp.down.isFlagged) flagsTouched++;
        if (temp.left != null && temp.left.isFlagged) flagsTouched++;
        if (temp.right != null && temp.right.isFlagged) flagsTouched++;
        if (temp.upLeft != null && temp.upLeft.isFlagged) flagsTouched++;
        if (temp.upRight != null && temp.upRight.isFlagged) flagsTouched++;
        if (temp.downLeft != null && temp.downLeft.isFlagged) flagsTouched++;
        if (temp.downRight != null && temp.downRight.isFlagged) flagsTouched++;

        if(flagsTouched >= temp.bombsTouched)
        {
            if (temp.up != null && temp.up.isBomb && !temp.up.isFlagged) LoseGame(temp.up);
            else if (temp.down != null && temp.down.isBomb && !temp.down.isFlagged) LoseGame(temp.down);
            else if (temp.left != null && temp.left.isBomb && !temp.left.isFlagged) LoseGame(temp.left);
            else if (temp.right != null && temp.right.isBomb && !temp.right.isFlagged) LoseGame(temp.right);
            else if (temp.upRight != null && temp.upRight.isBomb && !temp.upRight.isFlagged) LoseGame(temp.upRight);
            else if (temp.upLeft != null && temp.upLeft.isBomb && !temp.upLeft.isFlagged) LoseGame(temp.upLeft);
            else if (temp.downRight != null && temp.downRight.isBomb && !temp.downRight.isFlagged) LoseGame(temp.downRight);
            else if (temp.downLeft != null && temp.downLeft.isBomb && !temp.downLeft.isFlagged) LoseGame(temp.downLeft);
            else SpecialOpenAdjacent(temp);            
        }
    }

    void OpenTile(Node temp)
    {
        if (temp == null || temp.isFlagged) return;
        temp.isOpened = true;
        temp.tile.GetComponent<SpriteRenderer>().sprite = temp.realSprite;
        openedTileCount++;
    }

    void WinGame()
    {
        Node temp = head, temp2;
        gameEnd = true;
        ac.PlayOneShot(winSound);
        for(int i = 0; i < gridSize; i++)
        {
            temp2 = temp;
            for(int j = 0; j < gridSize; j++)
            {
                if(!temp2.isOpened && !temp2.isFlagged && temp2.isBomb)
                {
                    temp2.tile.GetComponent<SpriteRenderer>().sprite = otherBomb;
                }
                temp2 = temp2.right;
            }

            temp = temp.up;
        }

        if(timeValue < timeToBeat)
        {
            PlayerPrefs.SetFloat(difficulties[difficulty - 1], timeValue);
            StartCoroutine(CallEndGame(true, true));
        }
        else
        {
            StartCoroutine(CallEndGame(true));
        }

    }

    void LoseGame(Node temp)
    {
        temp.tile.GetComponent<SpriteRenderer>().sprite = bombClicked;
        temp.isOpened = true;
        gameEnd = true;
        ac.PlayOneShot(bombSound);

        Node temp2 = head, temp3;
        for(int i = 0; i < gridSize; i++)
        {
            temp3 = temp2;
            for(int j = 0; j < gridSize; j++)
            {
                if(temp3 == temp)
                {
                    temp3 = temp3.right;
                    continue;
                }
                if(temp3.isFlagged && !temp3.isBomb) temp3.tile.GetComponent<SpriteRenderer>().sprite = wrongFlag;
                else if(temp3.isBomb && !temp3.isFlagged) temp3.tile.GetComponent<SpriteRenderer>().sprite = otherBomb;
                else if(!temp3.isBomb && !temp3.isOpened) temp3.tile.GetComponent<SpriteRenderer>().sprite = temp3.realSprite;
                temp3.isOpened = true;

                temp3 = temp3.right;
            }

            temp2 = temp2.up;
        }

        StartCoroutine(CallEndGame(false));
    }

    void UISetUp()
    {
        difficultyText.text = difficulties[difficulty - 1];
        flagCountText.text = flagCount.ToString();

        if (PlayerPrefs.HasKey(difficulties[difficulty - 1]))
        {
            timeToBeat = PlayerPrefs.GetFloat(difficulties[difficulty - 1]);
            float minutes = Mathf.FloorToInt(timeToBeat / 60);
            float seconds = Mathf.FloorToInt(timeToBeat % 60);

            bestTimerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
        else timeToBeat = float.MaxValue;
    }

    void EndGame(bool win, bool newBest)
    {
        winPanel.SetActive(win);
        gameEndTime.text = currentTimerText.text;
        if(newBest)
        {
            gameEndBestTime.text = currentTimerText.text;
            newBestText.SetActive(newBest);
        }
        else if(!newBest)
        {
            gameEndBestTime.text = bestTimerText.text;
        }

        losePanel.SetActive(!win);
        exitEndGameButton.SetActive(true);
    }

    IEnumerator CallEndGame(bool con1, bool con2 = false)
    {
        endGamePanel.SetActive(true);
        endGamePanelAnim.SetTrigger("End");
        yield return new WaitForSeconds(3);
        EndGame(con1, con2);
    }

    public void ExitButton()
    {
        SceneManager.LoadScene(0);
    }

    public void PauseButton()
    {
        pausePanel.SetActive(!pausePanel.activeSelf);
        gamePaused = pausePanel.activeSelf;
    }
}
