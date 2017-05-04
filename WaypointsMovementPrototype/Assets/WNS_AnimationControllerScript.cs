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
//			if (displacement > m_walkRunSpeedThreshold)
//			{
//				m_animator.SetBool ("IsRunning", true);
//				m_animator.SetBool ("IsWalking", false);
//				m_animator.SetFloat ("Speed", m_runSpeedCoeff * displacement);
//			}
//			else
//			{
//				m_animator.SetBool ("IsWalking", true);
//				m_animator.SetBool ("IsRunning", false);
//				m_animator.SetFloat ("Speed", m_walkSpeedCoeff * displacement);
//			}
			m_animator.SetFloat("Speed",m_speedCoeff * m_displacement);
		}
	}

	public void Stop()
	{
		if (m_animator)
		{
			m_animator.SetBool ("IsWalking", false);
			m_animator.SetBool ("IsRunning", false);
			m_animator.SetFloat ("Speed", 0f);
		}
	}

	// Update is called once per frame
	void Update () {
	
	}
}
