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

        public List<float> qpos;
        public List<float> qvel;


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
            var qlen = MujocoJoints.Length + 3;
            qpos = Enumerable.Range(0,qlen).Select(x=>0f).ToList();
            qvel = Enumerable.Range(0,qlen).Select(x=>0f).ToList();
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
            UpdateQ();
        }
        void UpdateQ()
        {
            float lastQ;
            var topJoint = MujocoJoints[0];
            var topTransform = topJoint.Joint.transform.parent.transform;
            lastQ = qpos[0];
            qpos[0] = topTransform.position.x;
            qvel[0] = qpos[0]-lastQ;
            lastQ = qpos[1];
            qpos[1] = topTransform.position.y;
            qvel[1] = qpos[1]-lastQ;
            lastQ = qpos[2];
            qpos[2] = topTransform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            qvel[2] = qpos[2]-lastQ;
            for (int i = 0; i < MujocoJoints.Length; i++)
            {
                var hingeJoint = MujocoJoints[i].Joint as HingeJoint;
                var targ = hingeJoint.transform.parent.transform;
                float pos = 0f;
                if (hingeJoint.axis.x != 0f)
                    pos = targ.rotation.eulerAngles.x;
                else if (hingeJoint.axis.y != 0f)
                    pos = targ.rotation.eulerAngles.y;
                else if (hingeJoint.axis.z != 0f)
                    pos = targ.rotation.eulerAngles.z;
                if (hingeJoint){
                    qpos[3+i] = pos * Mathf.Deg2Rad;
                    qvel[3+i] = hingeJoint.velocity;
                }
            }
            
        }

		static public void ApplyAction(MujocoJoint mJoint, float? target = null)
        {
            HingeJoint hingeJoint = mJoint.Joint as HingeJoint;
            if (hingeJoint == null)
                return;
            if (hingeJoint.useSpring)
            {
                var ctrlRangeMin = -1f;
                var ctrlRangeMax = 1f;
                // var ctrlRangeMin = 0f;
                // var ctrlRangeMax = 1f;
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

                target = Mathf.Clamp(target.Value, -1f, 1f);
                // target = Mathf.Clamp(target.Value, 0f, 1f);
                // target *= 2;
                // target -= 1f;

                JointMotor jm;
                jm = hingeJoint.motor;
                jm.targetVelocity = target.Value * 200f;
                hingeJoint.motor = jm;
            }
        }
    }
}