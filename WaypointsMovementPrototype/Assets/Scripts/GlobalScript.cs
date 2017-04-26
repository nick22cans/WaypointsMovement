﻿using UnityEngine;
using System.Collections;

public class GlobalScript : MonoBehaviour {

	public bool m_printStatus;

	public float m_distanceFraction = 0.8f;
	private float m_radius;
	private float m_rvoDisposition;
	private float m_adjustmentDistance;

	private Vector3 m_adjustmentDirection;
	private Vector3 m_desiredPosition;
	// Use this for initialization
	void Start () {
		RVOUnityMgr.GetInstance ("").SetCheckMoveFn (CheckMoveOverride);
		if (GetComponent<RVOUnity> () != null)
			m_radius = GetComponent<RVOUnity> ().m_radius - transform.localScale.x;
		else
			print ("Script RVOUnity is not attached");
	}

	bool CheckMoveOverride(GameObject _who, Vector3 _from, ref Vector3 _to)
	{
		m_desiredPosition = _who.transform.position;
		m_rvoDisposition = GetDistanceBetweenPoints (_to, m_desiredPosition);
		m_adjustmentDistance = Mathf.Min (m_radius, m_rvoDisposition);

		//m_adjustmentDistance *= 0.75f;

		if (m_radius > 0)
		{
			m_adjustmentDirection = GetDirectionFromOnePointToAnother (_to, m_desiredPosition);	
			_to += m_adjustmentDistance * m_adjustmentDirection * m_distanceFraction;
			//_to = m_desiredPosition;
			if (m_printStatus)
				print(m_rvoDisposition);
		}
		else
		{
			print ("Radius < 0");
			return false;
		}

		return true;
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


	// Update is called once per frame
	void Update () {
	
	}
}
