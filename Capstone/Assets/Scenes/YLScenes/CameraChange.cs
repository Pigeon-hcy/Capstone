using Unity.Cinemachine;
using UnityEngine;

public class CameraChange : MonoBehaviour
{
    public CinemachineCamera main;
    public CinemachineCamera second;

    public int ChangeTo;
    void Start()
    {
        main.Priority = 99;
        second.Priority = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            switch (ChangeTo)
            {
                case 0:
                    main.Priority = 99;
                    second.Priority = 0;
                    break;

                case 1:
                    main.Priority = 0;
                    second.Priority = 99;
                    break;
            }
        }
    }
}
