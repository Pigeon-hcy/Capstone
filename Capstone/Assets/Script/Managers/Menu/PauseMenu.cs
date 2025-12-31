using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    private static PauseMenu _instance;
    public static PauseMenu Instance {get => _instance;}
    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
