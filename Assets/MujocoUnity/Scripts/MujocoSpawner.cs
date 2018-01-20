using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace MujocoUnity
{
	public class MujocoSpawner : MonoBehaviour {

		public TextAsset MujocoXml;
        public Material Material;
        public PhysicMaterial PhysicMaterial;
        public LayerMask CollisionLayer; // used to disable colliding with self
        public bool DebugOutput;
        public string[] ListOf2dScripts = new string[] {"half_cheetah", "hopper", "walker2d"};
		
		XElement _root;
        float _damping = 1;

        bool _hasParsed;
        bool _useWorldSpace;
        public void SpawnFromXml()
        {
            if (_hasParsed)
                return;
			LoadXml(MujocoXml.text);
			Parse();
            _hasParsed = true;
        }


		// Use this for initialization
		void Start () {
            SpawnFromXml();
		}

		// Update is called once per frame
		void Update () {
			
		}

		void LoadXml(string str)
        {
            var parser = new ParseMujoco();
            _root = XElement.Parse(str);
        }

        void DebugPrint(string str)
        {
            if (DebugOutput)
                print(str);
        }

		void Parse()
        {
			XElement element = _root;
            var name = element.Name.LocalName;
            DebugPrint($"- Begin");

            ParseCompilerOptions(_root);

            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case "model":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
						this.gameObject.name = attribute.Value;
                        break;
                    default:
                        throw new NotImplementedException();
                }
                // result = result.Append(ParseBody(element, element.Attribute("name")?.Value));
            }

            //ParseBody(element.Element("default"), "default", this.gameObject);
        	// <compiler inertiafromgeom="true"/>
        	// <option gravity="0 0 -9.81" integrator="RK4" timestep="0.02"/>
            // <size nstack="3000"/>
            // // worldbody
            var joints = ParseBody(element.Element("worldbody"), "worldbody", this.gameObject, null);
	        // <actuator>		<motor gear="100" joint="slider" name="slide"/>
            var mujocoJoints = ParseGears(element.Element("actuator"), joints);
            
            GetComponent<MujocoController>().SetMujocoJoints(mujocoJoints);
            if (Material != null)
                foreach (var item in GetComponentsInChildren<Renderer>())
                {
                    item.material = Material;
                }
            if (PhysicMaterial != null)
                foreach (var item in GetComponentsInChildren<Collider>())
                {
                    item.material = PhysicMaterial;
                }
            var layer = (int) Mathf.Log(CollisionLayer.value, 2);
            if (CollisionLayer != null)
                foreach (var item in GetComponentsInChildren<Transform>())
                {
                    item.gameObject.layer = layer;
                }
            // 
            if (ListOf2dScripts.FirstOrDefault(x=>x == this.name) != null)
                foreach (var item in GetComponentsInChildren<Rigidbody>())
                {
                    item.constraints = item.constraints | RigidbodyConstraints.FreezeRotationY;
                }

        }

        void ParseCompilerOptions(XElement xdoc)
        {
            var name = "compiler";
            var elements = xdoc.Elements(name);
            foreach (var element in elements) {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "angle":
                            DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            break;
                        case "coordinate":
                            DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            if (attribute.Value.ToLower() == "global")
                                _useWorldSpace = true;
                            else if (attribute.Value.ToLower() == "local")
                                _useWorldSpace = false;
                            break;
                        case "inertiafromgeom":
                            DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            break;
                        default:
                            DebugPrint($"*** MISSING --> {name}.{attribute.Name.LocalName}");
                            throw new NotImplementedException(attribute.Name.LocalName);
                            break;
                    }
                }
            }
        }

		List<KeyValuePair<string, Joint>> ParseBody(XElement xdoc, string bodyName, GameObject parentBody, List<GameObject> parentGeoms, XElement parentXdoc = null)
        {
            var joints = new List<KeyValuePair<string, Joint>>();
            List<KeyValuePair<string, Joint>> newJoints = null;
            GameObject geom = null;
            List<GameObject> geoms = new List<GameObject>();
            foreach (var element in xdoc.Elements("geom"))
            {
                newJoints = new List<KeyValuePair<string, Joint>>();
                geom = ParseGeom(element, parentBody);
                if (geom != null)
                    geoms.Add(geom);
                if (xdoc.Element("joint") != null && parentGeoms != null && parentGeoms.Count > 0 && geom != null) {
                    foreach (var parentGeom in parentGeoms)
                        newJoints.AddRange(ParseJoint(xdoc, parentGeom, geom));
                }
            else if (parentXdoc?.Element("joint") != null && parentGeoms != null && parentGeoms.Count > 0 && geom != null)
                foreach (var parentGeom in parentGeoms) 
                    newJoints.AddRange(ParseJoint(parentXdoc, parentGeom, geom));
            if (newJoints.Count > 0) joints.AddRange(newJoints);
            }
            

            var name = "body";
            var elements = xdoc.Elements(name);
            foreach (var element in elements) {
				var body = new GameObject();
				body.transform.parent = parentBody.transform;
                
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "name":
                            //DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
							body.name = attribute.Value;
                            break;
                        case "pos":
                            // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            if (_useWorldSpace)
    							body.transform.position = MujocoHelper.ParseVector3(attribute.Value);
                            else
    							body.transform.localPosition = MujocoHelper.ParseVector3(attribute.Value);
                            break;
                        case "quat":
                            DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            break;
                        default:
                            DebugPrint($"*** MISSING --> {name}.{attribute.Name.LocalName}");
                            throw new NotImplementedException(attribute.Name.LocalName);
                            break;
                    }
                }
                var nextParentGeoms = geom != null ? new List<GameObject>{geom} : parentGeoms;
				newJoints = ParseBody(element, element.Attribute("name")?.Value, body, nextParentGeoms, xdoc);
                if (newJoints != null) joints.AddRange(newJoints);
            }
            return joints;
        }
		GameObject ParseGeom(XElement element, GameObject parent)
        {
            var name = "geom";
			GameObject geom = null;
            
            if (element == null)
                return null;

			var type = element.Attribute("type")?.Value;
			if (type == null) {
				DebugPrint($"--- WARNING: ParseGeom: no type found in geom. Ignoring ({element.ToString()}");
				return geom;
			}
			float size;
			string geomName = element.Attribute("name")?.Value;
			switch (type)
			{
				case "capsule":
                    if (element.Attribute("size")?.Value?.Split()?.Length > 1)
                        size = float.Parse(element.Attribute("size")?.Value.Split()[0]);
                    else
    					size = float.Parse(element.Attribute("size")?.Value);
					var fromto = element.Attribute("fromto").Value;
					DebugPrint($"ParseGeom: Creating type:{type} fromto:{fromto} size:{size}");
					geom = parent.CreateBetweenPoints(MujocoHelper.ParseFrom(fromto), MujocoHelper.ParseTo(fromto), size, _useWorldSpace);
					geom.name = geomName;
					break;
				case "sphere":
					size = float.Parse(element.Attribute("size")?.Value);
					var pos = element.Attribute("pos").Value;
					DebugPrint($"ParseGeom: Creating type:{type} pos:{pos} size:{size}");
					geom = parent.CreateAtPoint(MujocoHelper.ParseVector3(pos), size, _useWorldSpace);
					geom.name = geomName;
					break;
				default:
					DebugPrint($"--- WARNING: ParseGeom: {type} geom is not implemented. Ignoring ({element.ToString()}");
					return geom;
			}
			geom.AddRigidBody();
			//geom.transform.parent = parent.transform;			
			// TODO set collision based on size and fromto


            foreach (var attribute in element.Attributes())
            {
                // <geom name="rail" pos="0 0 0" quat="0.707 0 0.707 0"  size="0.02 1" type="capsule"/>
				// <geom fromto="0 0 0 0.001 0 0.6" name="cpole" rgba="0 0.7 0.7 1" size="0.049 0.3" type="capsule"/>
                switch (attribute.Name.LocalName)
                {
                    case "contype":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "friction":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "rgba":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "name":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "pos":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "quat":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "size":
                        //DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "type":
                        //DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "fromto":
                        //DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "conaffinity":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "condim":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "density":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "material":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solimp":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solref":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "axisangle":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "user":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "margin":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    default: {
                        DebugPrint($"*** MISSING --> {name}.{attribute.Name.LocalName}");
                        throw new NotImplementedException(attribute.Name.LocalName);
                        break;
                    }
                }
            }
			return geom;
        }
		Joint FixedJoint(GameObject parent)
		{
			parent.gameObject.AddComponent<FixedJoint> ();  
			var joint = parent.GetComponent<Joint>();
			return joint;
		}
        //GameObject parentGeom, GameObject parentBody)
		List<KeyValuePair<string, Joint>> ParseJoint(XElement xdoc, GameObject parentGeom, GameObject childGeom)
		{
            var name = "joint";
			var joints = new List<KeyValuePair<string, Joint>>();

            var element = xdoc.Element(name);
            if (element == null)
                return joints;

			var type = element.Attribute("type")?.Value;
			if (type == null) {
				DebugPrint($"--- WARNING: ParseJoint: no type found. Ignoring ({element.ToString()}");
				return joints;
			}
            Joint joint = null;
			string jointName = element.Attribute("name")?.Value;
			switch (type)
			{
				case "hinge":
					DebugPrint($"ParseJoint: Creating type:{type} ");
					parentGeom.gameObject.AddComponent<HingeJoint> ();
					joint = parentGeom.GetComponents<Joint>()?.ToList().LastOrDefault();
					//joint.name = jointName;
                    joint.connectedBody = childGeom.GetComponent<Rigidbody>();
					break;
				case "slide":
				case "free":
					DebugPrint($"ParseJoint: Creating type:{type} ");
					parentGeom.gameObject.AddComponent<FixedJoint> ();
					joint = parentGeom.GetComponents<Joint>()?.ToList().LastOrDefault();
					//joint.name = jointName;
                    joint.connectedBody = childGeom.GetComponent<Rigidbody>();
					break;
				default:
					DebugPrint($"--- WARNING: ParseJoint: joint type '{type}' is not implemented. Ignoring ({element.ToString()}");
					return joints;
			}
			HingeJoint hingeJoint = joint as HingeJoint;
            FixedJoint fixedJoint = joint as FixedJoint;
            if (hingeJoint != null){
                var sp = hingeJoint.spring;
                sp.damper = _damping;
                hingeJoint.spring = sp;                
            }
			
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case "armature":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "damping":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "limited":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
						if (hingeJoint != null) hingeJoint.useLimits = bool.Parse(attribute.Value);
                        break;
        			// <joint axis="1 0 0" limited="true" name="slider" pos="0 0 0" range="-1 1" type="slide"/>
                    case "axis":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
						joint.axis = MujocoHelper.ParseVector3(attribute.Value);
						// joint.axis = MujocoHelper.ParseVector3NoFlipYZ(attribute.Value);
                        break;
                    case "name":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "pos":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
						//joint.transform.localPosition += MujocoHelper.ParseVector3(attribute.Value);
						// joint.anchor = MujocoHelper.ParseVector3NoFlipYZ(attribute.Value);
                        break;
                    case "range":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
						var limits = hingeJoint.limits;
						limits.min = MujocoHelper.ParseGetMin(attribute.Value);
						limits.max = MujocoHelper.ParseGetMax(attribute.Value);
						hingeJoint.limits = limits;
						hingeJoint.useLimits = true;
                        break;
                    case "type":
                        // DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solimplimit":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solreflimit":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "stiffness":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "margin":
                        DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    default: 
                        DebugPrint($"*** MISSING --> {name}.{attribute.Name.LocalName}");                    
                        throw new NotImplementedException(attribute.Name.LocalName);
                        break;
                }
            }
            if (joint != null)
                joints.Add(new KeyValuePair<string,Joint>(jointName, joint));	
			return joints;
		}		
		List<MujocoJoint>  ParseGears(XElement xdoc, List<KeyValuePair<string, Joint>>  joints)
        {
            var mujocoJoints = new List<MujocoJoint>();
            var name = "motor";

            var elements = xdoc?.Elements(name);
            if (elements == null)
                return mujocoJoints;
            foreach (var element in elements)
            {
                mujocoJoints.AddRange(ParseGear(element, joints));
            }
            return mujocoJoints;
        }
		List<MujocoJoint> ParseGear(XElement element, List<KeyValuePair<string, Joint>>  joints)
        {
            var mujocoJoints = new List<MujocoJoint>();

			string jointName = element.Attribute("joint")?.Value;
			if (jointName == null) {
				DebugPrint($"--- WARNING: ParseGears: no jointName found. Ignoring ({element.ToString()}");
				return mujocoJoints;
			}
            var matches = joints.Where(x=>x.Key == jointName)?.Select(x=>x.Value);
            if(matches == null){
				DebugPrint($"--- ERROR: ParseGears: joint:'{jointName}' was not found in joints. Ignoring ({element.ToString()}");
				return mujocoJoints;                
            }

            foreach (Joint joint in matches)
            {
                HingeJoint hingeJoint = joint as HingeJoint;
                JointSpring spring = new JointSpring(); 
                if (hingeJoint != null) {
                    hingeJoint.useSpring = true;
                    spring = hingeJoint.spring;
                }
                var mujocoJoint = new MujocoJoint{
                    Joint = joint,
                    JointName = jointName,
                };
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "joint":
                            break;
                        case "ctrllimited":
                            var ctrlLimited = bool.Parse(attribute.Value);
                            mujocoJoint.CtrlLimited = ctrlLimited;
                            break;
                        case "ctrlrange":
                            var ctrlRange = MujocoHelper.ParseVector2(attribute.Value);
                            mujocoJoint.CtrlRange = ctrlRange;
                            break;
                        case "gear":
                            var gear = float.Parse(attribute.Value);
                            mujocoJoint.Gear = gear;
                            if (hingeJoint != null)
                                spring.spring = gear;
                            //DebugPrint($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            break;
                        case "name":
                            var objName = attribute.Value;
                            mujocoJoint.Name = objName;
                            break;
                        default: 
                            DebugPrint($"*** MISSING --> {name}.{attribute.Name.LocalName}");                    
                            throw new NotImplementedException(attribute.Name.LocalName);
                            break;
                    }
                }
                if (hingeJoint != null) {
                    hingeJoint.spring = spring;
                }
                mujocoJoints.Add(mujocoJoint);
            }
            return mujocoJoints;
        }
	}
}
