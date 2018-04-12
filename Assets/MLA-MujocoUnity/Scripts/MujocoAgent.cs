using System.Collections.Generic;
using System.Linq;
using MujocoUnity;
using UnityEngine;
using System;

namespace MlaMujocoUnity {
    public class MujocoAgent : Agent 
    {
        Rigidbody rBody;
		public TextAsset MujocoXml;
        public string ActorId;
		public float[] Low;
		public float[] High;
        public bool ShowMonitor;
		float[] _observation1D;
        float[] _internalLow;
        float[] _internalHigh;
        int _jointSize = 13; // 9+4
        int _numJoints = 3; // for debug object
        int _sensorOffset; // offset in observations to where senors begin
        int _numSensors;
        int _sensorSize; // number of floats per senor
        int _observationSize; // total number of floats
        MujocoController _mujocoController;
        bool _footHitTerrain;
        bool _nonFootHitTerrain;
        List<float> _actions;
        Func<bool> _terminate;
        Func<float> _stepReward;

        int _frameSkip = 4; // number of physics frames to skip between training
        int _nSteps = 1000; // total number of training steps

        void Start () {
            rBody = GetComponent<Rigidbody>();
        }
        public void SetupMujoco()
        {
            _mujocoController = GetComponent<MujocoController>();
            _numJoints = _mujocoController.qpos.Count;
            _numSensors = _mujocoController.MujocoSensors.Count;            
            _jointSize = 2;
            _sensorSize = 1;
            _sensorOffset = _jointSize * _numJoints;
            _observationSize = _sensorOffset + (_sensorSize * _numSensors);
            _observation1D = Enumerable.Repeat<float>(0f, _observationSize).ToArray();
            Low = _internalLow = Enumerable.Repeat<float>(float.MinValue, _observationSize).ToArray();
            High = _internalHigh = Enumerable.Repeat<float>(float.MaxValue, _observationSize).ToArray();
            for (int j = 0; j < _numJoints; j++)
            {
                var offset = j * _jointSize;
                _internalLow[offset+0] = -5;//-10;
                _internalHigh[offset+0] = 5;//10;
                _internalLow[offset+1] = -5;//-500;
                _internalHigh[offset+1] = 5;//500;
                // _internalLow[offset+2] = -5;//-500;
                // _internalHigh[offset+3] = 5;//500;
            }
            for (int j = 0; j < _numSensors; j++)
            {
                var offset = _sensorOffset + (j * _sensorSize);
                _internalLow[offset+0] = -1;//-10;
                _internalHigh[offset+0] = 1;//10;
            }    
            this.brain = GameObject.Find("MujocoBrain").GetComponent<Brain>();
        }

        public int GetObservationCount()
        {
            return _observation1D.Length;
        }
        public int GetActionsCount()
        {
            return _mujocoController.MujocoJoints.Count;
        }

        public override void InitializeAgent()
        {
            agentParameters = new AgentParameters();
            agentParameters.resetOnDone = true;
            agentParameters.numberOfActionsBetweenDecisions = _frameSkip;
            agentParameters.maxStep = _nSteps * _frameSkip;
        }
     
        public override void AgentReset()
        {
            Monitor.SetActive(true);
            _mujocoController = GetComponent<MujocoController>();
            _mujocoController.MujocoJoints = null;
            _mujocoController.MujocoSensors = null;
            // var joints = this.GetComponentsInChildren<Joint>().ToList();
            // foreach (var item in joints)
            //     Destroy(item.gameObject);
            var rbs = this.GetComponentsInChildren<Rigidbody>().ToList();
            foreach (var item in rbs)
                DestroyImmediate(item.gameObject);
            Resources.UnloadUnusedAssets();

            var mujocoSpawner = this.GetComponent<MujocoUnity.MujocoSpawner>();
            if (mujocoSpawner != null)
                mujocoSpawner.MujocoXml = MujocoXml;
            mujocoSpawner.SpawnFromXml();
            SetupMujoco();
            _mujocoController.UpdateFromExternalComponent();
            switch(ActorId)
            {
                case "a_oai_walker2d-v0":
                    _stepReward = StepReward_OaiWalker;
                    _terminate = Terminate_OnNonFootHitTerrain;
                    break;
                case "a_dm_walker-v0":
                    _stepReward = StepReward_DmWalker;
                    _terminate = Terminate_OnNonFootHitTerrain;
                    break;
                case "a_oai_hopper-v0":
                    _stepReward = StepReward_OaiHopper;
                    _terminate = Terminate_HopperOai;
                    break;
                case "a_oai_humanoid-v0":
                    _stepReward = StepReward_OaiHumanoid;
                    _terminate = Terminate_OnNonFootHitTerrain;
                    break;
                case "a_ant-v0":
                case "a_oai_half_cheetah-v0":
                default:
                    throw new NotImplementedException();
            }
        }
			// AddRewardFunc("a_oai_half_cheetah-v0", StepReward_VelocityIfNewMaxPosX, TerminalReward_NegNonFoot);
			// AddRewardFunc("a_oai_hopper-v0", StepReward_OaiHopper, 		TerminalReward_None);
			// AddRewardFunc("a_oai_humanoid-v0", StepReward_VelocityIfNewMaxPosX, 	TerminalReward_NegNonFoot);
			// AddRewardFunc("a_oai_humanoid-v01", StepReward_VelocityNegNonFoot, 	TerminalReward_None);
			// AddRewardFunc("a_oai_humanoid-v02", StepReward_StandNegNonFoot, 	TerminalReward_None);
			// AddRewardFunc("a_oai_walker2d-v0", StepReward_OaiWalker, 	TerminalReward_NegNonFoot);
			// AddRewardFunc("a_dm_walker-v0", StepReward_DmWalker, 	TerminalReward_None);
			// AddRewardFunc("a_RagdollSnake-v0", StepReward_VelocityIfNewMaxPosX, 	TerminalReward_None);
			// AddRewardFunc("a_RagdollSnake-v01", StepReward_NewMaxPosX, 	TerminalReward_None);
			// AddRewardFunc("a_ant-v0", StepReward_VelocityIfNewMaxPosX, 	TerminalReward_None);
			// AddRewardFunc("a_ant-v01", StepReward_NewMaxPosX, 	TerminalReward_None);
            // AddRewardFunc("a_oai_half_cheetah-v0",  Terminate_OnFall);
			// AddRewardFunc("a_oai_hopper-v0",        Terminate_HopperOai);//Terminate_OnNonFootHitTerrain);
			// AddRewardFunc("a_oai_humanoid-v0",      Terminate_OnNonFootHitTerrain);
			// AddRewardFunc("a_oai_humanoid-v01",     Terminate_OnTenErrors);
			// AddRewardFunc("a_oai_humanoid-v02",     Terminate_OnNonFootHitTerrain);
			// AddRewardFunc("a_oai_walker2d-v0",      Terminate_OnNonFootHitTerrain);
			// AddRewardFunc("a_dm_walker-v0",      	Terminate_OnNonFootHitTerrain);
			// AddRewardFunc("a_RagdollSnake-v0",      Terminate_Never);
			// AddRewardFunc("a_RagdollSnake-v01",     Terminate_Never);
			// AddRewardFunc("a_ant-v0",               Terminate_Never);
			// AddRewardFunc("a_ant-v01",              Terminate_Never);
        public override void CollectObservations()
        {
            _mujocoController.UpdateQFromExternalComponent();
            var joints = _mujocoController.MujocoJoints;

            if (ShowMonitor) {
                // Monitor.Log("pos", _mujocoController.qpos, MonitorType.hist);
                // Monitor.Log("vel", _mujocoController.qvel, MonitorType.hist);
                // Monitor.Log("onSensor", _mujocoController.OnSensor, MonitorType.hist);
                // Monitor.Log("sensor", _mujocoController.SensorIsInTouch, MonitorType.hist);
            }
           for (int j = 0; j < _numJoints; j++)
            {
                var offset = j * _jointSize;
                _observation1D[offset+00] = _mujocoController.qpos[j];
                _observation1D[offset+01] = _mujocoController.qvel[j];
                // _observation1D[offset+02] = _mujocoController.qglobpos[j];
            }
            for (int j = 0; j < _numSensors; j++)
            {
                var offset = _sensorOffset + (j * _sensorSize);
                // _observation1D[offset+00] = _mujocoController.OnSensor[j];
                _observation1D[offset+00] = _mujocoController.SensorIsInTouch[j]; // try this when using nstack
                // _observation1D[offset+01] = _mujocoController.MujocoSensors[j].SiteObject.transform.position.x;
                // _observation1D[offset+02] = _mujocoController.MujocoSensors[j].SiteObject.transform.position.y;
                // _observation1D[offset+03] = _mujocoController.MujocoSensors[j].SiteObject.transform.position.z;
            }
            _observation1D = _observation1D
                .Select(x=> UnityEngine.Mathf.Clamp(x,-5, 5))
                // .Select(x=> x / 5f)
                .ToArray();
            AddVectorObs(_observation1D);
        }

        public override void AgentAction(float[] vectorAction, string textAction)
        {
            _actions = vectorAction
                .Select(x=>Mathf.Clamp(x, -1, 1f))
                .ToList();
            // if (ShowMonitor)
            //     Monitor.Log("actions", _actions, MonitorType.hist);
            for (int i = 0; i < _mujocoController.MujocoJoints.Count; i++) {
				var inp = (float)_actions[i];
				MujocoController.ApplyAction(_mujocoController.MujocoJoints[i], inp);
			}
            _mujocoController.UpdateFromExternalComponent();
            
            var done = _terminate();//Terminate_HopperOai();

            if (done)
            {
                Done();
                var reward = 0;
                // var reward = -1000f;
                SetReward(reward);
            }
            if (!IsDone())
            {
                var reward = _stepReward();//StepReward_OaiHopper();
                SetReward(reward);
            }
            _footHitTerrain = false;
            _nonFootHitTerrain = false;
        }  

		float StepReward_OaiWalker()
		{
			return StepReward_DmWalker();
		}
        float GetHeight()
        {
			var feetYpos = _mujocoController.MujocoJoints
				.Where(x=>x.JointName.ToLowerInvariant().Contains("foot"))
				.Select(x=>x.Joint.transform.position.y)
				.OrderBy(x=>x)
				.ToList();
            float lowestFoot = 0f;
            if(feetYpos!=null && feetYpos.Count != 0)
                lowestFoot = feetYpos[0];
			var height = _mujocoController.qpos[1]-lowestFoot;
            return height;
        }
        float GetVelocity()
        {
			var dt = Time.fixedDeltaTime;
			var rawVelocity = _mujocoController.qvel[0];
			var velocity = 10f * rawVelocity * dt * _frameSkip;
            // if (ShowMonitor)
                // Monitor.Log("velocity", velocity, MonitorType.text);
            return velocity;
        }
        float GetUprightBonus()
        {
			var uprightBonus = 0.5f * (2 - (Mathf.Abs(_mujocoController.qpos[2])*2)-1);
            // if (ShowMonitor)
                // Monitor.Log("uprightBonus", uprightBonus, MonitorType.text);
            return uprightBonus;
        }
        float GetHeightPenality(float maxHeight)
        {
            var height = GetHeight();
            var heightPenality = maxHeight - height;
			heightPenality = Mathf.Clamp(heightPenality, 0f, maxHeight);
            if (ShowMonitor) {
                Monitor.Log("height", height, MonitorType.text);
                Monitor.Log("heightPenality", heightPenality, MonitorType.text);
            }
            return heightPenality;
        }
        float GetEffort()
        {
			var effort = _actions
				.Select(x=>Mathf.Pow(Mathf.Abs(x),2))
				.Sum();
            // if (ShowMonitor)
                // Monitor.Log("effort", effort, MonitorType.text);
            return effort;
        }
		float StepReward_DmWalker()
		{
            // float heightPenality = GetHeightPenality(1f);
            float heightPenality = GetHeightPenality(.65f);
            float uprightBonus = GetUprightBonus();
            float velocity = GetVelocity();
            float effort = GetEffort();
            // var effortPenality = 1e-3f * (float)effort;
            var effortPenality = 1e-1f * (float)effort;

			var reward = velocity
                +uprightBonus
                -heightPenality
                -effortPenality;
            if (ShowMonitor) {
                var hist = new []{reward,velocity,uprightBonus,-heightPenality,-effortPenality}.ToList();
                Monitor.Log("rewardHist", hist, MonitorType.hist);
                // Monitor.Log("effortPenality", effortPenality, MonitorType.text);
                // Monitor.Log("reward", reward, MonitorType.text);
            }

			return reward;
		}
        float StepReward_OaiHumanoid()
        {
            float heightPenality = GetHeightPenality(.65f);
            float uprightBonus = GetUprightBonus();
            float velocity = GetVelocity();
            float effort = GetEffort();
			var alive_bonus = 0.5f;
            // var effortPenality = 1e-3f * (float)effort;
            var effortPenality = 1e-1f * (float)effort;
			var reward = velocity 
                + alive_bonus
			    - effortPenality;
            if (ShowMonitor) {
                var hist = new []{reward,velocity,alive_bonus,-effortPenality}.ToList();
                Monitor.Log("rewardHist", hist, MonitorType.hist);
                // Monitor.Log("effortPenality", effortPenality, MonitorType.text);
            }
			return reward;            
        }
        float StepReward_OaiHopper()
		{
            float heightPenality = GetHeightPenality(1.0f);
            float uprightBonus = GetUprightBonus();
            float velocity = GetVelocity();
            float effort = GetEffort();
			var alive_bonus = 0.5f;
            var effortPenality = 1e-3f * (float)effort;
            // var effortPenality = 1e-1f * (float)effort;
			var reward = velocity 
                + alive_bonus
			    - effortPenality;
            if (ShowMonitor) {
                var hist = new []{reward,velocity,alive_bonus,-effortPenality}.ToList();
                Monitor.Log("rewardHist", hist, MonitorType.hist);
                // Monitor.Log("effortPenality", effortPenality, MonitorType.text);
            }
			return reward;
		}      
		bool Terminate_Never()
		{
			return false;
		}
        bool Terminate_OnNonFootHitTerrain()
		{
			return _nonFootHitTerrain;
		}
        bool Terminate_HopperOai()
		{
			if (_nonFootHitTerrain)
				return true;
			if (_mujocoController.qpos == null)
				return false;
			var height = _mujocoController.qpos[1];
			var angle = Mathf.Abs(_mujocoController.qpos[2]);
			bool endOnHeight = (height < .3f);
			bool endOnAngle = (angle > (1f/180f) * (5.7296f *6));
			return endOnHeight || endOnAngle;
		}

		public void OnTerrainCollision(GameObject other, GameObject terrain) {
            if (string.Compare(terrain.name, "Terrain", true) != 0)
                return;
            
            switch (other.name.ToLowerInvariant().Trim())
            {
                case "left_foot": // oai_humanoid
                case "right_foot": // oai_humanoid
                case "right_shin1": // oai_humanoid
                case "left_shin1": // oai_humanoid
                case "foot_geom": // oai_hopper  //oai_walker2d
                case "leg_geom": // oai_hopper //oai_walker2d
                case "leg_left_geom": // oai_walker2d
                case "foot_left_geom": //oai_walker2d
                case "foot_left_joint": //oai_walker2d
                case "foot_joint": //oai_walker2d
                    _footHitTerrain = true;
                    break;
                default:
                    _nonFootHitTerrain = true;
                    break;
            }
		}            
    }
}