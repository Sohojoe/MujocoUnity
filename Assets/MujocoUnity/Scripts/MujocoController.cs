using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MujocoUnity 
{
    public class MujocoController : MonoBehaviour
    {
        public MujocoJoint[] MujocoJoints;
        
        public GameObject CameraTarget;

        public bool applyRandomToAll;
        public bool applyTargets;
        public float[] targets;


        public void SetMujocoJoints(List<MujocoJoint> mujocoJoints)
        {
            MujocoJoints = mujocoJoints.ToArray();
            targets = Enumerable.Repeat(0f, MujocoJoints.Length).ToArray();
            if (CameraTarget != null && MujocoJoints != null) {
                var target = FindTopMesh(MujocoJoints.FirstOrDefault()?.Joint.gameObject, null);
                var smoothFollow = CameraTarget.GetComponent<SmoothFollow>();
                if (smoothFollow != null) 
                    smoothFollow.target = target.transform;
            }
        }
        GameObject FindTopMesh(GameObject curNode, GameObject topmostNode = null)
        {
            var meshRenderer = curNode.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                topmostNode = meshRenderer.gameObject;
            var root = curNode.transform.root.gameObject;
            var meshRenderers = root.GetComponentsInChildren<MeshRenderer>();
            if (meshRenderers != null && meshRenderers.Length >0)
                topmostNode = meshRenderers[0].gameObject;
            
            // var parent = curNode.transform.parent;//curNode.GetComponentInParent<Transform>()?.gameObject;
            // if (parent != null)
            //     return FindTopMesh(curNode, topmostNode);
            return (topmostNode);
            
        }
        void Start () {
            
        }
        
        // Update is called once per frame
        void Update () 
        {
            if (MujocoJoints == null || MujocoJoints.Length ==0)
                return;
            for (int i = 0; i < MujocoJoints.Length; i++)
            {
                if (applyRandomToAll)
                    ApplyAction(MujocoJoints[i]);
                else if (applyTargets)
                    ApplyAction(MujocoJoints[i], targets[i]);
            }
        }

		void ApplyAction(MujocoJoint mJoint, float? target = null)
        {
            HingeJoint hingeJoint = mJoint.Joint as HingeJoint;
            if (hingeJoint != null)
            {
	            var inputScale = mJoint.CtrlRange.y - mJoint.CtrlRange.x;
                if (!target.HasValue) // handle random
                    target = Random.value * inputScale + mJoint.CtrlRange.x;

                JointSpring js;
                js = hingeJoint.spring;
                var inputTarget = Mathf.Clamp(target.Value, mJoint.CtrlRange.x, mJoint.CtrlRange.y);
                if (mJoint.CtrlRange.x < 0)
                    inputTarget = Mathf.Abs(mJoint.CtrlRange.x) + inputTarget;
                else
                    inputTarget = inputTarget - Mathf.Abs(mJoint.CtrlRange.x);
                inputTarget /= inputScale;
                var min = hingeJoint.limits.min;
                var max = hingeJoint.limits.max;
                var outputScale = max-min;
                var outputTarget = min+(inputTarget * outputScale);
                js.targetPosition = outputTarget;
                hingeJoint.spring = js;
            }        
        }
    }
}