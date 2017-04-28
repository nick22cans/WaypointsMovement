using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GlobalScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}


	static Vector3 GetDirectionFromOnePointToAnother(Vector3 from, Vector3 to)
	{
		Vector3 result = (to - from).normalized;
		result.y = 0;	
		return result;
	}

	static float GetDistanceBetweenPoints(Vector3 p1, Vector3 p2){
		Vector2 diff = p2 - p1;
		diff.y = 0;
		return (p2 - p1).magnitude;
	}
		

	public static float GetAngleBetweenVectors(Vector3 first, Vector3 second)
	{
		float dot = Vector3.Dot (first, second) / (first.magnitude * second.magnitude);
		var acos = Mathf.Acos(dot);
		return(acos*180/Mathf.PI);
	}

	public static bool GetTwoLinesIntersection(Vector3 dir_line_s, Vector3 dir_line_e, Vector3 wp_line_s, Vector3 wp_line_e, ref Vector3 intersectionPoint)
	{

		float a1 = dir_line_e.z - dir_line_s.z,
		b1 = dir_line_s.x - dir_line_e.x,
		c1 = a1 * dir_line_s.x + b1 * dir_line_s.z;

		float a2 = wp_line_e.z - wp_line_s.z,
		b2 = wp_line_s.x - wp_line_e.x,
		c2 = a2 * wp_line_s.x + b2 * wp_line_s.z;

		float delta = a1 * b2 - a2 * b1;

		if (delta == 0)
		{
			print ("false");
			return false;
		}

		float x = (b2 * c1 - b1 * c2) / delta,
		y = (dir_line_e.y + dir_line_s.y) / 2,
		z = (a1 * c2 - a2 * c1) / delta;

		if (Mathf.Abs (x) < 0.0001)
			x += wp_line_s.x;
		if (Mathf.Abs (z) < 0.0001)
			z += wp_line_s.z;

		x = Mathf.Max (x, Mathf.Min (wp_line_s.x, wp_line_e.x));
		x = Mathf.Min (x, Mathf.Max (wp_line_s.x, wp_line_e.x));
		z = Mathf.Max (z, Mathf.Min (wp_line_s.z, wp_line_e.z));
		z = Mathf.Min (z, Mathf.Max (wp_line_s.z, wp_line_e.z));

		intersectionPoint = new Vector3 (x, y, z);
		return true;
	}
		
	public static Vector3 PredictPointInDirection(Vector3 from, Vector3 direction, float distance){
		return(from + direction * distance);
	}


	// Update is called once per frame
	void Update () {
	}
}
