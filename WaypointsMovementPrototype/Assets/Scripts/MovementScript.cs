using UnityEngine;
using System.Collections;

public class MovementScript : MonoBehaviour {
	
	public GameObject[] waypoints;
	//rough amount of movement per frame
	public float speed;
	//distance calculation error amplitude
	public float distance_epsilon;
	//amount of lookup range required to switch to the next wp 
	public float waypointSwitchModifier;

	public float lookAheadDistance;

	#region waypoints
	private int m_currentWaypointIndex;
	//private float m_sqrCurrentWpReachingDistance;
	private Vector3 m_currentWaypoint;
	private bool m_waypointIsStrict;
	#endregion

	private bool m_isMovingAlongTheRoute = false;

	#region direction interpolation
	private bool m_isInterpolatingDirection;
	private Vector3 m_interpolationStep;
	//not really needed now -> prototype for dynamic update
	private Vector3 m_fromDirection;
	private Vector3 m_toDirection;
	private Vector3 m_currentDirection;
	private Vector3 m_lookAheadPoint;
	#endregion

	private float previousFrameDistance;

	// Use this for initialization
	void Start () {
		SetUpRoute ();
	}

	public void SetUpRoute()
	{
		if (waypoints.Length > 0) {
			m_currentWaypointIndex = -1;
			if (PickNewWaypoint()) {
				m_isMovingAlongTheRoute = true;
			}
		}
	}

	bool PickNewWaypoint()
	{
		m_currentWaypointIndex++;
		if (m_currentWaypointIndex == waypoints.Length)
			return false;
		
		m_currentWaypoint = waypoints [m_currentWaypointIndex].transform.position;
		m_waypointIsStrict = (waypoints [m_currentWaypointIndex]).GetComponent<WaypointScript> ().isStrict;
		previousFrameDistance = GetDistanceToThePoint (m_currentWaypoint);
		m_lookAheadPoint = m_currentWaypoint + m_currentDirection* lookAheadDistance;
		//m_currentDirection = GetDirectionTowardsWaypoint ();
		return true;
	}

	bool IsWaypointReached()
	{
		
		float distance = (m_lookAheadPoint - transform.position).magnitude - Mathf.Max(transform.localScale.x, transform.localScale.z);
		print (distance);
		if (distance < lookAheadDistance*waypointSwitchModifier && !m_waypointIsStrict)
			return true;
		if (m_waypointIsStrict && GetDistanceToThePoint (m_currentWaypoint) < distance_epsilon)
			return true;
		return false;
	}

	float GetDistanceToALine(Vector3 lineA)
	{
		Vector3 lineB = transform.position - m_currentWaypoint;
		return(Mathf.Sin (Vector3.Angle (lineA, lineB)) * lineB.magnitude);
	}

	void PerformStep()
	{

		//m_lookAheadPoint = transform.position + m_currentDirection * lookAheadDistance;
		float distanceToWp = GetDistanceToThePoint (m_currentWaypoint);
			//lookDistance = GetDistanceToThePoint (m_lookAheadPoint);


		if (distanceToWp < lookAheadDistance) {
			if (!m_waypointIsStrict){
				if (m_currentWaypointIndex != waypoints.Length - 1) {

					Vector3 crossWpDirection = GetDirectionFromOnePointToAnother (m_currentWaypoint, waypoints [m_currentWaypointIndex + 1].transform.position);
					//float distanceToAnEdge = Mathf.Abs (GetDistanceToALine (crossWpDirection));
					//print (lookAheadDistance - distanceToAnEdge);
					m_currentDirection = GetDirectionFromOnePointToAnother (transform.position,
						m_currentWaypoint + crossWpDirection * (lookAheadDistance - distanceToWp));
					m_lookAheadPoint = m_currentWaypoint + crossWpDirection * (lookAheadDistance - distanceToWp);
//
//					m_currentDirection = GetDirectionFromOnePointToAnother (transform.position,
//						m_currentWaypoint + crossWpDirection * (lookAheadDistance - distanceToWp));
//					m_lookAheadPoint = m_currentWaypoint + crossWpDirection * (lookAheadDistance - distanceToWp);
				}
			}
		} else {
			m_currentDirection = GetDirectionTowardsWaypoint ();
			m_lookAheadPoint = transform.position + lookAheadDistance * m_currentDirection;
		}
		transform.position += m_currentDirection * speed;
		previousFrameDistance = distanceToWp;
	}

	float GetDistanceToThePoint(Vector3 point) {
		return ((point - transform.position).magnitude);
	}

	Vector3 GetDirectionTowardsWaypoint(){
		return (m_currentWaypoint - transform.position).normalized;
	}

	Vector3 GetDirectionFromOnePointToAnother(Vector3 from, Vector3 to)
	{
		return (to - from).normalized;
	}


	// Update is called once per frame
	void Update () {
		if (m_isMovingAlongTheRoute) {
			if (IsWaypointReached ()) {
				if (!PickNewWaypoint ())
					m_isMovingAlongTheRoute = false;
			} else
				PerformStep ();
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawSphere (m_lookAheadPoint,1f);
		Gizmos.DrawLine (m_lookAheadPoint, transform.position);
	}
}
