using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Ending : MonoBehaviour
{

    public Door door;
    public PhotoScript photoScript;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (door.GetOpened())
        {
            List<Photo> activePhotos = photoScript.activePhotos;
            bool hasGreen = false;

            for (int i = 0; i < activePhotos.Count; i++)
            {
                if (activePhotos[i].obInPhoto == ObjectType.GREEN)
                {
                    hasGreen = true;
                }
            }

            if (hasGreen)
            {
                Debug.Log("You win!");
            }
            else
            {
                Debug.Log("Not enough evidence");
            }

            SceneManager.LoadScene("Set with interior");
        }
    }
}
