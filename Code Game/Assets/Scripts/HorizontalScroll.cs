using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HorizontalScroll : MonoBehaviour
{

    [SerializeField]
    private GameObject panel;
    private RectTransform panRect;
    [SerializeField]
    private InputField inputBox;
    [SerializeField]
    private GameObject textField;
    private RectTransform textRect;
    [SerializeField]
    private Scrollbar sc;
    private CaretPos Carrot;
    [SerializeField]
    private Canvas can;
    private float ratio = 1;
    private float prevVal = 0;

    [SerializeField]
    private GameObject otherText;

    // Use this for initialization
    void Start()
    {
        panRect = panel.GetComponent<RectTransform>();
        textRect = textField.GetComponent<RectTransform>();
        Carrot = GetComponent<CaretPos>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("right"))
        {
            float var1 = Carrot.LocalCaretPos().x;
            float var2 = otherText.transform.position.x / can.scaleFactor;
            float var3 = panRect.transform.position.x / can.scaleFactor;
            float var4 = (panRect.rect.width / 2);
            Debug.Log("x: " + var1 + ", y: " + var2
                + ", z: " + var3 + ", w: " + var4);
            //Values wrong currently
            if (var1 + var2 > var3 + var4)
            {
                Debug.Log("Something significant or distinct");
                sc.value += 0.25f * ratio;
            }
        }
    }

    public void resizeBar()
    {
        if (ratio < 1 || textRect.rect.width > (panRect.rect.width * 0.75))
        {
            ratio = (panRect.rect.width * 0.75f) / textRect.rect.width;
            sc.size = ratio;
            Canvas.ForceUpdateCanvases();
        }
        //Values wrong currently
        //if (Carrot.LocalCaretPos().x + otherText.transform.position.x > panRect.transform.position.x * can.scaleFactor)
        //{
        //    sc.value += 0.25f * ratio;
        //}
    }

    public void OnValueChanged()
    {
        float diff = prevVal - sc.value;
        prevVal = sc.value;
        Vector3 pos = inputBox.transform.localPosition;
        pos.x += ((textRect.rect.width - panRect.rect.width / 2) * diff);
        inputBox.transform.localPosition = pos;

    }

}
