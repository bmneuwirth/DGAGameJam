using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFollower : MonoBehaviour
{

    public Node[] nodes;
    public GameObject avatar;
    private int currentNode = 0;  
    public float movementSpeed = 5f;

    private Animator animator;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = avatar.GetComponent<Animator>();
        animator.SetBool("IsRunning", true);
    }

    // Update is called once per frame
    void Update()
    {
        avatar.transform.position = Vector3.MoveTowards(avatar.transform.position, nodes[currentNode].transform.position, movementSpeed * Time.deltaTime);

        avatar.transform.LookAt(nodes[currentNode].transform.position); 

        if (Vector3.Distance(avatar.transform.position, nodes[currentNode].transform.position) < 0.1f)
        {
            currentNode = currentNode + 1;
            if (currentNode == nodes.Length - 1)
            {
                currentNode = 0;
            }
        }
    }

    //void DrawLine()
    //{
    //    for (int i = 0; i < nodes.Length; i++)
    //    {
    //        if (i == nodes.Length - 1)
    //        {
    //            Debug.DrawLine(nodes[i].transform.position, nodes[0].transform.position, Color.green);
    //        }
    //        else
    //            Debug.DrawLine(nodes[i].transform.position, nodes[i + 1].transform.position, Color.green);
    //    }
    //}
}
