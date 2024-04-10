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
            Debug.Log("Müzik Kapalý Olmalý");
        }
        else
        {
            music.mute = false;
            Debug.Log("Müzik Açýk Olmalý");

        }

        if (effectWhat == 0 )
        {
            effect.mute = true;
            Debug.Log("Efekt Kapalý Olmalý");

        }
        else
        {
            effect.mute = false;
            Debug.Log("Efekt Açýk Olmalý");

        }

        Debug.Log(buttonEffect.effectOn+"Efekt Kontrol");
    }

    // Update is called once per frame
    void Update()
    {
        
    }





}
