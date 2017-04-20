using UnityEngine;
using System.Collections;

public class MovementScript : MonoBehaviour {
	
	public GameObject[] waypoints;
	//rough amount of movement per frame
	public float speed;
	//distance calculation error amplitude
	public float distance_sqrEpsilon;
	//direction difference error while turning
	public float direction_Epsilon;

	public float lookAheadDistance;


	private int m_currentWaypointIndex;
	private Vector3 m_currentWaypoint;
	private bool m_waypointIsStrict;

	private bool m_isMovingAlongTheRoute = false;
	private bool m_isTurning = false;
	private bool m_switchingToTheNextWaypoint = false;
	private bool m_thereIsAtLeastOneMoreWaypoint = false;

	private Vector3 m_intersectionPoint;

	private Vector3 m_direction;
	private Vector3 m_lookAheadPoint;

	private float m_previousFrameDistance;
	private float m_currentFrameDistance;

	float m_rotationStepsNumber = 100;
	Vector3 m_directionChangeStep; 

	//private float previousFrameDistance;

	// Use this for initialization
	void Start () {
		SetUpRoute ();
	}

	public void SetUpRoute()
	{
		if (waypoints.Length > 0)
		{
			m_currentWaypointIndex = -1;
			if (SwitchToTheNextWaypoint ())
			{
				m_isMovingAlongTheRoute = true;
			}
		}
	}

	bool SwitchToTheNextWaypoint()
	{
		m_currentWaypointIndex++;
		if (m_currentWaypointIndex == waypoints.Length)
			return false;
		if (m_currentWaypointIndex != waypoints.Length - 1)
			m_thereIsAtLeastOneMoreWaypoint = true;
		else
			m_thereIsAtLeastOneMoreWaypoint = false;

		m_currentWaypoint = waypoints [m_currentWaypointIndex].transform.position;
		m_waypointIsStrict = (waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().isStrict;

		if (!m_switchingToTheNextWaypoint || m_waypointIsStrict)
			m_direction = GetDirectionTowardsWaypoint ();
		if (m_switchingToTheNextWaypoint)
		{
			m_directionChangeStep = (GetDirectionTowardsPoint (m_currentWaypoint) - m_direction) / m_rotationStepsNumber;
		}

		m_isTurning = false;
		return true;
	}

	void UpdateLookAheadPoint()
	{


		if (m_isTurning && m_thereIsAtLeastOneMoreWaypoint)
		{
			if (GetTwoLinesIntersection (transform.position, PredictPointInCurrentDirection(lookAheadDistance), m_currentWaypoint, waypoints [m_currentWaypointIndex + 1].transform.position))
			{
				float distanceToTheIntersection = GetDistanceToThePoint (m_intersectionPoint);
				Vector3 crossWpDirection = GetDirectionFromOnePointToAnother (m_currentWaypoint, waypoints [m_currentWaypointIndex + 1].transform.position);
				float turnAmplitude = lookAheadDistance - distanceToTheIntersection;

				m_currentFrameDistance = turnAmplitude;
				print (turnAmplitude);
//				if (turnAmplitude < 1)
//					speed = speed;

				m_lookAheadPoint = m_currentWaypoint + crossWpDirection * turnAmplitude;
			}
		}
		else
			m_lookAheadPoint = PredictPointInCurrentDirection (lookAheadDistance);
	}

	bool LookAheadPointIsCloseToTheWaypoints()
	{
		return (GetSqrDistanceBetweenPoints (m_currentWaypoint, m_lookAheadPoint) < distance_sqrEpsilon);
	}

	bool CurrentWaypointIsBehind()
	{
//		if (m_isTurning && m_thereIsAtLeastOneMoreWaypoint)
//		{
//			if (GetSqrDistanceBetweenPoints (
//				GetDirectionFromOnePointToAnother(transform.position,waypoints[m_currentWaypointIndex+1].transform.position),
//					m_direction) < direction_sqrEpsilon)
//			{
//				m_isTurning = false;
//				return true;
//			}			
//		}

		if (m_currentFrameDistance < m_previousFrameDistance)
		{
			if (m_thereIsAtLeastOneMoreWaypoint)
			{
				m_switchingToTheNextWaypoint = true;
			}
			return true;
		}
		m_previousFrameDistance = m_currentFrameDistance;
		return false;
	}

	bool GetTwoLinesIntersection(Vector3 l1_s, Vector3 l1_e, Vector3 l2_s, Vector3 l2_e)
	{
		float a1 = l1_e.z - l1_s.z,
		b1 = l1_s.x - l1_e.x,
		c1 = a1 * l1_s.x + b1 * l1_s.z;

		float a2 = l2_e.z - l2_s.z,
		b2 = l2_s.x - l2_e.x,
		c2 = a2 * l2_s.x + b2 * l2_s.z;

		float delta = a1 * b2 - a2 * b1;

		if (delta == 0)
			return false;
		m_intersectionPoint = new Vector3 ((b2 * c1 - b1 * c2) / delta, (l1_e.y - l1_s.y)/2, (a1 * b2 - a2 * c1) / delta);
		return true;
	}
		
	void PerformStep()
	{
		UpdateLookAheadPoint ();

		if (m_switchingToTheNextWaypoint == true)
		{
			Vector3 wp_dir = GetDirectionTowardsWaypoint ();
			if (GetDistanceBetweenPoints (m_direction, wp_dir) < direction_Epsilon)
				m_switchingToTheNextWaypoint = false;
			else
				m_direction += m_directionChangeStep;
		} else
		{
			m_direction = GetDirectionTowardsPoint (m_lookAheadPoint);
		}
	


		transform.position += m_direction * speed;

		if (!m_isTurning)
			if (GetDistanceToThePoint (m_currentWaypoint) < lookAheadDistance)
			{
				m_isTurning = true;
				m_previousFrameDistance = 0;
			}
	}

	float GetDistanceBetweenPoints(Vector3 p1, Vector3 p2){
		return (p2 - p1).magnitude;
	}

	float GetSqrDistanceBetweenPoints(Vector3 p1, Vector3 p2){
		return (p2 - p1).sqrMagnitude;
	}

	float GetDistanceToThePoint(Vector3 point) {
		return ((point - transform.position).magnitude);
	}

	Vector3 GetDirectionFromOnePointToAnother(Vector3 from, Vector3 to)
	{
		Vector3 result = (to - from).normalized;
		result.y = 0;	
		return result;
	}

	Vector3 GetDirectionTowardsPoint(Vector3 point){
		return GetDirectionFromOnePointToAnother(transform.position, point);
	}

	Vector3 GetDirectionTowardsWaypoint(){
		return GetDirectionTowardsPoint(m_currentWaypoint);
	}

	Vector3 PredictPointInCurrentDirection(float distance){
		return(transform.position + m_direction * distance);
	}


	// Update is called once per frame
	void Update () 
	{
		if (m_isMovingAlongTheRoute) 
		{
			PerformStep ();
			if (m_isTurning && m_thereIsAtLeastOneMoreWaypoint) 
			{
				if (CurrentWaypointIsBehind ()) 
				{
					if (!SwitchToTheNextWaypoint ()) 
					{
						m_isMovingAlongTheRoute = false;
						transform.position = m_currentWaypoint;
					}
				}
			}
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (m_lookAheadPoint, 1f);
		Gizmos.DrawLine (m_lookAheadPoint, transform.position);

		Gizmos.DrawCube (PredictPointInCurrentDirection (lookAheadDistance), Vector3.one);
	}
}
