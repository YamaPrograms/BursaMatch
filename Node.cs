using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Node : MonoBehaviour
{
    public bool isUsable;
    public GameObject lamba;

    public Node(bool _isUsable, GameObject _lamba)
    {
        isUsable = _isUsable;
        lamba = _lamba;
    }


}
