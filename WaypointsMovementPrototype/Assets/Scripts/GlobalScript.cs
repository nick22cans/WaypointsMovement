using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GlobalScript : MonoBehaviour {

	public float m_dispThreshold;

	//private float m_radius;
	private float m_rvoDisposition;
	private float m_adjustmentDistance;
	private float m_impatience;
	private RVOUnity rvo;

	private Vector3 m_adjustmentDirection;
	private Vector3 m_desiredPosition;

	private Dictionary<string,int> m_typesPopularity;
	// Use this for initialization
	void Start () {
		m_typesPopularity = new Dictionary<string, int> ();
		foreach (GameObject ch in m_characters)
		{
			if (ch.GetComponent<RVOUnity> ())
			{
				if (!m_typesPopularity.ContainsKey(ch.GetComponent<RVOUnity> ().m_impatienceMode.ToString()))
					m_typesPopularity.Add (ch.GetComponent<RVOUnity> ().m_impatienceMode.ToString (), 0);
				m_typesPopularity [ch.GetComponent<RVOUnity> ().m_impatienceMode.ToString()]++;
			}
		}

		foreach (System.Collections.Generic.KeyValuePair<string, int> p in m_typesPopularity)
		{
			print ("Impatience mode: " + p.Key + " (" + p.Value + " agents)");
		}

		//RVOUnityMgr.GetInstance ("").SetCheckMoveFn (CheckMoveOverride);
		//rvo = GetComponent<RVOUnity> ();
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


	public GameObject[] m_characters;
	private int m_currentLapsNumber = 1;

	// Update is called once per frame
	void Update () {
		bool all_passed = true;
		foreach (GameObject ch in m_characters)
		{

			if (ch.GetComponent<MovementScript> ())
			{
				if (ch.GetComponent<MovementScript> ().m_numberOfLapsComplete < m_currentLapsNumber)
					all_passed = false;
			}
		}
		if (all_passed)
		{
			print ("Lap: " + m_currentLapsNumber++ + " Time: " + Time.time);
		}
	}
}
