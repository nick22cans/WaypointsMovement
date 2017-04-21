using UnityEngine;
using System.Collections;

public class GlobalScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
		RVOUnityMgr.GetInstance ("").SetCheckMoveFn (CheckMoveOverride);
	}

	bool CheckMoveOverride(GameObject _who, Vector3 _from, ref Vector3 _to)
	{
		if (_who.GetComponent<MovementScript>() != null)
		{
			Vector3 c = _who.transform.position;//GetComponent<MovementScript> ().m_lookAheadPoint;
		
			float rvo_scale = _who.GetComponent<RVOUnity> ().m_radius;
			float radius = rvo_scale - Mathf.Max (_who.transform.localScale.x, _who.transform.localScale.z);
			radius = Mathf.Min (radius, GetDistanceBetweenPoints (_to, c));
			if (radius > 0)
			_to = _to + GetDirectionFromOnePointToAnother (_to, c) * radius;

			return true;
		}


		return false;
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
