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
    
    private Dictionary<ulong, GameObject> _Spirits = new Dictionary<ulong, GameObject>();
    private BodySourceManager _BodyManager;
    
    private Dictionary<Kinect.JointType, Kinect.JointType> _BoneMap = new Dictionary<Kinect.JointType, Kinect.JointType>()
    {
        // // Left foot to ass spine
        // { Kinect.JointType.FootLeft, Kinect.JointType.AnkleLeft },
        // { Kinect.JointType.AnkleLeft, Kinect.JointType.KneeLeft },
        // { Kinect.JointType.KneeLeft, Kinect.JointType.HipLeft },
        // { Kinect.JointType.HipLeft, Kinect.JointType.SpineBase },
        
        // // Right foot to ass spine
        // { Kinect.JointType.FootRight, Kinect.JointType.AnkleRight },
        // { Kinect.JointType.AnkleRight, Kinect.JointType.KneeRight },
        // { Kinect.JointType.KneeRight, Kinect.JointType.HipRight },
        // { Kinect.JointType.HipRight, Kinect.JointType.SpineBase },
        
        // Left hand tip to Shoulder Spine
        { Kinect.JointType.HandTipLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.ThumbLeft, Kinect.JointType.HandLeft },
        { Kinect.JointType.HandLeft, Kinect.JointType.WristLeft },
        { Kinect.JointType.WristLeft, Kinect.JointType.ElbowLeft },
        { Kinect.JointType.ElbowLeft, Kinect.JointType.ShoulderLeft },
        // { Kinect.JointType.ShoulderLeft, Kinect.JointType.SpineShoulder },
        
        // Right hand tip to shoulder spine
        { Kinect.JointType.HandTipRight, Kinect.JointType.HandRight },
        { Kinect.JointType.ThumbRight, Kinect.JointType.HandRight },
        { Kinect.JointType.HandRight, Kinect.JointType.WristRight },
        { Kinect.JointType.WristRight, Kinect.JointType.ElbowRight },
        { Kinect.JointType.ElbowRight, Kinect.JointType.ShoulderRight },
        // { Kinect.JointType.ShoulderRight, Kinect.JointType.SpineShoulder },

        // // Ass to Head
        // { Kinect.JointType.SpineBase, Kinect.JointType.SpineMid },
        // { Kinect.JointType.SpineMid, Kinect.JointType.SpineShoulder },
        // { Kinect.JointType.SpineShoulder, Kinect.JointType.Neck },
        // { Kinect.JointType.Neck, Kinect.JointType.Head },
    };
    
	void Start() {
		if (BodySourceManager == null) return;
		_BodyManager = BodySourceManager.GetComponent<BodySourceManager>();
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
        
		// NOTE cleaning up untracked spirits seem simple(?)
		foreach(ulong trackingId in knownIds){
			if (!trackedIds.Contains(trackingId)) {
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
        // need some changes to make the strand root locations more appealing
        foreach(KeyValuePair<Kinect.JointType, Kinect.JointType> bone in _BoneMap) {
            GameObject strand = Instantiate(wingStrandTemplate);
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
        foreach(KeyValuePair<Kinect.JointType, Kinect.JointType> bone in _BoneMap) {
			Vector3 sourceJoint = GetVector3FromJoint(body.Joints[bone.Key]);
			Vector3 targetJoint = GetVector3FromJoint(body.Joints[bone.Value]);
			Transform strand = bodyObject.transform.Find(bone.ToString());
			// calculate position using the avg of two joints,
            // with a lerper to smoothen the movement
            strand.localPosition = Vector3.Lerp(strand.localPosition, (sourceJoint + targetJoint) / 2.0f, lerper);
            strand.localRotation.SetFromToRotation(sourceJoint, targetJoint);
		}
    }
    
    private static Vector3 GetVector3FromJoint(Kinect.Joint joint)
    {
        return new Vector3(joint.Position.X * 10, joint.Position.Y * 10, joint.Position.Z * 10);
    }
}
