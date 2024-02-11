using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CutsceneScript : MonoBehaviour
{
    public SpecialObject obj;
    public Transform targetPosition;
    public float movementSpeed = 5f;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        obj.transform.position = Vector3.MoveTowards(obj.transform.position, targetPosition.position, movementSpeed * Time.deltaTime);
    }
}
