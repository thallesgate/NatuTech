using System.Collections.Generic;
using UnityEngine;

public class DestroyObjectsWithTag : MonoBehaviour
{
    public static void DestroyObjects(string tag)
    {
        List<GameObject> objetos = new List<GameObject>(GameObject.FindGameObjectsWithTag(tag));

        if (objetos != null)
        {
            for (int i = 0; i < objetos.Count; i++)
            {
                if (objetos[i] != null)
                {
                    Destroy(objetos[i]);
                }
            }
        }
        else
        {
            Debug.Log("DestroyObjects: Objeto Nulo!");
        }
    }

    public static void DestroyObject(string tag)
    {
        GameObject objeto = GameObject.FindGameObjectWithTag(tag);

        if (objeto != null)
        {
            Destroy(objeto);
        }
        else
        {
            Debug.Log("DestroyObject: Objeto Nulo!");
        }
    }
}
