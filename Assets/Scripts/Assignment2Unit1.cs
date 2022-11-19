using System.Collections;
using System.Collections.Generic;
using UnityEngine;  

public class Assignment2Unit1 : MonoBehaviour
{

    public int[] array = { 0, 1, 2, 3 };


    // Start is called before the first frame update
    void Start()
    {

        InvokeRepeating("ArrayCount", 1, 3);

    }

    // Update is called once per frame
    void ArrayCount()
    {
        int largest = array[0];

         for (int i = 0; i < array.Length; i++)
        {
            if (array[i] > 2)
            {
                largest = array[i];
                Debug.Log("Value " + largest  + "    Index " + i);

                array[i] = (Random.Range(3, 10));
            }
        }

    }  
    
    
}
