using System.Collections;
using System.Collections.Generic;
using UnityEngine;


// 初始化位置的函数
public class produce : MonoBehaviour
{
    public GameObject prefab1;
    public GameObject OwnObject;
    private Vector3 pos;              // 实例的初始化位置

    // Start is called before the first frame update
    void Start()
    {
        pos = new Vector3(0, 8, 0);    
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            Instantiate(OwnObject, pos, Quaternion.identity);  // 用于初始化GameObject
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Instantiate(prefab1, pos, Quaternion.identity);
        }
    }
}
