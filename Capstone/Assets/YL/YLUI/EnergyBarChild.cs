using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.UI;

public class EnergyBarChild : MonoBehaviour
{
    Image image;
    MMSpringScale mmSpringScale;
    MMSpringRectTransformPosition mmSpringRectTrans;

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
        image.color = Color.white;
        mmSpringRectTrans.Bump(new Vector3(750, 1000, 50));
    }

    public void Deactivate()
    {
        image.color = Color.black;
        mmSpringScale.Bump(new Vector3(20, 20, 20));
    }
}
