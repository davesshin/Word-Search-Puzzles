using UnityEngine; using System; 
using System.Collections; 
using System.Collections.Generic;

public class WordSearch : MonoBehaviour {
	// you may customize these variables in the Unity Inspector however you want
    public bool useWordpool; // 'should we use the wordpool?'
    public TextAsset wordpool; // if true, wordpool will be utilized
    public string[] words; // overwritten if wordpool = true
    public int maxWordCount; // max number of words used
	public int maxWordLetters; // max length of word used 
    public bool allowReverse; // if true, words can be selected in reverse order.
    public int gridX, gridY; // grid dimensions
    public float sensitivity; // sensitivity of tiles when clicked
    public float spacing; // spacing between tiles
    public GameObject tile, background, current;             
    public Color defaultTint, mouseoverTint, identifiedTint;
    public bool ready = false, correct = false;
    public string selectedString = "";
    public List<GameObject> selected = new List<GameObject>();

    private List<GameObject> tiles = new List<GameObject>();
    private GameObject temporary, backgroundObject;
    private int identified = 0;
    private float time;
    private string[,] matrix;
    private Dictionary<string, bool> word = new Dictionary<string, bool>();
    private Dictionary<string, bool> insertedWords = new Dictionary<string, bool>();
    private string[] letters = new string[26]
	{"a", "b", "c", "d", "e", "f", "g", "h", "i", "j", "k", "l", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"};
    private Ray ray;
    private RaycastHit hit;
    private int mark = 0;

    private static WordSearch instance;
    public static WordSearch Instance {
        get {
			return instance;
		}
    }

	void Awake() {
        instance = this;
    }

    void Start() {
        List<string> findLength = new List<string>();
        int count = 0;

        if (useWordpool) {
            words = wordpool.text.Split(';');
        } else {
            maxWordCount = words.Length;
        }

        if (maxWordCount <= 0) {
            maxWordCount = 1;
        }

        Mix(words);
        Mathf.Clamp(maxWordLetters, 0, gridY < gridX ? gridX : gridY);
       
        while (findLength.Count < maxWordCount + 1) {
            if (words[count].Length <= maxWordLetters) {
                findLength.Add(words[count]);
            } 
			count++;
        }

        for (int i = 0; i < maxWordCount; i++) {
            if (!word.ContainsKey(findLength[i].ToUpper()) && !word.ContainsKey(findLength[i])) {
                    word.Add(findLength[i], false);
            }
        }

        Mathf.Clamp01(sensitivity);
        matrix = new string[gridX, gridY];
        InstantiateBG();

        for (int i = 0; i < gridX; i++) {
            for (int j = 0; j < gridY; j++) {
                temporary = Instantiate(tile, new Vector3(i * 1 * tile.transform.localScale.x * spacing, 10, j * 1 * tile.transform.localScale.z * spacing), Quaternion.identity) as GameObject;
                temporary.name = "tile-" + i.ToString() + "-" + j.ToString();
                temporary.transform.eulerAngles = new Vector3(180, 0, 0);
                temporary.transform.parent = backgroundObject.transform;
                BoxCollider boxCollider = temporary.GetComponent<BoxCollider>() as BoxCollider;
                boxCollider.size = new Vector3(sensitivity, 1, sensitivity);
                temporary.GetComponent<Letters>().letter.text = "";
                temporary.GetComponent<Letters>().gridX = i;
                temporary.GetComponent<Letters>().gridY = j;
                tiles.Add(tile);
                matrix[i, j] = "";
            }
        }
        CenterBG();
        InsertWords();
        FillRemaining();
        time = Time.time;
    }
    private void CenterBG() {
        backgroundObject.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width / 2, (Screen.height / 2) + 50, 200));
	}

    private void InstantiateBG() {
		if (gridX % 2 != 0 && gridY % 2 == 0) {
			backgroundObject = Instantiate (background, new Vector3 ((tile.transform.localScale.x * spacing)
			* (gridX / 2), 1, (tile.transform.localScale.z * spacing)
			* (gridY / 2) - (tile.transform.localScale.z * spacing)), Quaternion.identity) as GameObject;
		} else if (gridX % 2 == 0 && gridY % 2 != 0) {
			backgroundObject = Instantiate (background, new Vector3 ((tile.transform.localScale.x * spacing) * (gridX / 2)
			- (tile.transform.localScale.x * spacing), 1, (tile.transform.localScale.z * spacing) * (gridY / 2)), Quaternion.identity) as GameObject;
		} else {
			backgroundObject = Instantiate(background, new Vector3 ((tile.transform.localScale.x * spacing) * (gridX / 2) -
				(tile.transform.localScale.x * spacing), 1, (tile.transform.localScale.z * spacing) * (gridY / 2) - (tile.transform.localScale.z * spacing)), Quaternion.identity) as GameObject;
		}
        backgroundObject.transform.eulerAngles = new Vector3(180, 0, 0);
        backgroundObject.transform.localScale = new Vector3(((tile.transform.localScale.x * spacing) * gridX), 1, ((tile.transform.localScale.x * spacing) * gridY));
   }

    void Update() {
		if (Input.GetMouseButton (0)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				current = hit.transform.gameObject;
			}
			ready = true;
		}
		if (Input.GetMouseButtonUp (0)) {
			ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			if (Physics.Raycast (ray, out hit)) {
				current = hit.transform.gameObject;
			}
			Verify();
		}
	}

	private void Verify() {
        if (!correct) {
            foreach (KeyValuePair<string, bool> p in insertedWords) {
                if (selectedString.ToLower() == p.Key.Trim().ToLower()) {
                    foreach (GameObject g in selected) {
                        g.GetComponent<Letters>().identified = true;
                    }
                    correct = true;
                }
                if (allowReverse) {
                    if (Reverse(selectedString.ToLower()) == p.Key.Trim().ToLower()) {
                        foreach (GameObject g in selected) {
                            g.GetComponent<Letters>().identified = true;
                        }
                        correct = true;
                    }
                }
            }
        }

        if (correct) {
            insertedWords.Remove(selectedString);
            insertedWords.Remove(Reverse(selectedString));

			if (word.ContainsKey (selectedString)) {
				insertedWords.Add (selectedString, true);
			} else if (word.ContainsKey (Reverse (selectedString))) {
				insertedWords.Add (Reverse (selectedString), true);
			}
            identified++;
        }
        ready = false;
        selected.Clear();
        selectedString = "";
        correct = false;
    }

    private void InsertWords() {
        System.Random rn = new System.Random();
        foreach (KeyValuePair<string, bool> p in word) {
            string s = p.Key.Trim();
            bool placed = false;
            while (placed == false) {
                int row = rn.Next(gridX);
                int column = rn.Next(gridY);
                int directionX = 0;
                int directionY = 0;
                while (directionX == 0 && directionY == 0) {
                    directionX = rn.Next(3) - 1;
                    directionY = rn.Next(3) - 1;
                }                
                placed = InsertWord(s.ToLower(), row, column, directionX, directionY);
                mark++;
                if (mark > 100) {
                    break;
                }
            }
        }
    }

    private bool InsertWord(string word, int row, int column, int directionX, int directionY) {
        if (directionX > 0) {
            if (row + word.Length >= gridX) {
                return false;
            }
        }
        if (directionX < 0) {
            if (row - word.Length < 0) {
                return false;
            }
        }
        if (directionY > 0) {
            if (column + word.Length >= gridY) {
                return false;
            }
        }
        if (directionY < 0) {
            if (column - word.Length < 0) {
                return false;
            }
        }

		if (((0 * directionY) + column) == gridY - 1) {
			return false;
		}
	
        for (int i = 0; i < word.Length; i++) {
			if (!string.IsNullOrEmpty (matrix [(i * directionX) + row, (i * directionY) + column])) {
				return false;
			}
        }

        insertedWords.Add(word, false);
        char[] w = word.ToCharArray();
        for (int i = 0; i < w.Length; i++) {
            matrix[(i * directionX) + row, (i * directionY) + column] = w[i].ToString();
            GameObject.Find("tile-" + ((i * directionX) + row).ToString() + "-" + ((i * directionY) + column).ToString()).GetComponent<Letters>().letter.text = w[i].ToString();
        }
        return true;
    }

    private void FillRemaining() {
        for (int i = 0; i < gridX; i++) {
            for (int j = 0; j < gridY; j++) {
                if (matrix[i, j] == "") {
                    matrix[i, j] = letters[UnityEngine.Random.Range(0, letters.Length)];
                    GameObject.Find("tile-" + i.ToString() + "-" + j.ToString()).GetComponent<Letters>().letter.text = matrix[i, j];
                }
            }
        }
    }

    private void Mix(string[] words) {
        for (int t = 0; t < words.Length; t++) {
            string tmp = words[t];
            int r = UnityEngine.Random.Range(t, words.Length);
            words[t] = words[r];
            words[r] = tmp;
        }
    }

    private string TimeElapsed() {
        TimeSpan t = TimeSpan.FromSeconds(Mathf.RoundToInt(Time.time - time));
        return String.Format("{0:D2}:{1:D2}", t.Minutes, t.Seconds);
    }

    private string Reverse(string word) {
        string reversed = "";
        char[] letters = word.ToCharArray();
        for (int i = letters.Length - 1; i >= 0; i--) {
            reversed += letters[i];
        }
        return reversed;
    }

    void OnGUI() {
        GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("   Timer: ");
            GUILayout.Label(TimeElapsed());
            GUILayout.EndHorizontal();
   
        foreach (KeyValuePair<string, bool> p in insertedWords) {
            GUILayout.BeginHorizontal();
            GUILayout.Label("   " + p.Key);
            if (p.Value) {
				GUILayout.Label("*");
            }
            GUILayout.EndHorizontal();
        }
        GUILayout.EndVertical();
    }
}