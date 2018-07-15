using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StretchToFit : MonoBehaviour {

	public Transform head;
	public Transform tail;
	// Record intial scale
	private Vector3 scale0;

	// Use this for initialization
	void Start () {
		scale0 = transform.localScale;
	}
	
	// Update is called once per frame
	void Update () {
		Vector3 pH = head.position;
		Vector3 pT = tail.position;
		// position in middle of head and tail
		transform.position = (pH + pT) / 2;
		// rotate to tail direction
		transform.LookAt(pT);
		// stretch length
		Vector3 scale = scale0;
		scale.z = scale0.z * Vector3.Distance(pH, pT);
		transform.localScale = scale;
	}
}
