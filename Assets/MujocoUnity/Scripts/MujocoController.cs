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

		static public void ApplyAction(MujocoJoint mJoint, float? target = null)
        {
            HingeJoint hingeJoint = mJoint.Joint as HingeJoint;
            if (hingeJoint == null)
                return;
            if (hingeJoint.useSpring)
            {
                // HACK - make joint input range -1 to 1
                // var ctrlRangeMin = mJoint.CtrlRange.x;
                // var ctrlRangeMax = mJoint.CtrlRange.x;
                var ctrlRangeMin = 0f;//-1f;
                var ctrlRangeMax = 1f;
	            var inputScale = ctrlRangeMax - ctrlRangeMin;
                if (!target.HasValue) // handle random
                    target = ctrlRangeMin + (Random.value * inputScale);
    
                JointSpring js;
                js = hingeJoint.spring;
                var inputTarget = Mathf.Clamp(target.Value, ctrlRangeMin, ctrlRangeMax);
                if (ctrlRangeMin < 0)
                    inputTarget = Mathf.Abs(ctrlRangeMin) + inputTarget;
                else
                    inputTarget = inputTarget - Mathf.Abs(ctrlRangeMin);
                inputTarget /= inputScale;
                var min = hingeJoint.limits.min;
                var max = hingeJoint.limits.max;
                var outputScale = max-min;
                var outputTarget = min+(inputTarget * outputScale);
                js.targetPosition = outputTarget;
                hingeJoint.spring = js;
            }
            else if (hingeJoint.useMotor)
            {
                if (!target.HasValue) // handle random
                    target = Random.value * 2 - 1;

                // target = Mathf.Max(-1f, target.Value);
                // target = Mathf.Min(1f, target.Value);
                target = Mathf.Clamp(target.Value, 0f, 1f);
                target *= 2;
                target -= 1f;

                JointMotor jm;
                jm = hingeJoint.motor;
                jm.targetVelocity = target.Value * 200f;
                hingeJoint.motor = jm;
            }
        }
    }
}