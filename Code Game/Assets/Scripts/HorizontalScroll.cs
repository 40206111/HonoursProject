using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalScroll : MonoBehaviour {

    [SerializeField]
    private GameObject panel;
    private RectTransform panRect;
    [SerializeField]
    private UnityEngine.UI.InputField inputBox;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {

	}

    public void OnValueChanged()
    {
        Debug.Log("test");
    }
}
