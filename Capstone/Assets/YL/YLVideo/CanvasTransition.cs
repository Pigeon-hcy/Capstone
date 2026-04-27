using UnityEngine;

public class CanvasTransition : MonoBehaviour
{
    public static CanvasTransition Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}
