using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestBehavior : MonoBehaviour
{


    private bool dirRight = true;
    public float speed = 2.0f;
    public Material cubeColor;



    // Start is called before the first frame update
    void Start()
    {
        cubeColor = GetComponent<Renderer>().material;
    }

   



    void Update()
    {
        if (dirRight)
            transform.Translate(Vector2.right * speed * Time.deltaTime);
        else
            transform.Translate(-Vector2.right * speed * Time.deltaTime);

        if (transform.position.x >= 4.0f)
        {
            dirRight = false;
            cubeColor.color = Color.red; 
        }

        if (transform.position.x <= -4)
        {
            dirRight = true;
            cubeColor.color = Color.blue;
        }
    }
}
