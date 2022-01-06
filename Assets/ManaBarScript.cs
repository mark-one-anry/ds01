using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ManaBarScript : MonoBehaviour
{
    public Slider slider;

    

    public void SetMana(float mana)
    {
        slider.value = mana;
    }

    public void SetMaxMana(float maxMana)
    {
        if (slider == null)
            slider = GetComponent<Slider>();
        if (slider != null) { 
            slider.maxValue = maxMana;
            slider.value = maxMana;
        }
    }

    void Start()
    {
        slider = GetComponent<Slider>();
        if(slider == null)
        {
            Debug.Log("No Slider attached to ManaBar!");
        }
    }

    private void Update()
    {
        transform.rotation = Quaternion.identity;
    }
}
