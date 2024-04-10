using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Button = UnityEngine.UI.Button;
using Image = UnityEngine.UI.Image;
using UnityEngine.SceneManagement;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using Firebase.Database;
using GoogleMobileAds.Api;
using Firebase.Extensions;
using System.Globalization;

public class Game : MonoBehaviour
{
    #region Public

    public Button yourButton;
    public Button restartButton;
    public RawImage image;
    public Image scoreTable;
    public Player player;
    public TMP_Text pointText;
    public TMP_Text scoreText;
    public TMP_Text maxscoreText;
    public TMP_Text truePlayerName;
    public List<int> numbers = new List<int>() { };
    public Button sendButton;
    public int point = 0;
    public int health = 3;
    public List<Player> players = new List<Player>() { };
    public Image trueImage;
    public Image falseImage;
    public AudioClip goalSound;
    public AudioClip falseSound;
    public AudioClip tipSound;
    public AudioSource soundSource;
    public AudioSource effect;
    public AudioSource music;
    ButtonEffect buttonEffect;
    #endregion
    DatabaseReference reference;
    public InterstitialAd interstitialAd;
    BannerView _bannerView;

    #region Private Dropdown
    //[SerializeField] private Button blockerButton;
    [SerializeField] private GameObject buttonsPrefab = null;
    [SerializeField] private int maxScrollRectSize = 400;
    [SerializeField] private List<string> avlOptions = new List<string>();

    private Button ddButton = null;
    private TMP_InputField inputField = null;
    private ScrollRect scrollRect = null;
    private Transform content = null;
    private RectTransform scrollRectTrans;
    private bool isContentHidden = true;
    private bool canClick = true;
    private List<Button> initializedButtons = new List<Button>();

    public delegate void OnValueChangedDel(string val);
    public OnValueChangedDel OnValueChangedEvt;
    #endregion




    void Start()
    {

        #region SESLER

        int musicWhat = ButtonEffect.Instance.soundOn;
        int effectWhat = ButtonEffect.Instance.effectOn;
        buttonEffect = GetComponent<ButtonEffect>();
        reference = FirebaseDatabase.DefaultInstance.RootReference;

        if (musicWhat == 0)
        {
            music.mute = true;
            Debug.Log("M?zik Kapal? Olmal?");
        }
        else
        {
            music.mute = false;
            Debug.Log("M?zik A??k Olmal?");

        }

        if (effectWhat == 0)
        {
            effect.mute = true;
            Debug.Log("Efekt Kapal? Olmal?");

        }
        else
        {
            effect.mute = false;
            Debug.Log("Efekt A??k Olmal?");

        }




        #endregion

        reference.Child("settings").GetValueAsync().ContinueWithOnMainThread(d =>
        {

            DataSnapshot snapshot = d.Result;
            if (snapshot?.Value != null)
            {
                var json = JsonConvert.SerializeObject(snapshot.Value);
                //json = JsonConvert.DeserializeObject<string>(json);

                var model = JsonConvert.DeserializeObject<Settings>(json);

                var adModGoogleId = model.adMobBannerGoogleId;
                if (!string.IsNullOrWhiteSpace(adModGoogleId))
                {
                    MobileAds.Initialize((InitializationStatus initStatus) =>
                    {
                        if (_bannerView != null)
                        {
                            _bannerView.Destroy();
                            _bannerView = null;
                        }
                        else
                        {
                            _bannerView = new BannerView(adModGoogleId, AdSize.IABBanner, AdPosition.Bottom);
                            var adRequest = new AdRequest();
                            adRequest.Keywords.Add("unity-admob-sample");

                            _bannerView.LoadAd(adRequest);
                        }

                    });
                }

            }
        });

        for (int i = 1; i < 10; i++)
        {
            numbers.Add(i);
        }
        Button btn = yourButton.GetComponent<Button>();
        btn.onClick.AddListener(NextTip);
        Button snd = sendButton.GetComponent<Button>();
        snd.onClick.AddListener(Send);

        players = GetPlayers().Select(a => a.Value).ToList();
        Debug.Log(players.Count);
        scoreTable.GetComponent<Image>().enabled = false;


        newGame();
        Init();


    }

    void Update()
    {

    }
    public void GoMainMenuButton()
    {
        SceneManager.LoadScene(1);
    }
    void NextTip()
    {
        var rand = UnityEngine.Random.Range(0, numbers.Count);
        var number = numbers[rand];
        var button = GameObject.Find("b" + number);
        numbers.Remove(number);
        button.GetComponent<Image>().enabled = false;

        if (!numbers.Any())
        {
            yourButton.interactable = false;
        }
    }
    public void LoadInterstitialAd(string adModGoogleId)
    {
        // Clean up the old ad before loading a new one.
        if (interstitialAd != null)
        {
            interstitialAd.Destroy();
            interstitialAd = null;
        }

        Debug.Log("Loading the interstitial ad.");

        // create our request used to load the ad.
        var adRequest = new AdRequest();
        adRequest.Keywords.Add("unity-admob-sample");

        // send the request to load the ad.
        InterstitialAd.Load(adModGoogleId, adRequest,
            (InterstitialAd ad, LoadAdError error) =>
            {
                // if error is not null, the load request failed.
                if (error != null || ad == null)
                {
                    Debug.LogError("interstitial ad failed to load an ad " +
                                   "with error : " + error);
                    return;
                }

                Debug.Log("Interstitial ad loaded with response : "
                          + ad.GetResponseInfo());

                interstitialAd = ad;
            });
    }
    void newGame()
    {


        yourButton.interactable = true;
        trueImage.gameObject.SetActive(false);
        falseImage.gameObject.SetActive(false);
        truePlayerName.text = "";


        var rand = UnityEngine.Random.Range(0, players.Count);
        player = players[rand];
        StartCoroutine(GetText(player.Image));
        numbers = new List<int>() { };
        for (int i = 1; i < 10; i++)
        {

            numbers.Add(i);
        }
        foreach (var item in numbers)
        {
            var button = GameObject.Find("b" + item);
            button.GetComponent<Image>().enabled = item != 5;
        }
        numbers.Remove(5);

        if (inputField != null)
        {
            inputField.text = "";
            inputField.Select();

        }

    }

    public void TipButton()
    {
        int effectWhat = ButtonEffect.Instance.effectOn;
        buttonEffect = GetComponent<ButtonEffect>();
        if (effectWhat == 1)
        {
            soundSource.PlayOneShot(tipSound);

        }
    }

    private void EnableButton()
    {
        canClick = true;
        sendButton.interactable = true;
    }

    void Send()
    {
        int effectWhat = ButtonEffect.Instance.effectOn;
        buttonEffect = GetComponent<ButtonEffect>();
        if (canClick)
        {
            sendButton.interactable = false;
            canClick = false;

            // Belirtilen s?re sonra butonu tekrar etkinle?tir
            Invoke("EnableButton", 2);

            if (inputField.text == player.Name)
            {
                if (effectWhat == 1)
                {
                    soundSource.PlayOneShot(goalSound);

                }

                foreach (var number in numbers)
                {
                    var button = GameObject.Find("b" + number);
                    button.GetComponent<Image>().enabled = false;
                }

                point += 10 * numbers.Count;
                pointText.text = point.ToString();
                trueImage.gameObject.SetActive(true);
                StartCoroutine(WaitForFunction());

            }
            else
            {
                if (effectWhat == 1)
                {
                    soundSource.PlayOneShot(falseSound);

                }

                var healthBar = GameObject.Find("CanBar" + health);
                healthBar.GetComponent<Image>().enabled = false;
                health--;
                falseImage.gameObject.SetActive(true);
                truePlayerName.text = player.Name;




                if (health == 0)
                {
                    Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance; 
                    var userId = auth?.CurrentUser?.UserId;
                    if (string.IsNullOrWhiteSpace(userId))
                    {
                        return;
                    }

                    reference.Child("users").Child(userId).GetValueAsync().ContinueWith(d =>
                    {
                        int topScore = 0;
                        DataSnapshot snapshot = d.Result;
                        if (snapshot?.Value != null)
                        {
                            var json = JsonConvert.SerializeObject(snapshot.Value);
                            //   json = JsonConvert.DeserializeObject<string>(json);

                            var model = JsonConvert.DeserializeObject<User>(json);
                            topScore = model.TopScore;
                            if (topScore < point)
                            {
                                topScore = point;
                                model.TopScore = topScore;
                                reference.Child("users").Child(userId).Child("TopScore").SetValueAsync(topScore);
                            }
                            maxscoreText.text = topScore.ToString();



                        }

                    });
                    //var maxScore = PlayerPrefs.GetInt("MaxScore");
                    StartCoroutine(WaitForFunction2());


                }
                else
                {
                    foreach (var number in numbers)
                    {
                        var button = GameObject.Find("b" + number);
                        button.GetComponent<Image>().enabled = false;

                    }
                    StartCoroutine(WaitForFunction());
                }
            }
        }

    }

    private void RestartGame()
    {

        SceneManager.LoadScene(2);

    }

    IEnumerator WaitForFunction()
    {
        yield return new WaitForSeconds(2);

        newGame();
    }


    IEnumerator WaitForFunction2()
    {
        yield return new WaitForSeconds(2);
        StartCoroutine(WaitForFunction());
        scoreTable.GetComponent<Image>().enabled = true;
        scoreTable.gameObject.SetActive(true);

        scoreText.text = point.ToString();



        Button restart = restartButton.GetComponent<Button>();
        restart.onClick.AddListener(RestartGame);
        reference.Child("settings").GetValueAsync().ContinueWithOnMainThread(d =>
        {

            DataSnapshot snapshot = d.Result;
            if (snapshot?.Value != null)
            {
                var json = JsonConvert.SerializeObject(snapshot.Value);
                //json = JsonConvert.DeserializeObject<string>(json);

                var model = JsonConvert.DeserializeObject<Settings>(json);

                var adModGoogleId = model.adModGoogleId;
                if (!string.IsNullOrWhiteSpace(adModGoogleId))
                {
                    MobileAds.Initialize((InitializationStatus initStatus) =>
                    {

                        LoadInterstitialAd(adModGoogleId);
                        if (interstitialAd != null && interstitialAd.CanShowAd())
                        {
                            Debug.Log("Showing interstitial ad.");
                            interstitialAd.Show();
                        }
                        else
                        {
                            Debug.LogError("Interstitial ad is not ready yet.");
                        }
                    });
                }

            }
        });
        //skor


    }


    IEnumerator GetText(string url)
    {
        using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url))
        {
            yield return uwr.SendWebRequest();

            if (uwr.isNetworkError || uwr.isHttpError)
            {
                Debug.Log(uwr.error);
            }
            else
            {
                // Get downloaded asset bundle
                var t = DownloadHandlerTexture.GetContent(uwr);
                image.texture = t;
            }
        }
    }

    #region DropDown


    private void Init()
    {
        ddButton = this.GetComponentInChildren<Button>();
        scrollRect = this.GetComponentInChildren<ScrollRect>();
        inputField = this.GetComponentInChildren<TMP_InputField>();
        scrollRectTrans = scrollRect.GetComponent<RectTransform>();
        content = scrollRect.content;
        foreach (var player in players)
        {
            avlOptions.Add(player.Name);

        }
        ////blocker is a button added and scaled it to screen size so that we can close the dd on clicking outside
        //blockerButton.GetComponent<RectTransform>().sizeDelta = new Vector2(Screen.width, Screen.height);
        //blockerButton.gameObject.SetActive(false);
        //blockerButton.transform.SetParent(this.GetComponentInParent<Canvas>().transform);

        //blockerButton.onClick.AddListener(OnBlockerButtClick);
        ddButton.onClick.AddListener(OnDDButtonClick);
        scrollRect.onValueChanged.AddListener(OnScrollRectvalueChange);
        inputField.onValueChanged.AddListener(OnInputvalueChange);
        inputField.onEndEdit.AddListener(OnEndEditing);
        inputField.Select();
        AddItemToScrollRect(avlOptions);

    }

    /// <summary>
    /// public method to get the selected value
    /// </summary>
    /// <returns></returns>
    public string GetValue()
    {
        return inputField.text;
    }

    public void ResetDropDown()
    {
        inputField.text = string.Empty;

    }

    //call this to Add items to Drop down
    public void AddItemToScrollRect(List<string> options)
    {
        foreach (var option in options)
        {
            var buttObj = Instantiate(buttonsPrefab, content);
            buttObj.GetComponentInChildren<TMP_Text>().text = option;
            buttObj.name = option;
            buttObj.SetActive(true);
            var butt = buttObj.GetComponent<Button>();
            butt.onClick.AddListener(delegate { OnItemSelected(buttObj); });
            initializedButtons.Add(butt);
        }
        ResizeScrollRect();
        scrollRect.gameObject.SetActive(false);
    }


    /// <summary>
    /// listner To Input Field End Editing
    /// </summary>
    /// <param name="arg"></param>
    private void OnEndEditing(string arg)
    {
        if (string.IsNullOrEmpty(arg))
        {
            Debug.Log("no value entered ");
            return;
        }
        StartCoroutine(CheckIfValidInput(arg));
    }

    /// <summary>
    /// Need to wait as end inputField and On option button  Contradicted and message was poped after selection of button
    /// </summary>
    /// <param name="arg"></param>
    /// <returns></returns>
    IEnumerator CheckIfValidInput(string arg)
    {
        yield return new WaitForSeconds(1);
        if (!avlOptions.Contains(arg))
        {
            // Message msg = new Message("Invalid Input!", "Please choose from dropdown",
            //                 this.gameObject, Message.ButtonType.OK);
            //
            //             if (MessageBox.instance)
            //                 MessageBox.instance.ShowMessage(msg); 

            //inputField.text = String.Empty;
        }
        //else
        //    Debug.Log("good job " );
        OnValueChangedEvt?.Invoke(inputField.text);
    }
    /// <summary>
    /// Called ever time on Drop down value is changed to resize it
    /// </summary>
    private void ResizeScrollRect()
    {
        //TODO Dont Remove this until checked on Mobile Deveice
        //var count = content.transform.Cast<Transform>().Count(child => child.gameObject.activeSelf);
        //var length = buttonsPrefab.GetComponent<RectTransform>().sizeDelta.y * count;

        LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)content.transform);
        var length = content.GetComponent<RectTransform>().sizeDelta.y;

        scrollRectTrans.sizeDelta = length > maxScrollRectSize ? new Vector2(scrollRectTrans.sizeDelta.x,
            maxScrollRectSize) : new Vector2(scrollRectTrans.sizeDelta.x, length + 5);
    }

    /// <summary>
    /// listner to the InputField
    /// </summary>
    /// <param name="arg0"></param>
    private void OnInputvalueChange(string arg0)
    {
        if (!avlOptions.Contains(arg0))
        {
            FilterDropdown(arg0);
        }
    }

    /// <summary>
    /// remove the elements from the dropdown based on Filters
    /// </summary>
    /// <param name="input"></param>
    public void FilterDropdown(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            foreach (var button in initializedButtons)
                button.gameObject.SetActive(true);
            ResizeScrollRect();
            scrollRect.gameObject.SetActive(false);
            return;
        }

        var count = 0;
        foreach (var button in initializedButtons)
        {
            if (!button.name.ToLower(CultureInfo.GetCultureInfo("en-US")).Contains(input.ToLower(CultureInfo.GetCultureInfo("en-US"))))
            {
                button.gameObject.SetActive(false);
            }
            else
            {
                button.gameObject.SetActive(true);
                count++;
            }
        }

        SetScrollActive(count > 0);
        ResizeScrollRect();
    }

    /// <summary>
    /// Listner to Scroll rect
    /// </summary>
    /// <param name="arg0"></param>
    private void OnScrollRectvalueChange(Vector2 arg0)
    {
        //Debug.Log("scroll ");
    }

    /// <summary>
    /// Listner to option Buttons
    /// </summary>
    /// <param name="obj"></param>
    private void OnItemSelected(GameObject obj)
    {
        inputField.text = obj.name;
        foreach (var button in initializedButtons)
            button.gameObject.SetActive(true);
        isContentHidden = false;
        OnDDButtonClick();
        //OnEndEditing(obj.name);
        StopAllCoroutines();
        StartCoroutine(CheckIfValidInput(obj.name));
    }

    /// <summary>
    /// listner to arrow button on input field
    /// </summary>
    private void OnDDButtonClick()
    {
        if (GetActiveButtons() <= 0)
            return;
        ResizeScrollRect();
        SetScrollActive(isContentHidden);
    }
    private void OnBlockerButtClick()
    {
        SetScrollActive(false);
    }

    /// <summary>
    /// respondisble to enable and disable scroll rect component 
    /// </summary>
    /// <param name="status"></param>
    private void SetScrollActive(bool status)
    {
        scrollRect.gameObject.SetActive(status);
        //   blockerButton.gameObject.SetActive(status);
        isContentHidden = !status;
        ddButton.transform.localScale = status ? new Vector3(1, -1, 1) : new Vector3(1, 1, 1);
    }

    /// <summary>
    /// Return numbers of active buttons in the dropdown
    /// </summary>
    /// <returns></returns>
    private float GetActiveButtons()
    {
        var count = content.transform.Cast<Transform>().Count(child => child.gameObject.activeSelf);
        var length = buttonsPrefab.GetComponent<RectTransform>().sizeDelta.y * count;
        return length;
    }
    #endregion

    public static Dictionary<string, Player> GetPlayers()
    {
        using (var httpClient = new HttpClient())
        {
            using (var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://whoisthis-849c6-default-rtdb.firebaseio.com/players.json?auth=AIzaSyAZTNguCdT73BLxevX70a6PQHrtnJM0nDc"))
            {
                request.Headers.TryAddWithoutValidation("accept", "application/json");

                var response = httpClient.SendAsync(request).GetAwaiter().GetResult();
                using (var stream = response.Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                {
                    var resultModel = ServiceHelperMethods.JsonDeserializeFromStream<Dictionary<string, Player>>(stream);
                    return resultModel;
                }
            }
        }

        return null;
    }

}

[Serializable]
public class Player
{
    public string Name;
    public string Image;
}

[Serializable]
public class User
{
    public string Name;
    public string Email;
    public int TopScore;
    public string Token;
    public string ImageUrl;
}
public class Settings
{
    public string adModGoogleId;
    public string adMobBannerGoogleId;
}
public class LeaderBoardModel
{
    public string Name;
    public string UserId;
    public int TopScore;
    public int Order;
}
public static class ServiceHelperMethods
{
    public static T JsonDeserializeFromStream<T>(Stream stream)
    {
        using (StreamReader sr = new StreamReader(stream))
        using (JsonReader reader = new JsonTextReader(sr))
        {
            JsonSerializer serializer = new JsonSerializer();
            // read the json from a stream
            // json size doesn't matter because only a small piece is read at a time from the HTTP request
            return serializer.Deserialize<T>(reader);
        }
    }
}