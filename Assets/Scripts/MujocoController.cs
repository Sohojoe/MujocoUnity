using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MujocoUnity 
{
    public class MujocoController : MonoBehaviour
    {
        public bool applyRandomToAll;
        public bool applyTargets;
        public float[] targets;

        List<MujocoJoint> _mujocoJoints;

        public void SetMujocoJoints(List<MujocoJoint> mujocoJoints)
        {
            _mujocoJoints = mujocoJoints;
            targets = Enumerable.Repeat(0f, _mujocoJoints.Count).ToArray();
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