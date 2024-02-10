using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhotoScript : MonoBehaviour
{
    public const int MAX_PHOTOS = 20;
    public const bool DEBUG_MODE = true;
    public const float DEBUG_SPEED = 0.01f;
    public Vector3 DEBUG_CENTER;

    public GameObject planePrefab;
    public List<RenderTexture> photos;
    public List<GameObject> photosPlanes;

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

            Vector3 planePos = DEBUG_CENTER;
            planePos.x = (i - (MAX_PHOTOS / 2)) * 2;
            GameObject photoPlane = Instantiate(planePrefab, planePos, Quaternion.Euler(90, -90, 90));
            photosPlanes.Add(photoPlane);

            Material planeMaterial = new Material(Shader.Find("Standard"));
            photoPlane.GetComponent<MeshRenderer>().material = planeMaterial;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Very basic camera movement for debugging
        if (DEBUG_MODE)
        {
            float xAxisValue = Input.GetAxis("Horizontal");
            float zAxisValue = Input.GetAxis("Vertical");
            if (Camera.main != null)
            {
                Camera.main.transform.Translate(new Vector3(xAxisValue * DEBUG_SPEED, 0.0f, zAxisValue * DEBUG_SPEED));
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Left click pressed");
            RenderTexture curPhoto = photos[curPhotoIndex];
            if (curPhoto && curPhoto.IsCreated())
            {
                GameObject curPlane = photosPlanes[curPhotoIndex];

                Camera camera = Camera.main;
                RenderTexture prevTarget = camera.targetTexture;
                camera.targetTexture = curPhoto;
                camera.Render();

                Material captureMat = curPlane.GetComponent<MeshRenderer>().material;
                captureMat.mainTexture = curPhoto;
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
