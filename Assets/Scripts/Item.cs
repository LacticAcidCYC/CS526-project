using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : MonoBehaviour {
    // 道具种类，1:加速 2：加炸弹数量 3:加炸弹威力
    public int type = 1;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    public void OnTriggerEnter(Collider other)
    {   

        if (other.gameObject.CompareTag("Player"))
        {   
            if (type == 1)
            {
                other.gameObject.GetComponent<Player>().speedUp();
            }
            else if (type == 2)
            {
                other.gameObject.GetComponent<Player>().addBombs();
            }
            else
            {
                other.gameObject.GetComponent<Player>().powerUp();
            }    
            Destroy(gameObject);
        }

    }
}
