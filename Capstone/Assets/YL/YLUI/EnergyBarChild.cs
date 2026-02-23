using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBarChild : MonoBehaviour
{
    Image image;
    MMSpringScale mmSpringScale;
    MMSpringRectTransformPosition mmSpringRectTrans;

    bool wasActive;

    void Start()
    {
        image = GetComponent<Image>();
        mmSpringScale = GetComponent<MMSpringScale>();
        mmSpringRectTrans = GetComponent<MMSpringRectTransformPosition>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Activate()
    {
        if (wasActive) return;

        image.color = Color.white;
        mmSpringRectTrans.Bump(new Vector3(750, 1000, 50));
        wasActive = true;
    }

    public void Deactivate()
    {
        if (!wasActive) return;

        image.color = Color.black;
        mmSpringScale.Bump(new Vector3(20, 20, 20));
        wasActive = false;
    }
}
