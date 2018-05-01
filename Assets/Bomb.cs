using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bomb : MonoBehaviour {

	// Use this for initialization
	public float radius = 5.0F;
    public float power = 10.0F;
    public float delay = 1.0F;
    public float repeate = 0F;


	void Update()
	{
		if (delay == 0f)
			return;
		delay -= Time.deltaTime;
		delay = Mathf.Max(0f, delay);
		if (delay == 0f){
			Explode();
			delay = repeate;
		}
		
	}

    void Explode()
    {
        Vector3 explosionPos = transform.position;
        Collider[] colliders = Physics.OverlapSphere(explosionPos, radius);
        foreach (Collider hit in colliders)
        {
            Rigidbody rb = hit.GetComponent<Rigidbody>();

            if (rb != null)
                rb.AddExplosionForce(power, explosionPos, radius, 3.0F);
        }
    }
}
