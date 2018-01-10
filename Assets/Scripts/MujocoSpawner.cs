using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;
using UnityEngine;

namespace MujocoUnity
{
	public class MujocoSpawner : MonoBehaviour {

		public TextAsset MujocoXml;
		
		XElement _root;


		// Use this for initialization
		void Start () {
			LoadXml(MujocoXml.text);
			Parse();
		}

		// Update is called once per frame
		void Update () {
			
		}

		void LoadXml(string str)
        {
            var parser = new ParseMujoco();
            _root = XElement.Parse(str);
        }

		void Parse()
        {
			XElement element = _root;
            var name = element.Name.LocalName;
            print($"- Begin");

            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case "model":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
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
            ParseBody(element.Element("worldbody"), "worldbody", this.gameObject, null);
	        // <actuator>		<motor gear="100" joint="slider" name="slide"/>
        }
		void ParseBody(XElement xdoc, string bodyName, GameObject parentBody, GameObject parentGeom)
        {
            var geom = ParseGeom(xdoc, parentBody);
			Joint joint = null;
			joint = ParseJoint(xdoc, parentGeom);
            if (joint != null)
                joint.connectedBody = geom.GetComponent<Rigidbody>();

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
                            //print($"{name} {attribute.Name.LocalName}={attribute.Value}");
							body.name = attribute.Value;
                            break;
                        case "pos":
                            // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
							body.transform.localPosition = MujocoHelper.ParseVector3(attribute.Value);
                            break;
                        case "quat":
                            print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            break;
                        default:
                            Console.WriteLine($"*** MISSING --> {name}.{attribute.Name.LocalName}");
                            throw new NotImplementedException(attribute.Name.LocalName);
                            break;
                    }
                }
				ParseBody(element, element.Attribute("name")?.Value, body, geom ?? parentGeom);
            }
        }
		GameObject ParseGeom(XElement xdoc, GameObject parent)
        {
            var name = "geom";
			GameObject geom = null;
            
            var element = xdoc.Element(name);
            if (element == null)
                return null;

			var type = element.Attribute("type")?.Value;
			if (type == null) {
				print($"--- WARNING: ParseGeom: no type found in geom. Ignoring ({element.ToString()}");
				return geom;
			}
			float size;
			string geomName = element.Attribute("name")?.Value;
			switch (type)
			{
				case "capsule":
					size = float.Parse(element.Attribute("size")?.Value);
					var fromto = element.Attribute("fromto").Value;
					print($"ParseGeom: Creating type:{type} fromto:{fromto} size:{size}");
					geom = parent.CreateBetweenPoints(MujocoHelper.ParseFrom(fromto), MujocoHelper.ParseTo(fromto), size);
					geom.name = geomName;
					break;
				case "sphere":
					size = float.Parse(element.Attribute("size")?.Value);
					var pos = element.Attribute("pos").Value;
					print($"ParseGeom: Creating type:{type} pos:{pos} size:{size}");
					geom = parent.CreateAtPoint(MujocoHelper.ParseVector3(pos), size);
					geom.name = geomName;
					break;
				default:
					print($"--- WARNING: ParseGeom: {type} geom is not implemented. Ignoring ({element.ToString()}");
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
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "friction":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "rgba":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "name":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "pos":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "quat":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "size":
                        //print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "type":
                        //print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "fromto":
                        //print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "conaffinity":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "condim":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "density":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "material":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solimp":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solref":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "axisangle":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "user":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "margin":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    default: {
                        Console.WriteLine($"*** MISSING --> {name}.{attribute.Name.LocalName}");
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
		Joint ParseJoint(XElement xdoc, GameObject parentGeom)
		{
            var name = "joint";
			Joint joint= null;

            var element = xdoc.Element(name);
            if (element == null)
                return joint;

			var type = element.Attribute("type")?.Value;
			if (type == null) {
				print($"--- WARNING: ParseJoint: no type found. Ignoring ({element.ToString()}");
				return joint;
			}
			string jointName = element.Attribute("name")?.Value;
			switch (type)
			{
				case "hinge":
					print($"ParseJoint: Creating type:{type} ");
					parentGeom.gameObject.AddComponent<HingeJoint> ();
					joint = parentGeom.GetComponent<Joint>();
					joint.name = jointName;
					break;
				default:
					print($"--- WARNING: ParseJoint: joint type '{type}' is not implemented. Ignoring ({element.ToString()}");
					return joint;
			}
			HingeJoint hingeJoint = joint as HingeJoint;
			
            foreach (var attribute in element.Attributes())
            {
                switch (attribute.Name.LocalName)
                {
                    case "armature":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "damping":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "limited":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
						hingeJoint.useLimits = bool.Parse(attribute.Value);
                        break;
        			// <joint axis="1 0 0" limited="true" name="slider" pos="0 0 0" range="-1 1" type="slide"/>
                    case "axis":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
						joint.axis = MujocoHelper.ParseVector3(attribute.Value);
						// joint.axis = MujocoHelper.ParseVector3NoFlipYZ(attribute.Value);
                        break;
                    case "name":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "pos":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
						//joint.transform.localPosition += MujocoHelper.ParseVector3(attribute.Value);
						// joint.anchor = MujocoHelper.ParseVector3NoFlipYZ(attribute.Value);
                        break;
                    case "range":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
						var limits = hingeJoint.limits;
						limits.min = MujocoHelper.ParseGetMin(attribute.Value);
						limits.max = MujocoHelper.ParseGetMax(attribute.Value);
						hingeJoint.limits = limits;
						hingeJoint.useLimits = true;
                        break;
                    case "type":
                        // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solimplimit":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "solreflimit":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "stiffness":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    case "margin":
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                        break;
                    default: 
                        Console.WriteLine($"*** MISSING --> {name}.{attribute.Name.LocalName}");                    
                        throw new NotImplementedException(attribute.Name.LocalName);
                        break;
                }
            }	
			return joint;		
		}		
	}
}
