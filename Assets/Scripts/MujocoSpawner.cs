using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using xml;

public class MujocoSpawner : MonoBehaviour {

	public TextAsset MujocoXml;

	// Use this for initialization
	void Start () {
		ParseMujoco.FromString(MujocoXml.text);

	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
