using UnityEngine;
using System.Collections;

public class WNS_AnimationControllerScript : MonoBehaviour {

	private Animator m_animator;
	public float m_speedCoeff = 1f;
	private float m_displacement;

	// Use this for initialization
	void Start () {
		m_animator = GetComponent<Animator> ();
		if (!m_animator)
			print ("Animator is not linked");
	}


	public void HandleSpeedChange(float displacement)
	{
		m_displacement = displacement;
		if (m_animator)
		{
			m_animator.SetFloat("Speed",m_speedCoeff * m_displacement);
		}
	}

	public void HandleSpeedChange(Vector3 rvoPosition, Vector3 originalPosition, Vector3 previousFramePosition)
	{
//		Vector3 fwDir = GlobalScript.GetDirection (previousFramePosition, originalPosition);
//		Vector3 rvoDir = GlobalScript.GetDirection (previousFramePosition, rvoPosition);
//		if (GlobalScript.GetAngle(fwDir,rvoDir) > 90)
//			m_animator.SetFloat("Speed",0f);
//		else
//			m_animator.SetFloat("Speed",m_speedCoeff * GlobalScript.GetDistance(rvoPosition,previousFramePosition));
	}

	public void Stop()
	{
		if (m_animator)
		{
			m_animator.SetFloat ("Speed", 0f);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
