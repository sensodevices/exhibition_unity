using UnityEngine;

public class DrawingBoard : MonoBehaviour {

	public int textureSize = 1024;
	public float bottomU = 0.24f; // Bottom U component
	private float m_Usiz; // Remain size of U component of texture

	public Color[] drawColors; // Colors for each finger
	public int[] drawRadiuses; // Radiuses for each finger

	private Texture2D m_texture;
	Color[] m_colors;
	private Vector3 m_center;
	private Vector3 m_size;
	private float m_bottom;
	private float m_left;

	private bool m_needUpdateTexture = false;

	void Start()
	{
		m_Usiz = 1.0f - bottomU;

		m_texture = new Texture2D(textureSize, textureSize);
		Renderer rend = GetComponent<Renderer>();
		rend.material.mainTexture = m_texture;

		m_colors = new Color[textureSize*textureSize];
		for (int y = 0; y < textureSize; ++y) {
			for (int x = 0; x < textureSize; ++x) {
				m_colors[y * textureSize + x] = Color.white;
			}
		}
		m_texture.SetPixels(m_colors);
		m_texture.Apply();
		
		m_center = GetComponent<Renderer>().bounds.center;
		m_size = GetComponent<Renderer>().bounds.size;
		m_bottom = m_center.y - m_size.y / 2.0f;
		m_left = m_center.z - m_size.z / 2.0f;
	}

	void OnTriggerStay (Collider other) {
		var obj = other.gameObject;
		var finger = obj.GetComponent<FingerTarget>();
		if (finger != null && finger.fingerId != HandNetworkData.FingerType.Thumb) {
			float diffZ = finger.GetRelativeAngle();
			if (diffZ < 50) {
				var closestPoint = GetComponent<Renderer>().bounds.ClosestPoint(other.gameObject.transform.position);
				float X = ((closestPoint.z - m_left) / m_size.z);
				float Y = ((closestPoint.y - m_bottom) / m_size.y);
				// transform.position.y - transform.localScale
				DrawOnTexture(VtoX(X), UtoY(Y), drawColors[(int)finger.fingerId - 1], drawRadiuses[(int)finger.fingerId - 1]);
				m_needUpdateTexture = true;
			}
		}
	}

	void Update()
	{
		if (m_needUpdateTexture) {
			m_texture.SetPixels(m_colors);
			m_texture.Apply();
			m_needUpdateTexture = false;
		}
	}

	private int UtoY(float U) {
		return (int)Mathf.Floor(U * textureSize * m_Usiz + textureSize * bottomU);
	}
	private int VtoX(float V) {
		return (int)Mathf.Floor(V * textureSize);
	}

	private void DrawOnTexture(int x, int y, Color col, int radius)
	{
		float dRadius = radius * radius;
		for (int y1 = -radius; y1 < radius; ++y1) {
			for (int x1 = -radius; x1 < radius; ++x1) {
				if (x1 * x1 + y1 * y1 <= dRadius) {
					m_colors[(y + y1) * textureSize + (x + x1)] = col;
				}
			}
		}
	}
}
