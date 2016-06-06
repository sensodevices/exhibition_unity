using UnityEngine;

public class FingerTip : MonoBehaviour {

	public bool isPulling { get { return m_isPulling; } }
	private bool m_isPulling = false;
	
	public PotObject m_pot;
	
	private Collider m_collider;
	private Vibrator m_vibrator;
	
	// Use this for initialization
	void Start () {
		m_collider = GetComponent<SphereCollider>();
		m_vibrator = GetComponent<Vibrator>();
	}
	
	// Update is called once per frame
	void Update () {
		if (m_isPulling) {
			m_pot.PullLayer(transform.position, m_collider.bounds.extents.y);
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.GetComponent<FingerTip>() != null) {
				m_isPulling = true;
				m_vibrator.StartVibrate(5);
		}
	}

	void OnTriggerExit(Collider other) {
		if (other.gameObject.GetComponent<FingerTip>() != null && m_isPulling) {
			m_isPulling = false;
			m_vibrator.StopVibrate();
			m_pot.StopPull();
		}
	}
}
