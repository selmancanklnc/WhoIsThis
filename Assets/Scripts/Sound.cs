using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound : MonoBehaviour
{

    public AudioSource effect;
    public AudioSource music;

    ButtonEffect buttonEffect;
    // Start is called before the first frame update
    void Start()
    {

       int musicWhat = ButtonEffect.Instance.soundOn;
       int effectWhat = ButtonEffect.Instance.effectOn;



        buttonEffect = GetComponent<ButtonEffect>();

        if (musicWhat == 0 )
        {          
            music.mute = true;
            Debug.Log("M�zik Kapal� Olmal�");
        }
        else
        {
            music.mute = false;
            Debug.Log("M�zik A��k Olmal�");

        }

        if (effectWhat == 0 )
        {
            effect.mute = true;
            Debug.Log("Efekt Kapal� Olmal�");

        }
        else
        {
            effect.mute = false;
            Debug.Log("Efekt A��k Olmal�");

        }

        Debug.Log(buttonEffect.effectOn+"Efekt Kontrol");
    }

    // Update is called once per frame
    void Update()
    {
        
    }





}
