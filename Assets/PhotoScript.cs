using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Photo
{
    public Photo(RenderTexture texture)
    {
        this.texture = texture;
    }

    public RenderTexture texture { get; }

    // Eventually add flags for what photos contain
}

public class PhotoScript : MonoBehaviour
{
    public const int MAX_PHOTOS = 20;
    public const bool DEBUG_MODE = true;
    public const float DEBUG_SPEED = 0.01f;
    public Vector3 DEBUG_CENTER;

    public GameObject planePrefab;
    private GameObject[] photosPlanes;

    // Active photos are a list we can remove things from
    public List<Photo> activePhotos;

    // Inactive photos are a stack we can pull from and add to the list
    private Stack<Photo> inactivePhotos;

    // Start is called before the first frame update
    void Start()
    {
        activePhotos = new List<Photo>();
        inactivePhotos = new Stack<Photo>();
        photosPlanes = new GameObject[MAX_PHOTOS];


        for (int i = 0; i < MAX_PHOTOS; i++)
        {
            RenderTexture rt = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            rt.Create();
            Photo photo = new Photo(rt);
            inactivePhotos.Push(photo);

            Vector3 planePos = DEBUG_CENTER;
            planePos.x = (i - (MAX_PHOTOS / 2)) * 2;
            GameObject photoPlane = Instantiate(planePrefab, planePos, Quaternion.Euler(90, -90, 90));
            photosPlanes[i] = photoPlane;

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
            if (inactivePhotos.Count == 0)
            {
                Debug.Log("Out of photos! Delete some");
            }
            else
            {
                Photo curPhoto = inactivePhotos.Pop();
                if (curPhoto.texture && curPhoto.texture.IsCreated())
                {

                    Camera camera = Camera.main;
                    RenderTexture prevTarget = camera.targetTexture;
                    camera.targetTexture = curPhoto.texture;
                    camera.Render();
                    camera.targetTexture = prevTarget;

                    activePhotos.Add(curPhoto);
                }
                else
                {
                    Debug.Log("Writing to render texture failed.");
                }

            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            DeletePhoto(0);
        }

        // Update the photos (this only needs to happen if they are being displayed)
        for (int i = 0; i < MAX_PHOTOS; i++)
        {
            GameObject curPlane = photosPlanes[i];
            Material mat = curPlane.GetComponent<MeshRenderer>().material;
            if (activePhotos.Count > i)
            {
                mat.mainTexture = activePhotos[i].texture;
            }
            else
            {
                mat.mainTexture = null;
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

    /** Delete the ith photo (0-indexed) */
    public void DeletePhoto(int i)
    {
        if (activePhotos.Count > i)
        {
            Photo photoToDelete = activePhotos[i];
            activePhotos.RemoveAt(i);
            inactivePhotos.Push(photoToDelete);
        }
    }
}
