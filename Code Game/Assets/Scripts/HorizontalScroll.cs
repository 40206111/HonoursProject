using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HorizontalScroll : MonoBehaviour {

    [SerializeField]
    private GameObject panel;
    private RectTransform panRect;
    [SerializeField]
    private UnityEngine.UI.InputField inputBox;
    [SerializeField]
    private GameObject textField;
    private RectTransform textRect;
    [SerializeField]
    private UnityEngine.UI.Scrollbar sc;
    [SerializeField]
    private Transform Carrot;

    private float ratio = 1;
    private float prevVal = 0;

	// Use this for initialization
	void Start () {
        panRect = panel.GetComponent<RectTransform>();
        textRect = textField.GetComponent<RectTransform>();
        Carrot = transform.Find("InputField Input Caret");
	}
	
	// Update is called once per frame
	void Update () {
        if (Carrot == null)
        {
            Carrot = transform.Find("InputField Input Caret");
        }
        else
        {
            Vector3 pos = Carrot.transform.localPosition;
            pos.x += 1;
            Carrot.transform.localPosition = pos;
        }
    }

    public void resizeBar()
    {
        if (ratio < 1 || textRect.rect.width > (panRect.rect.width-2))
        {
            ratio = panRect.rect.width / textRect.rect.width;
            sc.size = ratio;
            Canvas.ForceUpdateCanvases();
        }
    }

    public void OnValueChanged()
    {
        float diff = prevVal - sc.value;
        prevVal = sc.value;
        Vector3 pos = inputBox.transform.localPosition;
        pos.x += ((textRect.rect.width - panRect.rect.width/2) * diff);
        inputBox.transform.localPosition = pos;
    }
}
