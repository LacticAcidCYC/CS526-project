using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Banana : NetworkBehaviour
{
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void OnTriggerEnter(Collider other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 slipDir = Vector3.zero;
            Vector3 relativePosition = transform.InverseTransformPoint(other.transform.position);
            Vector3 relativeDir = relativePosition.normalized;
            float forwardDot = Vector3.Dot(relativeDir, Vector3.forward);
            float rightDot = Vector3.Dot(relativeDir, Vector3.right);
            // we want to give the direction of the player, so it is the opposite of the relativeDir
            if(Mathf.Abs(forwardDot) > Mathf.Abs(rightDot)) {
                if(forwardDot > 0) {
                    slipDir = Vector3.back;
                }
                else{
                    slipDir = Vector3.forward;
                }
            }
            else{
                if(rightDot > 0) {
                    slipDir = Vector3.left;
                }
                else{
                    slipDir = Vector3.right;
                }
            }
            other.gameObject.GetComponent<Player>().onBanana(slipDir);
            Destroy(gameObject);
        }

    }
}
