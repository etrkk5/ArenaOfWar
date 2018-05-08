using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DatabaseControl; // << Remember to add this reference to your scripts which use DatabaseControl

public class LoginMenu : MonoBehaviour
{
    //All public variables bellow are assigned in the Inspector

    //These are the GameObjects which are parents of groups of UI elements. The objects are enabled and disabled to show and hide the UI elements.
    public GameObject loginParent;
    public GameObject registerParent;
    public GameObject loadingParent;

    //These are all the InputFields which we need in order to get the entered usernames, passwords, etc
    public InputField Login_UsernameField;
    public InputField Login_PasswordField;
    public InputField Register_UsernameField;
    public InputField Register_PasswordField;
    public InputField Register_ConfirmPasswordField;

    //These are the UI Texts which display errors
    public Text Login_ErrorText;
    public Text Register_ErrorText;

    //Called at the very start of the game
    void Awake()
    {
        ResetAllUIElements();
    }

    //Called by Button Pressed Methods to Reset UI Fields
    void ResetAllUIElements()
    {
        //This resets all of the UI elements. It clears all the strings in the input fields and any errors being displayed
        Login_UsernameField.text = "";
        Login_PasswordField.text = "";
        Register_UsernameField.text = "";
        Register_PasswordField.text = "";
        Register_ConfirmPasswordField.text = "";
        blankErrors();
    }

    void blankErrors()
    {
        //blanks all error texts when part is changed e.g. login > Register
        Login_ErrorText.text = "";
        Register_ErrorText.text = "";
    }

    //Called by Button Pressed Methods. These use DatabaseControl namespace to communicate with server.
    IEnumerator LoginUser(string username, string password)
    {
        IEnumerator e = DCF.Login(username, password); // << Send request to login, providing username and password
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
            //Username and Password were correct. Stop showing 'Loading...' and show the LoggedIn UI. And set the text to display the username.
            ResetAllUIElements();
            //          loadingParent.gameObject.SetActive(false);
            //			backGround.gameObject.SetActive(false);
            // loggedInParent.gameObject.SetActive(true);
            UserAccountManager.instance.LogIn(username, password);
        }
        else
        {
            //Something went wrong logging in. Stop showing 'Loading...' and go back to LoginUI
            loadingParent.gameObject.SetActive(false);
            loginParent.gameObject.SetActive(true);
            if (response == "UserError")
            {
                //The Username was wrong so display relevent error message
                Login_ErrorText.text = "Error: Username not Found";
            }
            else
            {
                if (response == "PassError")
                {
                    //The Password was wrong so display relevent error message
                    Login_ErrorText.text = "Error: Password Incorrect";
                }
                else
                {
                    //There was another error. This error message should never appear, but is here just in case.
                    Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
                }
            }
        }
    }

    IEnumerator RegisterUser(string username, string password, string data)
    {
        IEnumerator e = DCF.RegisterUser(username, password, data); // << Send request to register a new user, providing submitted username and password. It also provides an initial value for the data string on the account, which is "Hello World".
        while (e.MoveNext())
        {
            yield return e.Current;
        }
        string response = e.Current as string; // << The returned string from the request

        if (response == "Success")
        {
            //Username and Password were valid. Account has been created. Stop showing 'Loading...' and show the loggedIn UI and set text to display the username.
            ResetAllUIElements();
            //loadingParent.gameObject.SetActive(false);
            UserAccountManager.instance.LogIn(username, password);
        }
        else
        {
            //Something went wrong logging in. Stop showing 'Loading...' and go back to RegisterUI
            loadingParent.gameObject.SetActive(false);
            registerParent.gameObject.SetActive(true);
            if (response == "UserError")
            {
                //The username has already been taken. Player needs to choose another. Shows error message.
                Register_ErrorText.text = "Error: Username Already Taken";
            }
            else
            {
                //There was another error. This error message should never appear, but is here just in case.
                Login_ErrorText.text = "Error: Unknown Error. Please try again later.";
            }
        }
    }

    //UI Button Pressed Methods
    public void Login_LoginButtonPressed()
    {
        //Called when player presses button to Login

        //Get the username and password the player entered
        string username = Login_UsernameField.text;
        string pass = Login_PasswordField.text;
        //Check the lengths of the username and password. (If they are wrong, we might as well show an error now instead of waiting for the request to the server)
        if (username.Length > 3)
        {
            if (pass.Length > 5)
            {
                //Username and password seem reasonable. Change UI to 'Loading...'. Start the Coroutine which tries to log the player in.
                loginParent.gameObject.SetActive(false);
                loadingParent.gameObject.SetActive(true);
                StartCoroutine(LoginUser(username, pass));
            }
            else
            {
                //Password too short so it must be wrong
                Login_ErrorText.text = "Error: Password format Incorrect (length must be > 5)";
            }
        }
        else if (username.Length == 0 && pass.Length == 0)
            Login_ErrorText.text = "Error : Blank Field, Try again please.";
        else
        {
            //Username too short so it must be wrong
            Login_ErrorText.text = "Error: Username format Incorrect (length must be > 3)";
        }
    }

    public void Login_RegisterButtonPressed() //QUAND LE MEC APPUIE SUR LE BOUTON REGISTER(qui n'estpas actif de base, pour ouvrir les elements)
    {
        //Called when the player hits register on the Login UI, so switches to the Register UI
        ResetAllUIElements();
        loginParent.gameObject.SetActive(false);
        registerParent.gameObject.SetActive(true);
    }

    public void Register_RegisterButtonPressed() //APRES AVOIR REMPLIT LES 3 champs USER PASS CONFIRM ...
    {
        //Called when the player presses the button to register

        //Get the username and password and repeated password the player entered
        string username = Register_UsernameField.text;
        string password = Register_PasswordField.text;
        string confirmedPassword = Register_ConfirmPasswordField.text;

        //Make sure username and password are long enough
        if (username.Length > 3)
        {
            if (password.Length > 5)
            {
                //Check the two passwords entered match
                if (password == confirmedPassword)
                {
                    //Username and passwords seem reasonable. Switch to 'Loading...' and start the coroutine to try and register an account on the server
                    registerParent.gameObject.SetActive(false);
                    loadingParent.gameObject.SetActive(true);
                    StartCoroutine(RegisterUser(username, password, "[KILLS]0/[DEATHS]0"));
                }
                else
                {
                    //Passwords don't match, show error
                    Register_ErrorText.text = "Error: Password's don't Match";
                }
            }
            else
            {
                //Password too short so show error
                Register_ErrorText.text = "Error: Password too Short";
            }
        }
        else
        {
            //Username too short so show error
            Register_ErrorText.text = "Error: Username too Short";
        }
    }

    public void Register_BackButtonPressed() //PAS ENVIE DE MENREGISTRER LOL, DONC REVIENS EN ARRIERE
    {
        //Called when the player presses the 'Back' button on the register UI. Switches back to the Login UI
        ResetAllUIElements();
        loginParent.gameObject.SetActive(true);
        registerParent.gameObject.SetActive(false);
    }

    public void LoggedIn_LogoutButtonPressed() // A METTRE UN BOUTON LOGOUT SUR LE MENU PRINCIPALE
    {
        //Called when the player hits the 'Logout' button. Switches back to Login UI and forgets the player's username and password.
        //Note: Database Control doesn't use sessions, so no request to the server is needed here to end a session.
        ResetAllUIElements();
        UserAccountManager.instance.LogOut();
        loginParent.gameObject.SetActive(true);
        //SceneManagement.LoadScene(0);
        // loggedInParent.gameObject.SetActive(false);
    }
}