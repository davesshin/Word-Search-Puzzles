using UnityEngine;
using System.Collections;

public class Letters : MonoBehaviour {
	
    public bool utilized = false;
    public bool identified = false;
	public TextMesh letter;
	public int gridX, gridY;

    void Start() {
        GetComponent<Renderer>().materials[0].color = WordSearch.Instance.defaultTint;
    }
    
    void Update() {
        if (WordSearch.Instance.ready) {
            if (!utilized && WordSearch.Instance.current == gameObject) {
                WordSearch.Instance.selected.Add(this.gameObject);
                GetComponent<Renderer>().materials[0].color = WordSearch.Instance.mouseoverTint;
                WordSearch.Instance.selectedString += letter.text;
                utilized = true;
            }
        }

        if (identified) {
			if (GetComponent<Renderer>().materials[0].color != WordSearch.Instance.identifiedTint) {
				GetComponent<Renderer>().materials[0].color = WordSearch.Instance.identifiedTint;
			} 
			return;
        }

        if (Input.GetMouseButtonUp(0)) {
            utilized = false;
			if (GetComponent<Renderer>().materials[0].color != WordSearch.Instance.defaultTint) {
				GetComponent<Renderer>().materials[0].color = WordSearch.Instance.defaultTint;
			}
        }
    }
}
