
using UnityEngine;
using BaseUtility;

public class DizzyController : MessageBehavior
{

    public Material dizzyMat;

    public float dizzySpeed = 1;
    public float dizzyDownSpeed = 3;

    public float dizzyOffsetMax = 0.05f;


    private float dizzyCount = 0;
    private bool inDizzy = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SafeRegister<PostProTag>(PostProTag.Dizzy, DizzyHandler);
    }

    void Update()
    {
       
    }

    void LateUpdate()
    {
        if (inDizzy)
        {
            dizzyCount = Mathf.Clamp01(dizzyCount + dizzySpeed * Time.deltaTime);
        }
        else
        {
            dizzyCount = Mathf.Clamp01(dizzyCount - dizzyDownSpeed * Time.deltaTime);
        }
        dizzyMat.SetFloat("_Offset", dizzyCount*dizzyOffsetMax);
        inDizzy = false;
    }

    public void DizzyHandler(MessageBox box, MonoBehaviour sender)
    {
        inDizzy = true;
    }
}

