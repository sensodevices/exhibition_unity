using UnityEngine;
using UnityEngine.UI;
using System;

class Cross : MonoBehaviour {

    public float SecondsToActivate = 3.0f;
    private GameObject prevGO = null;
    private float m_activateTimer = 0.0f;

    void Update() 
    {
        // FIXME: make this more universal, not only MenuItem
        var point = Camera.main.WorldToScreenPoint(transform.position);
        var ray = Camera.main.ScreenPointToRay(point);
        var hits = Physics.RaycastAll(ray);
        if (hits.Length > 0){
            foreach (var hit in hits){
                var go = hit.collider.gameObject;
                if (go.tag == "MenuItem"){
                    if (go != prevGO) {
                        if (prevGO != null) 
                            SetButtonCollor(prevGO, Color.white);    
                        // SetButtonCollor(go, Color.green);
                        prevGO = go;
                        m_activateTimer = 0.0f;
                        break;
                    } else {
                        m_activateTimer += Time.deltaTime;
                        if (m_activateTimer > SecondsToActivate) m_activateTimer = SecondsToActivate;
                        float grColor = 1.0f - m_activateTimer / SecondsToActivate;
                        Color aColor = new Color(grColor, 1.0f, grColor, 1.0f);
                        SetButtonCollor(go, aColor);

                        if (m_activateTimer >= SecondsToActivate) {
                            ButtonClick(go);
                            GlobalFuncs.ToggleCross(false);
                        }
                    }
                }
                
            }
        }
    }

    private void SetButtonCollor(GameObject go, Color aColor) {
        var btn = go.GetComponent<Button>();
        var c = btn.colors;
        c.normalColor = aColor;
        btn.colors = c;
    }

    private void ButtonClick(GameObject go) {
        var btn = go.GetComponent<Button>();
        btn.onClick.Invoke();
    }

}