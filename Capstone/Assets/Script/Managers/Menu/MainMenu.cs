using UnityEngine;
using UnityEngine.SceneManagement;
using SkateGame;

public class MainMenu : MonoBehaviour
{
    public void OnClickStart()
    {
        if (GameStateController.Instance != null)
            GameStateController.Instance.EnterInGame();
        SceneManager.LoadScene("New_1-1");
    }
}
