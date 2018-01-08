
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
		
		static public GameObject CreateBetweenPoints(this GameObject prefab, Vector3 start, Vector3 end, float width)
		{
			var offset = end - start;
			var scale = new Vector3(width, offset.magnitude / 2.0f, width);
			var position = start + (offset / 2.0f);

			var instance = GameObject.Instantiate(prefab, position, Quaternion.identity);
			instance.transform.up = offset;
			instance.transform.localScale = scale;

            var capsuleCollider = instance.GetComponent<CapsuleCollider>();
            capsuleCollider.radius = width;

            var height = Mathf.Max(Mathf.Abs(offset.x), Mathf.Abs(offset.y), Mathf.Abs(offset.z));
            capsuleCollider.height = height;

			return instance;
		}
		static public GameObject CreateAtPoint(this GameObject prefab, Vector3 position, float width)
		{
			// var offset = end - start;
			var scale = new Vector3(width, width, width);
			// var position = start + (offset / 2.0f);

			var instance = GameObject.Instantiate(prefab, position, Quaternion.identity);
			// instance.transform.up = offset;
			instance.transform.localScale = scale;

            var sphereCollider = instance.GetComponent<SphereCollider>();
            sphereCollider.radius = width * 2;
            
			return instance;
		}
        
    }
}