using Firebase.Database;
using Firebase.Extensions;
using Google.MiniJSON;
using Newtonsoft.Json;
using NUnit.Framework.Internal;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
public class NextScene : MonoBehaviour
{


    public GameObject profilePanel;
    public GameObject netControlPanel;
    public TMP_Text userNameText;
    public TMP_Text[] Entries;
    public TMP_Text[] EntriesScore;
    public AudioListener audiolistener;
    public TMP_Text myRank;
    public GameObject exitPanel;


    DatabaseReference reference;


    // Start is called before the first frame update
    public void PlayGame()
    {
        SceneManager.LoadScene(2);
    }

    void Start()
    {
        reference = FirebaseDatabase.DefaultInstance.RootReference;
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        var userId = auth?.CurrentUser?.UserId;

        if (string.IsNullOrWhiteSpace(userId))
        {
            SceneManager.LoadScene(0);

        }


        reference.Child("users").Child(userId).GetValueAsync().ContinueWithOnMainThread(d =>
        {
            if (d.IsCanceled)
            {
                Debug.LogError("Firebase oturum açma iptal edildi.");

                return;
            }
            if (d.IsFaulted)
            {
                Debug.LogError("Firebase oturum açma baþarýsýz: " + d.Exception);

                return;
            }
            int topScore = 0;
            DataSnapshot snapshot = d.Result;

            if (snapshot.Exists)
            {

                var json = JsonConvert.SerializeObject(snapshot.Value);
                //json = JsonConvert.DeserializeObject<string>(json);
                var model = JsonConvert.DeserializeObject<User>(json);
                topScore = model.TopScore;
                userNameText.text = model.Name;
                UserRank(topScore);

            }
            else
            {
                SceneManager.LoadScene(0);

            }

        });



    }
    public void NetControl()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            if (!netControlPanel.gameObject.activeSelf)
            {
                netControlPanel.SetActive(true);

            }
            AudioListener.volume = 0;
        }
        else
        {
            if (netControlPanel.gameObject.activeSelf)
            {
                netControlPanel.SetActive(false);
            }
            AudioListener.volume = 1;


        }

    }


    void Update()
    {

        if (Input.GetKeyUp(KeyCode.Escape))
        {
            if (!profilePanel.activeSelf && !exitPanel.activeSelf)
            {
                exitPanel.SetActive(true);
            }
        }

        NetControl();


    }


    public void ApplicationQuit()
    {
        Application.Quit();
    }

    public void UserRank(int score)
    {
        reference.Child("users").OrderByChild("TopScore").EndAt(score).GetValueAsync().ContinueWithOnMainThread(d =>
        {

            DataSnapshot snapshot = d.Result;
            if (snapshot?.Value != null)
            {
                var rank = snapshot.Children.Count();
                myRank.text = rank.ToString();
            }
        });
    }


    public void FillLeaderboard()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        var userId = auth?.CurrentUser?.UserId;

        var leaderBoards = new List<LeaderBoardModel>();
        reference.Child("users").OrderByChild("TopScore").LimitToLast(10).GetValueAsync().ContinueWithOnMainThread(d =>
        {

            DataSnapshot snapshot = d.Result;
            if (snapshot?.Value != null)
            {
                int order = 1;
                foreach (var childSnapshot in snapshot.Children)
                {
                    var json = JsonConvert.SerializeObject(childSnapshot.Value);
                    //json = JsonConvert.DeserializeObject<string>(json);
                    var model = JsonConvert.DeserializeObject<User>(json);
                    leaderBoards.Add(new LeaderBoardModel
                    {
                        Name = model.Name,
                        TopScore = model.TopScore,
                        UserId = childSnapshot.Key
                    });

                }
                leaderBoards = leaderBoards.OrderByDescending(a => a.TopScore).ToList();
                foreach (var leaderBoard in leaderBoards)
                {
                    leaderBoard.Order = order;
                    order++;
                }
                for (int i = 0; i < leaderBoards.Count; i++)
                {
                    Entries[i].text = $"{leaderBoards[i].Order}. {leaderBoards[i].Name}";
                    EntriesScore[i].text = $"{leaderBoards[i].TopScore}";
                    if (userId == leaderBoards[i].UserId)
                    {
                        Entries[i].color = Color.red;
                        EntriesScore[i].color = Color.red;
                    }
                }
                for (int i = leaderBoards.Count; i < 10; i++)
                {
                    Entries[i].text = "";
                    EntriesScore[i].text = "";

                }

            }
        });

    }



}
