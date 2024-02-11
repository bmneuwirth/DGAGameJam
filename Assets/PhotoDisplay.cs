using UnityEngine;
using UnityEngine.UI;

public class PhotoDisplay : MonoBehaviour
{
    public RenderTexture photoTexture; 
    public int slotIndex; 

    void Start()
    {
        LoadPhoto();
    }

    void LoadPhoto()
    {
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
            Debug.LogError("No photo texture assigned to the PhotoDisplay component.");
        }
    }
}