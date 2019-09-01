using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewBehaviourScript : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject Object;
    void Start()
    {
        
        Object.gameObject.transform.GetComponent<RectTransform>().position = new Vector3();

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
