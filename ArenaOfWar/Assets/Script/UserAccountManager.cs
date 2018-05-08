using System.Collections;
using UnityEngine;
using DatabaseControl;
using UnityEngine.SceneManagement;

public class UserAccountManager : MonoBehaviour
{

    public static UserAccountManager instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(this);
    }

    public static string playerUsername { get; protected set; }
    public static string playerPassword { get; protected set; }

    public static string LoggedIn_data { get; protected set; }

    public static bool isLoggedIn { get; protected set; }

    public string loggedInSceneName = "Join";
    public string loggedOutSceneName = "LoginMenu";

    public void LogOut()
    {
        playerUsername = "";
        playerPassword = "";
        isLoggedIn = false;
        SceneManager.LoadScene(loggedOutSceneName);
    }

    public void LogIn(string _Username, string _Password)
    {
        playerUsername = _Username;
        playerPassword = _Password;
        isLoggedIn = true;
        SceneManager.LoadScene(loggedInSceneName);
    }

    public void SendData(string data)
    {
        if (isLoggedIn)
            StartCoroutine(SetData(data));
    }

    IEnumerator SetData(string data)
    {
        IEnumerator e = DCF.SetUserData(playerUsername, playerPassword, data); // << Send request to set the player's data string. Provides the username, password and new data string
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
            //The data string was set correctly. Goes back to LoggedIn UI
            Debug.Log("Succes sending data");
            // loggedInParent.gameObject.SetActive(true);
        }
        else
        {
            Debug.Log("Error: Unknown Error. Please try again later. Send data problem");
        }
    }

    public void GetData()
    {
        //Called when the player hits 'Get Data' to retrieve the data string on their account. Switches UI to 'Loading...' and starts coroutine to get the players data string from the server
        if (isLoggedIn)
            StartCoroutine(GetData_numerator());
    }

    IEnumerator GetData_numerator()
    {
        string data = "ERROR";
        IEnumerator e = DCF.GetUserData(playerUsername, playerPassword); // << Send request to get the player's data string. Provides the username and password
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Error")
        {
            Debug.Log("Error: Unknown Error. Please try again later. GetDataProblem");
        }
        else
        {
            //The player's data was retrieved. Goes back to loggedIn UI and displays the retrieved data in the InputField
            data = response;
        }

        LoggedIn_data = data;
    }
}