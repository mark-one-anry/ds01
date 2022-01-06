using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVars : MonoBehaviour
{
    public const string CRYSTAL_CONTAINER_NAME = "ManaCrystals";
    private Transform[] crystalsArray; // ������ ��������� � �����
    private bool crystalsArrayInitialized = false;
    
    // Start is called before the first frame update
    void Start()
    {
        if (InitCrystalArray())
            crystalsArrayInitialized = true;
    }

    bool InitCrystalArray()
    {
        GameObject CContainer = GameObject.Find(CRYSTAL_CONTAINER_NAME);
        if (CContainer == null)
            return false;

        crystalsArray = CContainer.GetComponentsInChildren<Transform>();
        // ������� ������ ��������� �.�. ��� ��� ��������� 
        /*CContainer.transform.
        foreach (Transform child in CContainer.transform)
        {
            crystalsArray[crystalsArray.Length] = child.gameObject.transform;
        }*/


        // Debug.Log("There are " + crystalsArray.Length + " crystals in the scene: ");
        foreach (Transform e in crystalsArray) {
           // Debug.Log(e.gameObject.name + " at " + e.position.x + "/" + e.position.y);
        }
        return true;
    }

    public Transform[] getCrystalArray()
    {
        // Debug.Log("getCrystalArray invoked: crystalsArrayInitialized = " + crystalsArrayInitialized + ", crystalsArray = " + crystalsArray);
        if (crystalsArrayInitialized)
            return crystalsArray;
        else
            return null;
    }
}
/*
function FindGameObjectsWithLayer(layer : int) : GameObject[]
{
    var goArray = FindObjectsOfType(GameObject);
    var goList = new System.Collections.Generic.List.< GameObject > ();
    for (var i = 0; i < goArray.Length; i++)
    {
        if (goArray[i].layer == layer)
        {
            goList.Add(goArray[i]);
        }
    }
    if (goList.Count == 0)
    {
        return null;
    }
    return goList.ToArray();
}
*/