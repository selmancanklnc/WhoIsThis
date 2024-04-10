using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DropDownSearchItem : MonoBehaviour
{

    public TextMeshProUGUI tex_name;

    //Private variables
    private string name;
    private string data;


    public void setData(string name, string data)
    {
        this.name = name;
        this.data = data;
        tex_name.text = name;
    }

    public string getData()
    {
        return data;
    }
}