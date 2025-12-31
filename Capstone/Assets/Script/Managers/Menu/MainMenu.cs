using UnityEngine;
using UnityEngine.SceneManagement;
using SkateGame;

public class MainMenu : MonoBehaviour
{
    public void OnClickStart()
    {
        GameStateController.Instance.EnterInGame();
        SceneManager.LoadScene("1-1");
    }
}
