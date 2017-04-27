using UnityEngine;
using System.Collections;

public class GlobalScript : MonoBehaviour {

	public float m_dispThreshold;

	//private float m_radius;
	private float m_rvoDisposition;
	private float m_adjustmentDistance;
	private float m_impatience;
	private RVOUnity rvo;

	private Vector3 m_adjustmentDirection;
	private Vector3 m_desiredPosition;
	// Use this for initialization
	void Start () {
		RVOUnityMgr.GetInstance ("").SetCheckMoveFn (CheckMoveOverride);
		rvo = GetComponent<RVOUnity> ();
//		if (rvo)
//			//m_radius = GetComponent<RVOUnity> ().m_radius - transform.localScale.x;
//			m_impatience = GetComponent<RVOUnity>().m_impatience;
//		else
//			print ("Script RVOUnity is not attached");
	}
//
//	bool CheckMoveOverride(GameObject _who, Vector3 _from, ref Vector3 _to)
//	{
//		m_desiredPosition = _who.transform.position;
//		m_rvoDisposition = GetDistanceBetweenPoints (_to, m_desiredPosition);
//
//		m_adjustmentDistance = Mathf.Min (m_radius, m_rvoDisposition) * m_distanceFraction;
////		if (m_adjustmentDistance > m_adjustmentThreshold)
////			m_adjustmentDistance *= m_distanceFraction;
////		GetComponent<MovementScript> ().m_rvoFrom = _from;
////		GetComponent<MovementScript> ().m_rvoTo = _to;
////		GetComponent<MovementScript> ().m_rvoDesired = m_desiredPosition;
////		GetComponent<MovementScript> ().m_rvoCompromise = _to + m_adjustmentDistance * m_adjustmentDirection;;
//
//		//print (m_adjustmentDistance);
////		if (m_adjustmentDistance > m_distanceFraction)
////			return true;
//		// * m_distanceFraction;
//		//print (m_adjustmentDistance);
//		//m_adjustmentDistance = Mathf.Min (m_distanceFraction * m_adjustmentDistance, 0.1f * m_radius);
//
//		//m_adjustmentDistance *= 0.75f;
//
//		if (m_adjustmentDistance > 0)
//		{
//			m_adjustmentDirection = GetDirectionFromOnePointToAnother (_to, m_desiredPosition);	
//			_to += m_adjustmentDistance * m_adjustmentDirection;
//			//_to = m_desiredPosition;
//
//			return true;
//
//		}
//		print ("Radius < 0");
//		return false;
//	}


	bool CheckMoveOverride(GameObject _who, Vector3 _beforeRvo, ref Vector3 _afterRvo)
	{
		m_rvoDisposition = GetDistanceBetweenPoints (_beforeRvo, _afterRvo);
		m_impatience = rvo.m_impatience;
		if (m_rvoDisposition > m_dispThreshold)
			m_impatience += 0.01f;
		else
			m_impatience -= 0.01f;
		rvo.SetImpatience (m_impatience);
		print (m_impatience);
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
