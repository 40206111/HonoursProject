using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Numbers : MonoBehaviour {

    [SerializeField]
    private TMP_InputField theText;
    [SerializeField]
    private TMP_InputField lineNos;

    private static int charicters = 0;
    private static int current = 1;

    public void OnValueChanged()
    {
        int change = theText.text.Length - charicters;
        charicters = theText.text.Length;

        for (int i = 0; i < change; ++i)
        {
            if (theText.text[theText.text.Length - i - 1] == '\n')
            {
                current += 1;
                lineNos.text += "\n" + current;
            }
        }
      }
}
