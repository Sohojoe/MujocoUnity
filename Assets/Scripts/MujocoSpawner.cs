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
		public GameObject CapsulePrefab;
		public GameObject SpherePrefab;
		// public GameObject PlanePrefab;
		// public GameObject SlidePrefab;
		// public GameObject BoxPrefab;
		
		XElement _root;

		

		// Use this for initialization
		void Start () {
			LoadXml(MujocoXml.text);
			Parse();
			
			// // // <geom fromto="0.0 0.0 0.0 0.2 0.2 0.0" name="aux_1_geom" size="0.08" type="capsule"/>
			// var fromTo="0.0 0.0 0.0 0.2 0.2 0.0";
			// // var c0 = CreateCylinderBetweenPoints(ParseFrom(fromTo), ParseTo(fromTo), float.Parse("0.08"));
			// // c0.AddRigidBody();
			// // c0.transform.parent = this.transform;
			// // <geom fromto="0.0 0.0 0.0 -0.2 0.2 0.0" name="aux_2_geom" size="0.08" type="capsule"/>
			// fromTo="0.0 0.0 0.0 -0.2 0.2 0.0";
			// var c1 = CapsulePrefab.CreateBetweenPoints(MujocoHelper.ParseFrom(fromTo), MujocoHelper.ParseTo(fromTo), float.Parse("0.08"));
			// c1.AddRigidBody();
			// c1.transform.parent = this.transform;
			// // <geom fromto="0.0 0.0 0.0 -0.4 0.4 0.0" name="right_ankle_geom" size="0.08" type="capsule"/>
			// fromTo="0.0 0.0 0.0 -0.4 0.4 0.0";
			// var c2 = CapsulePrefab.CreateBetweenPoints(MujocoHelper.ParseFrom(fromTo), MujocoHelper.ParseTo(fromTo), float.Parse("0.08"));
			// c2.AddRigidBody();
			// c2.transform.parent = this.transform;
			// // <geom fromto="0.0 0.0 0.0 -0.4 -0.4 0.0" name="third_ankle_geom" size="0.08" type="capsule"/>
			// fromTo="0.0 0.0 0.0 -0.4 -0.4 0.0";
			// var c3 = CapsulePrefab.CreateBetweenPoints(MujocoHelper.ParseFrom(fromTo), MujocoHelper.ParseTo(fromTo), float.Parse("0.08"));
			// c3.AddRigidBody();
			// c3.transform.parent = this.transform;
			// // <geom fromto="0.0 0.0 0.0 0.4 -0.4 0.0" name="fourth_ankle_geom" size="0.08" type="capsule"/>
			// fromTo="0.0 0.0 0.0 0.4 -0.4 0.0";
			// var c4 = CapsulePrefab.CreateBetweenPoints(MujocoHelper.ParseFrom(fromTo), MujocoHelper.ParseTo(fromTo), float.Parse("0.08"));
			// c4.AddRigidBody();
			// c4.transform.parent = this.transform;

			// // var c0 = new ProceduralCapsule{

			// // };
			
			// // c0.transform.parent = this.transform;
			

			// // var c1 = GameObject.CreatePrimitive(PrimitiveType.Capsule);
			// // c1.AddRigidBody();
			// // c1.transform.parent = this.transform;
			// // c1.SetStartEnd(new Vector3(0f, 0f, 0f), new Vector3(0.2f, 0.08f, 0.2f));
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
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
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
            ParseBody(element.Element("worldbody"), "worldbody", this.gameObject);
	        // <actuator>		<motor gear="100" joint="slider" name="slide"/>
        }
		void ParseBody(XElement xdoc, string bodyName, GameObject parent)
        {
            // ParseJoint(xdoc);
            var geom = ParseGeom(xdoc, parent);
            // ParseMotor(xdoc);
            // ParseTendon(xdoc);
			// if (geom == null)
			// 	return;

            var name = "body";
            var element = xdoc.Element(name);
            if (element != null) {
                foreach (var attribute in element.Attributes())
                {
                    switch (attribute.Name.LocalName)
                    {
                        case "name":
                            print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            break;
                        case "pos":
                            // print($"{name} {attribute.Name.LocalName}={attribute.Value}");
							(geom ?? parent).transform.position = MujocoHelper.ParseVector3(attribute.Value);
                            break;
                        case "quat":
                            print($"{name} {attribute.Name.LocalName}={attribute.Value}");
                            break;
                        default:
                            Console.WriteLine($"*** MISSING --> {name}.{attribute.Name.LocalName}");
                            throw new NotImplementedException(attribute.Name.LocalName);
                            break;
                    }
                    ParseBody(element, element.Attribute("name")?.Value, geom ?? parent);
                }
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
			switch (type)
			{
				case "capsule":
					size = float.Parse(element.Attribute("size")?.Value);
					var fromto = element.Attribute("fromto").Value;
					print($"ParseGeom: Creating type:{type} fromto:{fromto} size:{size}");
					geom = CapsulePrefab.CreateBetweenPoints(MujocoHelper.ParseFrom(fromto), MujocoHelper.ParseTo(fromto), size);
					break;
				case "sphere":
					size = float.Parse(element.Attribute("size")?.Value);
					var pos = element.Attribute("pos").Value;
					print($"ParseGeom: Creating type:{type} pos:{pos} size:{size}");
					geom = SpherePrefab.CreateAtPoint(MujocoHelper.ParseVector3(pos), size);
					break;
				default:
					print($"--- WARNING: ParseGeom: {type} geom is not implemented. Ignoring ({element.ToString()}");
					return geom;
			}
			geom.AddRigidBody();
			geom.transform.parent = parent.transform;			
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
						geom.name = attribute.Value;
                        print($"{name} {attribute.Name.LocalName}={attribute.Value}");
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
	}
}
