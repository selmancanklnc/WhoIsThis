using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonEffect : MonoBehaviour
{
    public Button soundButton;
    public Button effectdButton;

    public Sprite oldSprite;
    public Sprite newSprite;

    public Sprite oldeffectSprite;
    public Sprite neweffectSprite;

    public AudioSource audioSource;

    public int soundOn = 1;
    public int effectOn = 1;

    public static ButtonEffect Instance;

    // Start is called before the first frame update
    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);

        }
        else if (Instance == this)
        {
            Destroy(this.gameObject);
        }


        soundOn = PlayerPrefs.GetInt(nameof(soundOn));
        effectOn = PlayerPrefs.GetInt(nameof(effectOn));

    }

    // Update is called once per frame
    void Update()
    {
        soundChange();
        effectChange();
    }


    public void effectChange()
    {
        if (effectOn == 1)
        {         
            effectdButton.image.sprite = oldeffectSprite;

        }
        else if (effectOn == 0)
        {         
            effectdButton.image.sprite = neweffectSprite;
        }

        PlayerPrefs.SetInt(nameof(effectOn), effectOn);

    }


    public void soundChange()
    {
        if (soundOn == 1)
        {
            audioSource.mute = false;
            soundButton.image.sprite = oldSprite;


        }
        else if (soundOn == 0)
        {
            audioSource.mute = true;
            soundButton.image.sprite = newSprite;

        }
        PlayerPrefs.SetInt(nameof(soundOn), soundOn);

    }

    public void ChangeImage()
    {

        if (soundButton.image.sprite != newSprite)
        {
            soundOn = 0;
        }
        else
        {
            soundOn = 1;
        }

        soundChange();

    }

    public void ChangeImageEffect()
    {
        if(effectdButton.image.sprite != neweffectSprite)
        {
            effectOn = 0;
        }
        else 
        { 
            effectOn = 1; 
        }

        effectChange();


    }

}
//    public void KaydetAyarlar()
//    {
//        // Ayarlarý PlayerPrefs ile kaydedin.
//        //PlayerPrefs.SetFloat("Ayar1", audioSource.mute);
//        PlayerPrefs.Save(); // PlayerPrefs verilerini diske kaydedin (opsiyonel).
//    }
//}
