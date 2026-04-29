using UnityEngine;

public class PlayerUI : MonoBehaviour
{
    private static PlayerUI _instance;
    public static PlayerUI Instance {get => _instance;}

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
