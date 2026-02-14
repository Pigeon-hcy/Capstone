using UnityEngine;

public class ParticleRotator : MonoBehaviour
{
    public Transform root;
    public ParticleSystem particleSys;
    
    private ParticleSystemRenderer particleRenderer;

    void Start()
    {
        if(particleSys != null)
        {
            particleRenderer = particleSys.GetComponent<ParticleSystemRenderer>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(root != null && particleSys != null && particleRenderer != null)
        {

            float rotationY = root.transform.eulerAngles.y;

            if(Mathf.Abs(rotationY - 180f) < 1f)
            {
                particleRenderer.flip = new Vector3(1, 0, 0);
                //Debug.Log("Flip X: " + particleRenderer.flip + " (Rotation Y: " + rotationY + ")");
            }
            else if(Mathf.Abs(rotationY) < 1f || Mathf.Abs(rotationY - 360f) < 1f)
            {
                particleRenderer.flip = new Vector3(0, 0, 0);
                //Debug.Log("Flip X: " + particleRenderer.flip + " (Rotation Y: " + rotationY + ")");
            }
        }
    }
}
