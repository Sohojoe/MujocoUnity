
using System.Linq;
using UnityEngine;

namespace MujocoUnity
{
    public static class MujocoHelper
    {
        static readonly bool MujocoFlipYZ = true;
        static public void AddRigidBody(this GameObject onObj)
        {
            onObj.AddComponent<Rigidbody>();
            var rb = onObj.GetComponent<Rigidbody>();
            rb.useGravity = true;
        }
        // static public void SetStartEnd(this GameObject onObj, Vector3 start, Vector3 end)
        // {
        //     onObj.transform.localPosition = start;
        //     // onObj.transform.localScale = end;

        //     var size = onObj.GetComponent<Renderer> ().bounds.size;
        //     Vector3 rescale = onObj.transform.localScale;
        //     rescale.x = end.x * rescale.x / size.x;
        //     rescale.y = end.y * rescale.y / size.y;
        //     rescale.z = end.z * rescale.z / size.z;
        //     onObj.transform.localScale = rescale;
        // }	  
    
    	static char[] _delimiterChars = { ' ', ',', ':', '\t' };

        static public Vector3 ParseVector3NoFlipYZ(string str)
		{
			string[] words = str.Split(_delimiterChars);
			float x = float.Parse(words[0]);
			float y = float.Parse(words[1]);
			float z = float.Parse(words[2]);
			var vec3 = new Vector3(x,y,z);
			return vec3;
		}

		static public Vector3 ParseVector3(string str)
		{
			string[] words = str.Split(_delimiterChars);
			float x = float.Parse(words[0]);
			float y = float.Parse(words[1]);
			float z = float.Parse(words[2]);
			var vec3 = new Vector3(x,y,z);
            if (MujocoFlipYZ) {
    			vec3 = new Vector3(x,z,y);
            }
			return vec3;
		}
		static public Vector3 ParseFrom(string fromTo)
		{
			return ParseVector3(fromTo);
		}
		static public Vector3 ParseTo(string fromTo)
		{
			string[] words = fromTo.Split(_delimiterChars);
			float x = float.Parse(words[3]);
			float y = float.Parse(words[4]);
			float z = float.Parse(words[5]);
			var vec3 = new Vector3(x,y,z);
            if (MujocoFlipYZ) {
    			vec3 = new Vector3(x,z,y);
            }            
			return vec3;
		}

		static public Vector2 ParseVector2(string str)
		{
			string[] words = str.Split(_delimiterChars);
			float x = float.Parse(words[0]);
			float y = float.Parse(words[1]);
			var vec2 = new Vector2(x,y);
			return vec2;
		}

		static public float ParseGetMin(string rangeAsText)
		{
			string[] words = rangeAsText.Split(_delimiterChars);
            var range = words.Select(x=>float.Parse(x));
            return range.Min();
		}
		static public float ParseGetMax(string rangeAsText)
		{
			string[] words = rangeAsText.Split(_delimiterChars);
            var range = words.Select(x=>float.Parse(x));
            return range.Max();
		}        


		static public GameObject CreateBetweenPoints(this GameObject parent, Vector3 start, Vector3 end, float width)
		{

            // parent.AddComponent<MeshFilter>();
            // parent.AddComponent<CapsuleCollider>();
            // parent.GetComponent<CapsuleCollider>().height = 2f;
            // parent.AddComponent<MeshRenderer>();
            // parent.GetComponent<MeshFilter>().mesh = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Capsule);
            // var meshRenderer = parent.GetComponent <MeshRenderer>();
            // meshRenderer.materials[0] = PrimitiveHelper.GetDefaultMaterial();

			// var offset = end - start;
			// var scale = new Vector3(width, offset.magnitude / 2.0f, width);
			// var position = start + (offset / 2.0f);
			// parent.transform.up = offset;
			// parent.transform.localScale = scale;
            // parent.transform.localPosition = parent.transform.localPosition+position;
			// return parent;

			var offset = end - start;
			var scale = new Vector3(width*2, offset.magnitude / 2.0f, width*2);
			var position = start + (offset / 2.0f);
            var instance = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            instance.transform.parent = parent.transform;			
			instance.transform.up = offset;
			instance.transform.localScale = scale;
            instance.transform.localPosition = position;
			return instance;
		}
		static public GameObject CreateAtPoint(this GameObject parent, Vector3 position, float width)
		{
			var scale = new Vector3(width, width, width);

            // parent.AddComponent<MeshFilter>();
            // parent.AddComponent<SphereCollider>();
            // parent.AddComponent<MeshRenderer>();
            // parent.GetComponent<MeshFilter>().mesh = PrimitiveHelper.GetPrimitiveMesh(PrimitiveType.Sphere);
            // var meshRenderer = parent.GetComponent <MeshRenderer>();
            // meshRenderer.materials[0] = PrimitiveHelper.GetDefaultMaterial();
			// parent.transform.localScale = scale;
            // parent.transform.localPosition = parent.transform.localPosition+position;
            // return parent;

            var instance = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            instance.transform.parent = parent.transform;			
			instance.transform.localScale = scale*2;
            instance.transform.localPosition = position;
			return instance;
		}
        
    }
}