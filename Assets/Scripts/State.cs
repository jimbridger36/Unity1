using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class D 
{


    public LinkedList<int> lst;
    public int[] arr = { 0, 1, 2, 3, 4 };





    public void H() {
        Debug.Log("hello");
    }



    public D()
    {
        lst = new LinkedList<int>(arr);
        LinkedListNode<int> n1 = lst.First, n2; n2 = n1.Next;
        int v1 = n1.Value, v2; v2 = n2.Value;
        int count = lst.Count;


        







        for (int i = 1; i < count; i++)
        {

            for (int j = i+1; j < count+1; j++)
            {

                //Interactions

                if (j != count) {
                    n2 = n2.Next; v2 = n2.Value;
                }


            }



            if (i != count - 1)
            {
                n1 = n1.Next; v1 = n1.Value;
                n2 = n1.Next; v2 = n2.Value;
            }
        }













    }






}
