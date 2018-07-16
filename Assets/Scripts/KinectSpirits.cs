using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Kinect = Windows.Kinect;

public class KinectSpirits : MonoBehaviour 
{
    public GameObject BodySourceManager;

    public GameObject wingStrandTemplate;
    public GameObject bodyStrandTemplate;

    public float lerper;

    public int strandPerBone = 5;
    
    private Dictionary<ulong, GameObject> _Spirits = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;

    // HACK this data structure is good for single player but not two
    // TODO fix this with better data structure
    private List<Transform> _strands;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMapWing = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        // Left hand tip to Shoulder Spine
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        // { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        // Right hand tip to shoulder spine
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        // { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },
    };
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMapBody = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        // left to right shoulder
        { Kinect.JointType.ShoulderLeft, Kinect.JointType.ShoulderRight },
    };
    
	void Start() {
		if (BodySourceManager == null) return;
		_BodyManager = BodySourceManager.GetComponent<BodySourceManager>();

        _strands = new List<Transform>();
	}

    void Update () {
		if (BodySourceManager == null) return;
		if (_BodyManager == null) return;
		Kinect.Body[] data = _BodyManager.GetData();
		if (data == null) return;
		List<ulong> trackedIds = new List<ulong>();
		foreach(var body in data) {
			if (body == null) continue;
			if (!body.IsTracked) continue;
			trackedIds.Add(body.TrackingId);
		}
		List<ulong> knownIds = new List<ulong>(_Spirits.Keys);
        
		// NOTE cleaning up spirits that aren't tracked anymore
		foreach(ulong trackingId in knownIds){
			if (!trackedIds.Contains(trackingId)) {
                _strands.Clear();
				Destroy(_Spirits[trackingId]);
				_Spirits.Remove(trackingId);
			}
		}

        foreach(var body in data) {
            if (body == null) continue;
            if (!body.IsTracked) continue;
            
            if(!_Spirits.ContainsKey(body.TrackingId)) {
                // NOTE This would be where I create the Spirits
                _Spirits[body.TrackingId] = CreateSpiritObject(body.TrackingId);
            }

            RefreshSpiritObject(body, _Spirits[body.TrackingId]);
        }
    }
    
    // Initialize a spirit with strands and such
    private GameObject CreateSpiritObject(ulong id)
    {
        GameObject spirit = new GameObject("Spirit:" + id);
        
        // NOTE Strand initialization time!
        // Initialize different types of strand
        // and give strands reference to which joints to look for and how much
        foreach(KeyValuePair<Kinect.JointType, Kinect.JointType> bone in _BoneMapWing) {
            // many strands per bone!
            for (int i = 0; i < strandPerBone; i++) {
                GameObject strand = Instantiate(wingStrandTemplate);
                strand.name = bone.ToString();
                KinectStrandPosition positioner = strand.AddComponent<KinectStrandPosition>();
                positioner.head = bone.Key;
                positioner.tail = bone.Value;
                positioner.lerper = i * (1.0f /strandPerBone);
                strand.transform.parent = spirit.transform;
                _strands.Add(strand.transform);
            }
        }

        
        foreach(KeyValuePair<Kinect.JointType, Kinect.JointType> bone in _BoneMapBody) {
            GameObject strand = Instantiate(bodyStrandTemplate);
            strand.name = bone.ToString();
            strand.transform.parent = spirit.transform;
        }

        return spirit;
    }
    
    // NOTE this is the update function for Spirits.
    // Since each spirit will have its own update,
    // we only update what's necessary here - aka skeleton data
    // So the roots will need to be updated here with their joints
    // and distance
    private void RefreshSpiritObject(Kinect.Body body, GameObject bodyObject) {
        // update the body, which doesn't have multiple strands
        foreach(KeyValuePair<Kinect.JointType, Kinect.JointType> bone in _BoneMapBody) {
			Vector3 headJoint = GetVector3FromJoint(body.Joints[bone.Key]);
			Vector3 tailJoint = GetVector3FromJoint(body.Joints[bone.Value]);
			Transform strand = bodyObject.transform.Find(bone.ToString());
			// calculate position using the avg of two joints,
            // with a lerper to smoothen the movement
            strand.localPosition = Vector3.Lerp(strand.localPosition, (headJoint + tailJoint) * 0.5f, lerper);
            strand.localRotation.SetFromToRotation(headJoint, tailJoint);
		}

        // Multi-strand update!
        foreach (Transform strand in _strands)
        {
            KinectStrandPosition positioner = strand.gameObject.GetComponent<KinectStrandPosition>();
            Vector3 headJoint = GetVector3FromJoint(body.Joints[positioner.head]);
            Vector3 tailJoint = GetVector3FromJoint(body.Joints[positioner.tail]);
            Vector3 targetPosition = Vector3.Lerp(headJoint, tailJoint, positioner.lerper);
            strand.transform.localPosition = Vector3.Lerp(strand.localPosition, targetPosition, lerper);
            strand.transform.localRotation.SetFromToRotation(headJoint, tailJoint);
        }
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
