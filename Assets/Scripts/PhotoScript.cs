using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
using static Unity.VisualScripting.Member;
using static UnityEditor.Experimental.GraphView.GraphView;
using static UnityEngine.GraphicsBuffer;

public class Photo
{
    public Photo(RenderTexture texture)
    {
        this.texture = texture;
        this.obInPhoto = ObjectType.NOTHING;
    }

    public RenderTexture texture { get; }

    public ObjectType obInPhoto { get; set; }

    // Eventually add flags for what photos contain
}

public class PhotoScript : MonoBehaviour
{
    public const int MAX_PHOTOS = 20;
    public const bool DEBUG_MODE = true;
    public const float DEBUG_SPEED = 5f;
    public Vector3 DEBUG_CENTER;
    // How much of the area the object has to be of the photo to be a "good shot"
    public const float REQ_AREA = 0.005f;
    public const float FLASH_TIME = 0.5f;

    public bool inCameraMode = false;
    public float zoomMult = 0.5f;
    public float defaultFov;
    public float zoomSpeed = 10f; // Speed of zoom transition

    // Objects that are special and can be in photo
    public List<GameObject> targetObjects;

    public GameObject planePrefab;
    private GameObject[] photosPlanes;

    // Active photos are a list we can remove things from
    public List<Photo> activePhotos;

    // Inactive photos are a stack we can pull from and add to the list
    private Stack<Photo> inactivePhotos;

    // We use this RenderTexture to detect if certain objects are in the photo
    private RenderTexture inspectTexture;

    // Save the depth from the initial pass
    private RenderTexture depthTexture;

    // We use this RenderTexture to blit to so we don't have to check so many pixels
    private RenderTexture blitTexture;

    // Texture2D to copy the blitted texture to for CPU checking stuff
    private Texture2D blitTexture2D;

    // Layer for camera detection to render
    int layerMask;

    // Crosshair UI for camera mode
    public Image crosshair;

    // Image flash for camera taken
    public Image flash;
    private float timeSinceFlash;

    // All the fields below are for debugging
    private GameObject[] debugPlanes;
    public Material greenMat;
    public Material blueMat;
    public Material redMat;

    // Start is called before the first frame update
    void Start()
    {
        layerMask = 0 | (1 << LayerMask.NameToLayer("SpecialObject"));
        defaultFov = Camera.main.fieldOfView;
        Camera.main.backgroundColor = Color.black;
        crosshair.enabled = false;
        flash.enabled = false;

        activePhotos = new List<Photo>();
        inactivePhotos = new Stack<Photo>();
        photosPlanes = new GameObject[MAX_PHOTOS];
        debugPlanes = new GameObject[MAX_PHOTOS];

        inspectTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        depthTexture = new RenderTexture(512, 512, 32, RenderTextureFormat.Depth);
        blitTexture = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGB32);
        blitTexture2D = new Texture2D(32, 32, TextureFormat.ARGB32, false);

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

            planePos.y += 2;
            GameObject debugPlane = Instantiate(planePrefab, planePos, Quaternion.Euler(90, -90, 90));
            debugPlanes[i] = debugPlane;

            Material planeMaterial = new Material(Shader.Find("Standard"));
            photoPlane.GetComponent<MeshRenderer>().material = planeMaterial;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Toggle between zoomed in and zoomed out
        if (Input.GetMouseButtonDown(1))
        {
            inCameraMode = !inCameraMode;
            crosshair.enabled = inCameraMode;
        }

        // Logic for switching modes
        float targetFOV = defaultFov * (inCameraMode ? zoomMult : 1);
        Camera.main.fieldOfView = Mathf.Lerp(Camera.main.fieldOfView, targetFOV, zoomSpeed * Time.deltaTime);

        // Taking photos
        if (Input.GetMouseButtonDown(0) && inCameraMode)
        {
            if (inactivePhotos.Count == 0)
            {
                Debug.Log("Out of photos! Delete some");
            }
            else
            {
                Photo curPhoto = inactivePhotos.Pop();
                // Make sure the photo exists
                if (curPhoto.texture && curPhoto.texture.IsCreated())
                {
                    // Start the flash
                    flash.enabled = true;
                    timeSinceFlash = 0.0f;

                    // Save old camera values
                    Camera camera = Camera.main;
                    RenderTexture prevTarget = camera.targetTexture;
                    int oldMask = camera.cullingMask;
                    CameraClearFlags oldClearFlags = camera.clearFlags;

                    // Take picture
                    camera.SetTargetBuffers(curPhoto.texture.colorBuffer, depthTexture.depthBuffer);
                    camera.Render();
                    activePhotos.Add(curPhoto);

                    // Prep camera for content testing pass
                    camera.clearFlags = CameraClearFlags.Nothing;
                    camera.cullingMask = layerMask;

                    // Take other picture
                    Graphics.SetRenderTarget(inspectTexture);
                    GL.Clear(false, true, Color.clear); // Clear the color buffer for the initial 
                    Graphics.SetRenderTarget(null);
                    camera.SetTargetBuffers(inspectTexture.colorBuffer, depthTexture.depthBuffer);
                    camera.Render();
                    /*                    Graphics.SetRenderTarget(null);
                    */
                    // Cleanup
                    camera.clearFlags = oldClearFlags;
                    camera.cullingMask = oldMask;
                    camera.targetTexture = prevTarget;

                    // Check the pixels for non-black
                    Graphics.Blit(inspectTexture, blitTexture);
                    RenderTexture.active = blitTexture;
                    blitTexture2D.ReadPixels(new Rect(0, 0, blitTexture.width, blitTexture.height), 0, 0);

                    // Cleanup
                    RenderTexture.active = null; // Reset active RenderTexture

                    // Count non-black pixels
                    int nonBlackPixels = 0;
                    Color[] pixels = blitTexture2D.GetPixels();
                    Color compare = new Color(0, 0, 0, 0);

                    // Right now, arbitrarily selects the subject of the photo if there are multiple special objects
                    // By sampling depth buffer and picking closest non-black pixel here, could deterministicly pick the closest object as the subject
                    int latestNonBlack = -1;
                    for (int i = 0; i < pixels.Length; i++)
                    {
                        if (pixels[i] != compare)
                        {
                            nonBlackPixels++;
                            latestNonBlack = i;
                        }

                    }

                    // Check against threshold
                    if (nonBlackPixels > REQ_AREA * blitTexture.width * blitTexture.height)
                    {
                        // See which object it is
                        Vector3 screenPosOnRenderTexture = new Vector3(((latestNonBlack % blitTexture.width) - 0.5f) / (float)blitTexture.width, ((latestNonBlack / blitTexture.height) - 0.5f) / (float)blitTexture.height);

                        float scale = (Camera.main.aspect - 1) / 2.0f;
                        screenPosOnRenderTexture.x *= (Camera.main.aspect - 2 * scale) / Camera.main.aspect;
                        screenPosOnRenderTexture.x += scale / Camera.main.aspect;

                        Ray ray = Camera.main.ViewportPointToRay(screenPosOnRenderTexture);
                        RaycastHit hit;
                        if (Physics.Raycast(ray, out hit))
                        {
                            GameObject hitObject = hit.collider.gameObject;

                            if (hitObject.GetComponent<SpecialObject>() != null)
                            {
                                curPhoto.obInPhoto = hitObject.GetComponent<SpecialObject>().obType;
                            }
                            else
                            {
                                Debug.Log("Could not find special object");
                            }
                        }
                    }
                }
                else
                {
                    Debug.Log("Writing to render texture failed.");
                }

            }
        }

        // Flash logic
        if (flash.enabled)
        {
            flash.color = new Color(1, 1, 1, Mathf.Lerp(1, 0, timeSinceFlash / FLASH_TIME));
        }
        timeSinceFlash += Time.deltaTime;
        if (timeSinceFlash >= FLASH_TIME)
        {
            flash.enabled = false;
        }

        // Temp way to delete
        if (DEBUG_MODE && Input.GetKeyDown(KeyCode.R))
        {
            DeletePhoto(0);
        }

        // Code for displaying photos
        for (int i = 0; i < MAX_PHOTOS; i++)
        {
            GameObject curPlane = photosPlanes[i];
            GameObject curDebugPlane = debugPlanes[i];
            Material mat = curPlane.GetComponent<MeshRenderer>().material;
            curDebugPlane.GetComponent<MeshRenderer>().material = redMat;
            if (activePhotos.Count > i)
            {
                mat.mainTexture = activePhotos[i].texture;
                if (activePhotos[i].obInPhoto == ObjectType.GREEN)
                {
                    curDebugPlane.GetComponent<MeshRenderer>().material = greenMat;
                }
                else if (activePhotos[i].obInPhoto == ObjectType.BLUE)
                {
                    curDebugPlane.GetComponent<MeshRenderer>().material = blueMat;
                }
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

        if (inspectTexture && inspectTexture.IsCreated())
        {
            inspectTexture.Release();
        }
        if (blitTexture && blitTexture.IsCreated())
        {
            blitTexture.Release();
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
            photoToDelete.obInPhoto = ObjectType.NOTHING;
        }
    }
}
