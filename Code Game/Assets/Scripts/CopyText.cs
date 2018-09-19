using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CopyText : MonoBehaviour {

    [SerializeField]
    UnityEngine.UI.Text theText;

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnValueChanged()
    {
        StartCoroutine(WaitForText());
    }

    IEnumerator WaitForText()
    {
        yield return null;
        theText.text = GetComponent<UnityEngine.UI.Text>().text;
    }
}
