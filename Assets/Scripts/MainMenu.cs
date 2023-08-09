using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [Header("Buttons")]
    public Button easyButton;
    public Button mediumButton;
    public Button hardButton;

    [Header("Texts")]
    public TMP_Text easyHighScore;
    public TMP_Text mediumHighScore;
    public TMP_Text hardHighScore;

    [Header("Sprites")]
    public Sprite button;
    public Sprite clickedButton;

    float minutes;
    float seconds;
    float time;
    
    void Start()
    {
        if(PlayerPrefs.HasKey("difficulty"))
        {
            switch(PlayerPrefs.GetInt("difficulty"))
            {
                case 1:
                    EasyButton();
                    break;
                case 2:
                    MediumButton();
                    break;
                case 3:
                    HardButton();
                    break;
            }
        }
        else
        {
            EasyButton();
        }

        if(PlayerPrefs.HasKey("easy"))
        {
            time = PlayerPrefs.GetFloat("easy");
            minutes = Mathf.FloorToInt(time / 60);
            seconds = Mathf.FloorToInt(time % 60);            
            
            easyHighScore.text = "easy : " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (PlayerPrefs.HasKey("medium"))
        {
            time = PlayerPrefs.GetFloat("medium");
            minutes = Mathf.FloorToInt(time / 60);
            seconds = Mathf.FloorToInt(time % 60);

            mediumHighScore.text = "medium : " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }

        if (PlayerPrefs.HasKey("hard"))
        {
            time = PlayerPrefs.GetFloat("hard");
            minutes = Mathf.FloorToInt(time / 60);
            seconds = Mathf.FloorToInt(time % 60);

            hardHighScore.text = "hard : " + string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void StartButton()
    {
        SceneManager.LoadScene(1);
    }

    public void QuitButton()
    {
        Application.Quit();
    }

    public void EasyButton()
    {
        PlayerPrefs.SetInt("difficulty", 1);
        mediumButton.GetComponent<Image>().sprite = button;
        hardButton.GetComponent<Image>().sprite = button;
        easyButton.GetComponent<Image>().sprite = clickedButton;
    }

    public void MediumButton()
    {
        PlayerPrefs.SetInt("difficulty", 2);
        mediumButton.GetComponent<Image>().sprite = clickedButton;
        hardButton.GetComponent<Image>().sprite = button;
        easyButton.GetComponent<Image>().sprite = button;
    }

    public void HardButton()
    {
        PlayerPrefs.SetInt("difficulty", 3);
        mediumButton.GetComponent<Image>().sprite = button;
        hardButton.GetComponent<Image>().sprite = clickedButton;
        easyButton.GetComponent<Image>().sprite = button;
    }
}
