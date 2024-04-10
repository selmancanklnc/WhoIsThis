using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using Firebase.Auth;
public class SignIn : MonoBehaviour
{
    DatabaseReference reference;
    public GameObject netControlPanel;


    private bool netcontrolconnected = true;

    // Start is called before the first frame update
    public void MoveToScene(int sceneId)
    {
        SceneManager.LoadScene(sceneId);
    }

    void Start()
    {

        reference = FirebaseDatabase.DefaultInstance.RootReference;
        Login();


    }

    void Login()
    {
        // Android cihazdaysak Android butonunu etkinleþtir
        if (Application.platform == RuntimePlatform.Android)
        {
#if UNITY_ANDROID
            PlayGamesClientConfiguration config = new PlayGamesClientConfiguration.Builder().RequestServerAuthCode(false /* Don't force refresh */).Build();
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.Activate();
            // Otomatik oturum açma iþlemini baþlatýn
            FirebaseSignIn();
#endif

        }
        // Ios cihazdaysak Ios butonunu etkinleþtir
        else if (Application.platform == RuntimePlatform.IPhonePlayer)
        {

            FirebaseSignIn();

        }

    }


    private void FirebaseSignIn()
    {
        Social.localUser.Authenticate(success =>
        {
            if (success)
            {
                Debug.Log("Google oturum açma baþarýlý.");
                if (Application.platform == RuntimePlatform.Android)
                {
                    FirebaseGoogleSignIn();

                }
                // Ios cihazdaysak Ios butonunu etkinleþtir
                else if (Application.platform == RuntimePlatform.IPhonePlayer)
                {

                    FirebaseSignInIphone();
                }
            }
            else
            {

                Debug.LogError("Google oturum açma baþarýsýz.");
            }
        });
    }

    private void FirebaseGoogleSignIn()
    {
#if UNITY_ANDROID

        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        string idToken = PlayGamesPlatform.Instance.GetServerAuthCode();
        Credential credential = PlayGamesAuthProvider.GetCredential(idToken);
        Debug.Log("token2:" + idToken);
        auth.SignInAndRetrieveDataWithCredentialAsync(credential).ContinueWithOnMainThread(task =>
        {
            if (task.IsCanceled)
            {
                Debug.LogError("Firebase oturum açma iptal edildi.");

                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("Firebase oturum açma baþarýsýz: " + task.Exception);

                return;
            }

            FirebaseUser newUser = task.Result.User;
            Debug.Log("Signed in with Google: " + newUser.DisplayName);
            OnUserSignedIn(newUser);


        });



#endif
    }




    void SaveUserToFirestore(FirebaseUser user)
    {


        reference.Child("users").Child(user.UserId).GetValueAsync().ContinueWithOnMainThread(d =>
        {
            int topScore = 0;
            DataSnapshot snapshot = d.Result;

            if (snapshot?.Value == null)
            {
                Dictionary<string, object> playerData = new Dictionary<string, object>
                            {
                                { "Name", user.DisplayName },
                                { "Email", user.Email },
                                { "TopScore", topScore },
                                { "ImageUrl",  user.PhotoUrl.ToString() }
                            };
                reference.Child("users").Child(user.UserId).SetValueAsync(playerData).ContinueWithOnMainThread(d =>
                {
                    if (d.IsCanceled)
                    {
                        Debug.LogError("Kayýt Ýptal Edildi.");

                        return;
                    }
                    if (d.IsFaulted)
                    {
                        Debug.LogError("Kayýt hatasý: " + d.Exception);

                        return;
                    }
                    SceneManager.LoadScene(1);// ?

                });
            }
            else
            {
                SceneManager.LoadScene(1);
            }


        });


    }

    void OnUserSignedIn(FirebaseUser user)
    {

        SaveUserToFirestore(user);

    }


    private void FirebaseSignInIphone()
    {
        Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

        GameCenterAuthProvider.GetCredentialAsync().ContinueWithOnMainThread(f =>
        {
            auth.SignInAndRetrieveDataWithCredentialAsync(f.Result).ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("Firebase oturum açma iptal edildi.");

                    return;
                }
                if (task.IsFaulted)
                {
                    Debug.LogError("Firebase oturum açma baþarýsýz: " + task.Exception);

                    return;
                }

                FirebaseUser newUser = task.Result.User;
                Debug.Log("Signed in with Google: " + newUser.DisplayName);
                OnUserSignedIn(newUser);

            });
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
            netcontrolconnected = false;
        }
        else
        {
            if (netControlPanel.gameObject.activeSelf)
            {
                netControlPanel.SetActive(false);
            }
            AudioListener.volume = 1;
            if (!netcontrolconnected)
            {
                Login();
            }
            netcontrolconnected = true;

        }

    }


    void Update()
    {
        NetControl();


    }

}
