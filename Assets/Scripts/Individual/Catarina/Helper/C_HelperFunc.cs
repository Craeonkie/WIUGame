using System.Collections.Generic;
using UnityEngine;

public class C_HelperFunc:MonoBehaviour
{
    public static List<GameObject> FindSpecificObjectsWithNoParent(LayerMask _layer)
    {
        List<GameObject> list = new List<GameObject>();

       var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // Check layer
            if (((1 << obj.layer) & _layer) != 0)
            {
                // Check it has no parent
                if (obj.transform.parent == null)
                {
                    list.Add(obj);
                }
            }
        }
        return list;
    }

    public static List<GameObject> FindSpecificObjectsWithNoParentTag(string _tag)
    {
        List<GameObject> list = new List<GameObject>();

        var allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            // Check layer
            if (obj.CompareTag(_tag))
            {
                // Check it has no parent
                if (obj.transform.parent == null)
                {
                    list.Add(obj);
                }
            }
        }
        return list;
    }
}
