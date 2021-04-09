using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SmartMusic : MonoBehaviour
{
    public Button play;
    public Button stop;
    public Button pause;
    public Button downMusic;
    public Button upMusic;
    public Button connect;

    public Transform root;


    // Start is called before the first frame update
    void Start()
    {
        root = GameObject.Find("Canvas").GetComponent<Transform>();
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
