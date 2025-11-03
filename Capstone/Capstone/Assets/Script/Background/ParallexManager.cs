using SkateGame;
using UnityEngine;
using System.Collections.Generic;

public class ParallexManager : MonoBehaviour
{
    Transform cameraTrans;

    [SerializeField] List<Transform> parallaxTransforms = new();
    List<Vector2> parallaxStartPos = new();
    [SerializeField] List<float> parallaxPowers = new();

    void Start()
    {
        cameraTrans = Camera.main.transform;

        Debug.LogWarning(parallaxTransforms.Count);

        for (int i = 0; i < parallaxTransforms.Count; i++)
        {
            parallaxStartPos.Add(parallaxTransforms[i].position);
        }
    }

    void FixedUpdate()
    {
        for (int i = 0; i < parallaxTransforms.Count; i++)
        {
            parallaxTransforms[i].position = new Vector3(parallaxStartPos[i].x + cameraTrans.position.x * parallaxPowers[i], parallaxStartPos[i].y);
        }
    }
}
