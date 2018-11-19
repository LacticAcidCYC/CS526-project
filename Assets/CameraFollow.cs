using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class CameraFollow : MonoBehaviour {

    public GameObject player;
    Vector3 zero;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
        //transform.position = Vector3.MoveTowards(transform.position,player.transform.position - Vector3.forward*10,0.5f);
	}
    
}
