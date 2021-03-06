﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GlobalScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	}


	public static Vector3 GetDirection(Vector3 from, Vector3 to)
	{
		Vector3 result = (to - from).normalized;
		result.y = 0;	
		return result;
	}

	public static float GetDistance(Vector3 p1, Vector3 p2){
		Vector3 diff = p2 - p1;
		diff.y = 0;
		if (float.IsNaN (diff.magnitude))
			print ("NAN");
		return diff.magnitude;
	}
		

	public static float GetAngle(Vector3 first, Vector3 second)
	{
		float dot = Vector3.Dot (first, second) / (first.magnitude * second.magnitude);
		float acos = Mathf.Acos(dot);
		float res = acos * 180 / Mathf.PI;
		if (float.IsNaN (res))
			return 0;
		return res;
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
			return false;
		}

		float x = (b2 * c1 - b1 * c2) / delta,
		y = (dir_line_e.y + dir_line_s.y) / 2,
		z = (a1 * c2 - a2 * c1) / delta;

		if (Mathf.Abs (x) < 0.0001)
			x += wp_line_s.x;
		if (Mathf.Abs (z) < 0.0001)
			z += wp_line_s.z;

		x = Mathf.Clamp (x,Mathf.Min (wp_line_s.x, wp_line_e.x), Mathf.Max (wp_line_s.x, wp_line_e.x));
		z = Mathf.Clamp (z, Mathf.Min (wp_line_s.z, wp_line_e.z), Mathf.Max (wp_line_s.z, wp_line_e.z));

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
