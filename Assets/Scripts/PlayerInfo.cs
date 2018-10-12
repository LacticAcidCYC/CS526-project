using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Networking;
namespace Prototype.NetworkLobby{
	public class PlayerInfo : MonoBehaviour {
	//[Header("Network")]
	//[Space]
	//[SyncVar]
	public Color m_color;
	// Use this for initialization
	void Start () {
		//m_color = MyGameManager.Instance.GetLobbyPlayer(gameObject).playerColor;
		//GetComponentInChildren<Renderer>().material.color = m_color;
		GetComponentInChildren<Renderer>().material.color=MyGameManager.Instance.GetLobbyPlayer(gameObject).playerColor;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
}

