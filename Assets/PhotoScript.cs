using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoScript : MonoBehaviour
{
    public RenderTexture renderTexture;
    public Material captureMaterial; 

    // Start is called before the first frame update
    void Start()
    {
        if (renderTexture)
        {
            renderTexture.Create();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left click pressed");
            if (renderTexture && renderTexture.IsCreated())
            {
                Camera camera = Camera.main;
                RenderTexture prevTarget = camera.targetTexture;
                camera.targetTexture = renderTexture;
                camera.Render();
                captureMaterial.mainTexture = renderTexture;
                camera.targetTexture = prevTarget;
            }
            else
            {
                Debug.Log("Writing to render texture failed.");
            }
        }
    }
    private void OnDestroy()
    {
        if (renderTexture && renderTexture.IsCreated())
        {
            renderTexture.Release();
        }

    }
}
