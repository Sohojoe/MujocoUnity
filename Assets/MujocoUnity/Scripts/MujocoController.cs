using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MujocoUnity 
{
    public class MujocoController : MonoBehaviour
    {
        public GameObject CameraTarget;

        public bool applyRandomToAll;
        public bool applyTargets;
        public float[] targets;

        List<MujocoJoint> _mujocoJoints;

        public void SetMujocoJoints(List<MujocoJoint> mujocoJoints)
        {
            _mujocoJoints = mujocoJoints;
            targets = Enumerable.Repeat(0f, _mujocoJoints.Count).ToArray();
            if (CameraTarget != null && _mujocoJoints != null) {
                var target = FindTopMesh(_mujocoJoints.FirstOrDefault()?.Joint.gameObject, null);
                CameraTarget.GetComponent<SmoothFollow>().target = target.transform;
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
            if (_mujocoJoints == null || _mujocoJoints.Count ==0)
                return;
            for (int i = 0; i < _mujocoJoints.Count; i++)
            {
                if (applyRandomToAll)
                    ApplyTarget(_mujocoJoints[i], Random.value);
                else if (applyTargets)
                    ApplyTarget(_mujocoJoints[i], targets[i]);
            }
        }

        void ApplyTarget(MujocoJoint mJoint, float target)
        {
            HingeJoint hingeJoint = mJoint.Joint as HingeJoint;
            if (hingeJoint != null)
            {
                JointSpring js;
                js = hingeJoint.spring;
                var safeTarget = Mathf.Clamp(target, mJoint.CtrlRange.x, mJoint.CtrlRange.y);
                var min = hingeJoint.limits.min;
                var max = hingeJoint.limits.max;
                var scale = max-min;
                var scaledTarget = min+(safeTarget * scale);
                js.targetPosition = scaledTarget;
                hingeJoint.spring = js;
            }        
        }
    }
}