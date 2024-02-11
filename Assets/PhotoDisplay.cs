using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using UnityEngine;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.UI;

public class PhotoDisplay : MonoBehaviour
{
    public int slotIndex; 

    void Start()
    {
        LoadPhoto();
    }
    void LoadPhoto()
    {
        if (GameManager.instance != null && slotIndex >= 0 && slotIndex < GameManager.instance.capturedPhotos.Length)
        {
            RenderTexture photoTexture = GameManager.instance.capturedPhotos[slotIndex];
            if (photoTexture != null)
            {
                Texture2D photoTexture2D = new Texture2D(photoTexture.width, photoTexture.height);
                RenderTexture.active = photoTexture;
                photoTexture2D.ReadPixels(new Rect(0, 0, photoTexture.width, photoTexture.height), 0, 0);
                photoTexture2D.Apply();
                GetComponent<Image>().sprite = Sprite.Create(photoTexture2D, new Rect(0, 0, photoTexture.width, photoTexture.height), Vector2.one * 0.5f);
            }
            else
            {
                Debug.LogError("No photo found at slot index " + slotIndex);
            }
        }
        else
        {
            Debug.LogError("GameManager instance is null or invalid slot index: " + slotIndex);
        }
    }
}