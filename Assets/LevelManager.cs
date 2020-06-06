using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    // Start is called before the first frame update
    public void StartNavalGame()
    {
        SceneManager.LoadScene("NavalGame");
    }

    public void LoadNavalMenu()
    {
        SceneManager.LoadScene("NavalGameMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
