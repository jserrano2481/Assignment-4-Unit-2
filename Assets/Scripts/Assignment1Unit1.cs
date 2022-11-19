using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Assignment1Unit1 : MonoBehaviour
{



    public Vector3[] pos;

    public int posIndex;

    public float speed; 




    // Update is called once per frame
    void Update()
    {
        Vector3 target = pos[posIndex];
      
       
        transform.position = Vector3.MoveTowards(transform.position, target, (speed * Time.deltaTime));


        if(transform.position == target)
        {
            GoToNextPos();
        }
    }


   void GoToNextPos()
    {
        switch (posIndex)
        {
            case 0: posIndex = 1; break;
                case 1: posIndex = 2; break;
                    case 2: posIndex = 3; break;
                       case 3: posIndex = 0; break;

        }
    }


}
