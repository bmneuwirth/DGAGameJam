using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoScript : MonoBehaviour
{
    public const int MAX_PHOTOS = 20;

    public Material captureMaterial;
    public List<RenderTexture> photos;

    public int curPhotoIndex = 0;

    // Start is called before the first frame update
    void Start()
    {
        photos = new List<RenderTexture>();
        for (int i = 0; i < MAX_PHOTOS; i++)
        {
            RenderTexture rt = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            rt.Create();
            photos.Add(rt);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left click pressed");
            RenderTexture curPhoto = photos[curPhotoIndex];
            if (curPhoto && curPhoto.IsCreated())
            {
                Camera camera = Camera.main;
                RenderTexture prevTarget = camera.targetTexture;
                camera.targetTexture = curPhoto;
                camera.Render();
                captureMaterial.mainTexture = curPhoto;
                camera.targetTexture = prevTarget;
                curPhotoIndex += 1;
                curPhotoIndex %= MAX_PHOTOS; // Loop through
            }
            else
            {
                Debug.Log("Writing to render texture failed.");
            }
        }
    }
    private void OnDestroy()
    {
        for (int i = 0; i < MAX_PHOTOS; i++)
        {
            RenderTexture rt = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            if (rt && rt.IsCreated())
            {
                rt.Release();
            }
        }
    }
}
