using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;
public class Photo
{
    public Photo(RenderTexture texture, Material mat)
    {
        this.texture = texture;
        this.material = mat;
        this.obInPhoto = ObjectType.NOTHING;
    }

    public RenderTexture texture { get; }

    public Material material { get; }

    public ObjectType obInPhoto { get; set; }

    // Eventually add flags for what photos contain
}

public class PhotoScript : MonoBehaviour
{
    public const int MAX_PHOTOS = 12;
    public const bool DEBUG_MODE = true;
    public const float DEBUG_SPEED = 5f;

    // How much of the area the object has to be of the photo to be a "good shot"
    public const float REQ_AREA = 0.005f;
    public const float FLASH_TIME = 0.5f;

    public new Camera camera;
    private bool inCameraMode = false;
    public float zoomMult = 0.5f;
    public float defaultFov = 90.0f;
    public float zoomSpeed = 10f; // Speed of zoom transition

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

    // Start is called before the first frame update
    void Start()
    {
        layerMask = 0 | (1 << LayerMask.NameToLayer("SpecialObject"));
        defaultFov = camera.fieldOfView;
        camera.backgroundColor = Color.black;
        crosshair.enabled = false;
        flash.enabled = false;

        activePhotos = new List<Photo>();
        inactivePhotos = new Stack<Photo>();

        inspectTexture = new RenderTexture(512, 512, 0, RenderTextureFormat.ARGB32);
        depthTexture = new RenderTexture(512, 512, 32, RenderTextureFormat.Depth);
        blitTexture = new RenderTexture(32, 32, 0, RenderTextureFormat.ARGB32);
        blitTexture2D = new Texture2D(32, 32, TextureFormat.ARGB32, false);

        for (int i = 0; i < MAX_PHOTOS; i++)
        {
            RenderTexture rt = new RenderTexture(512, 512, 16, RenderTextureFormat.ARGB32);
            Material mat = new Material(Shader.Find("Unlit/Texture"));
            if (mat == null)
            {
                Debug.Log("Error creating material");
            }
            rt.Create();
            Photo photo = new Photo(rt, mat);
            inactivePhotos.Push(photo);
        }

        if (DEBUG_MODE)
        {
            Debug.Log("PhotoScript started");
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

                        float scale = (camera.aspect - 1) / 2.0f;
                        screenPosOnRenderTexture.x *= (camera.aspect - 2 * scale) / camera.aspect;
                        screenPosOnRenderTexture.x += scale / camera.aspect;

                        Ray ray = camera.ViewportPointToRay(screenPosOnRenderTexture);
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
        for (int i = 0; i < activePhotos.Count; i++)
        {
            activePhotos[i].material.mainTexture = activePhotos[i].texture;
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
            photoToDelete.material.mainTexture = null;
        }
    }
}
