using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StrandRoot : MonoBehaviour {

	public GameObject SegmentPrefab;

	public int segmentCount;
	public float mass;
	public float drag;
	public float angularDrag;
	public float spring;
	public float damper;
	public float minDistance;
	public float segmentDistanceX;
	public float segmentDistanceY;
	public float segmentDistanceZ;

	private GameObject[] danglers;
	private GameObject[] segments;

	void Start () {
		// add rigidbody to root
		// and make it only affected by scripted animation (aka kinect bonesss)
		Rigidbody rBody = this.gameObject.AddComponent<Rigidbody>();
		rBody.isKinematic = true;

		// instantiate danglers for every segment,
		// and set up rigidbody
		danglers = new GameObject[segmentCount];
		for (int i = 0; i < segmentCount; i++) {
			GameObject dangler = new GameObject("Dangler " + (i+1));
			danglers[i] = dangler;
			// position danglers away from each other
			int multi = i+1;
			dangler.transform.position = (new Vector3(
				segmentDistanceX * multi,
				segmentDistanceY * multi,
				segmentDistanceZ * multi
			)) + transform.position;
			Rigidbody rick = dangler.AddComponent<Rigidbody>();
			rick.mass = mass;
			rick.drag = drag;
			rick.angularDrag = angularDrag;
		}

		// add spring joints for root and all but the last dangler
		// first link up root and 1st dangler
		SpringJoint rootSpring = this.gameObject.AddComponent<SpringJoint>();
		rootSpring.connectedBody = danglers[0].GetComponent<Rigidbody>();
		rootSpring.spring = spring;
		rootSpring.damper = 0.2f; // TODO fix magic number?
		rootSpring.minDistance = minDistance;
		// then link up the danglers
		for (int j = 0; j < segmentCount - 1; j++) {
			SpringJoint danglerSpring = danglers[j].AddComponent<SpringJoint>();
			danglerSpring.connectedBody = danglers[j+1].GetComponent<Rigidbody>();
			danglerSpring.spring = spring;
			danglerSpring.damper = damper;
			danglerSpring.minDistance = minDistance;
		}
		
		// instantiate the segments
		// and pass head & tail to StretchToFit component
		segments = new GameObject[segmentCount];
		for (int k = 0; k < segmentCount; k++) {
			GameObject segment = Instantiate(SegmentPrefab, Vector3.zero, Quaternion.identity);
			segments[k] = segment;
			StretchToFit stretcher = segment.GetComponent<StretchToFit>();
			// if it's the first one, set head to root's transform.
			// otherwise set it to previous dangler's
			stretcher.head = (k == 0 ? transform : danglers[k-1].transform);
			stretcher.tail = danglers[k].transform;
		}
	}
	
	void Update () {
		
	}
}
