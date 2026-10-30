
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;

public class PlayFabAccountManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject registerPanel;
    public GameObject forgotPasswordPanel;
    public GameObject quitPanel;
    public GameObject quitButton;

    [Header("Panel Animators")]
    public Animator loginAnimator;
    public Animator registerAnimator;
    public Animator forgotPasswordAnimator;
    public Animator quitAnimator;

    [Header("Animation Names")]
    public string loginOpen = "Login Open";
    public string loginClose = "Login Close";
    public string registerOpen = "Register Open";
    public string registerClose = "Register Close";
    public string forgotOpen = "ForgotPassword Open";
    public string forgotClose = "ForgotPassword Close";
    public string quitOpen = "Quit Open";
    public string quitClose = "Quit Close";

    [Header("Login")]
    public TMP_InputField loginUsernameInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginErrorText;

    [Header("Register")]
    public TMP_InputField registerUsernameInput;
    public TMP_InputField registerEmailInput;
    public TMP_InputField registerPasswordInput;
    public TMP_InputField registerConfirmPasswordInput;
    public TMP_Text registerErrorText;

    [Header("Forgot Password")]
    public TMP_InputField forgotEmailInput;
    public TMP_Text emailSentText;
    public TMP_Text forgotErrorText;

    [Header("Scene")]
    public string mainMenuSceneName = "Dynamic Main Menu";

    private GameObject currentPanel;
    private GameObject previousPanelBeforeQuit;
    private Coroutine panelTransition;

    private void Start()
    {
        if (loginPanel != null)
            loginPanel.SetActive(false);

        if (registerPanel != null)
            registerPanel.SetActive(false);

        if (forgotPasswordPanel != null)
            forgotPasswordPanel.SetActive(false);

        if (quitPanel != null)
            quitPanel.SetActive(false);

        if (quitButton != null)
            quitButton.SetActive(false);

        currentPanel = null;
        previousPanelBeforeQuit = null;

        HideMessages();
    }

    public void ShowLoginPanel()
    {
        SwitchPanel(loginPanel, loginAnimator, loginOpen);
    }

    public void ShowRegisterPanel()
    {
        SwitchPanel(registerPanel, registerAnimator, registerOpen);
    }

    public void ShowForgotPasswordPanel()
    {
        SwitchPanel(forgotPasswordPanel, forgotPasswordAnimator, forgotOpen);
    }

    public void ShowQuitPanel()
    {
        if (currentPanel == quitPanel)
            return;

        if (panelTransition != null)
            return;

        previousPanelBeforeQuit = currentPanel;

        SwitchPanel(quitPanel, quitAnimator, quitOpen);
    }

    public void CloseQuitPanel()
    {
        if (panelTransition != null)
            return;

        GameObject targetPanel = previousPanelBeforeQuit;

        if (targetPanel == null)
            targetPanel = loginPanel;

        Animator targetAnimator = GetPanelAnimator(targetPanel);
        string openAnimation = GetOpenAnimation(targetPanel);

        SwitchPanel(targetPanel, targetAnimator, openAnimation);

        previousPanelBeforeQuit = null;
    }

    public void ConfirmQuit()
    {
        Debug.Log("Quitting Historeon...");
        Application.Quit();
    }

    public void ShowLoginPanelInstant()
    {
        if (panelTransition != null)
        {
            StopCoroutine(panelTransition);
            panelTransition = null;
        }

        if (loginPanel != null)
            loginPanel.SetActive(true);

        if (registerPanel != null)
            registerPanel.SetActive(false);

        if (forgotPasswordPanel != null)
            forgotPasswordPanel.SetActive(false);

        if (quitPanel != null)
            quitPanel.SetActive(false);

        if (quitButton != null)
            quitButton.SetActive(true);

        currentPanel = loginPanel;
        previousPanelBeforeQuit = null;

        HideMessages();
    }

    private void SwitchPanel(
        GameObject targetPanel,
        Animator targetAnimator,
        string openAnimation)
    {
        if (targetPanel == null)
            return;

        if (currentPanel == targetPanel && panelTransition == null)
            return;

        if (panelTransition != null)
            StopCoroutine(panelTransition);

        panelTransition = StartCoroutine(
            SwitchPanelSequence(targetPanel, targetAnimator, openAnimation)
        );
    }

    private IEnumerator SwitchPanelSequence(
        GameObject targetPanel,
        Animator targetAnimator,
        string openAnimation)
    {
        GameObject previous = currentPanel;

        Animator previousAnimator = GetPanelAnimator(previous);
        string closeAnimation = GetCloseAnimation(previous);

        if (previous != null && previous != targetPanel)
        {
            if (previousAnimator != null &&
                !string.IsNullOrEmpty(closeAnimation))
            {
                previousAnimator.Play(closeAnimation, 0, 0f);

                yield return null;

                yield return new WaitUntil(() =>
                    previousAnimator.GetCurrentAnimatorStateInfo(0)
                    .IsName(closeAnimation));

                yield return new WaitUntil(() =>
                    previousAnimator.GetCurrentAnimatorStateInfo(0)
                    .normalizedTime >= 1f);
            }

            previous.SetActive(false);
        }

        if (loginPanel != null && loginPanel != targetPanel)
            loginPanel.SetActive(false);

        if (registerPanel != null && registerPanel != targetPanel)
            registerPanel.SetActive(false);

        if (forgotPasswordPanel != null &&
            forgotPasswordPanel != targetPanel)
            forgotPasswordPanel.SetActive(false);

        if (quitPanel != null && quitPanel != targetPanel)
            quitPanel.SetActive(false);

        targetPanel.SetActive(true);

        currentPanel = targetPanel;

        if (quitButton != null)
            quitButton.SetActive(targetPanel != quitPanel);

        HideMessages();

        if (targetAnimator != null &&
            !string.IsNullOrEmpty(openAnimation))
        {
            targetAnimator.Play(openAnimation, 0, 0f);
        }

        panelTransition = null;
    }

    private Animator GetPanelAnimator(GameObject panel)
    {
        if (panel == loginPanel)
            return loginAnimator;

        if (panel == registerPanel)
            return registerAnimator;

        if (panel == forgotPasswordPanel)
            return forgotPasswordAnimator;

        if (panel == quitPanel)
            return quitAnimator;

        return null;
    }

    private string GetCloseAnimation(GameObject panel)
    {
        if (panel == loginPanel)
            return loginClose;

        if (panel == registerPanel)
            return registerClose;

        if (panel == forgotPasswordPanel)
            return forgotClose;

        if (panel == quitPanel)
            return quitClose;

        return null;
    }

    private string GetOpenAnimation(GameObject panel)
    {
        if (panel == loginPanel)
            return loginOpen;

        if (panel == registerPanel)
            return registerOpen;

        if (panel == forgotPasswordPanel)
            return forgotOpen;

        if (panel == quitPanel)
            return quitOpen;

        return null;
    }

    public void Login()
    {
        string username = loginUsernameInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(username) ||
            string.IsNullOrEmpty(password))
        {
            ShowLoginError("Please enter your username and password.");
            return;
        }

        if (password.Length < 6)
        {
            ShowLoginError("Password must be at least 6 characters.");
            return;
        }

        if (password.Length > 100)
        {
            ShowLoginError("Password must not exceed 100 characters.");
            return;
        }

        HideMessages();

        LoginWithPlayFabRequest request = new LoginWithPlayFabRequest
        {
            Username = username,
            Password = password
        };

        PlayFabClientAPI.LoginWithPlayFab(
            request,
            OnLoginSuccess,
            OnLoginFailure
        );
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log("PlayFab Login Successful!");
        Debug.Log("PlayFab ID: " + result.PlayFabId);
        Debug.Log("LOGIN SUCCESS TIME: " + System.DateTime.Now);
        Debug.Log("PLAYFAB LAST LOGIN: " + result.LastLoginTime);

        PlayerPrefs.SetString("Username", loginUsernameInput.text.Trim());
        PlayerPrefs.SetString("PlayFabId", result.PlayFabId);
        PlayerPrefs.Save();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidUsernameOrPassword:
                ShowLoginError("Incorrect username or password.");
                break;

            case PlayFabErrorCode.InvalidParams:
                ShowLoginError("Please check your username and password.");
                break;

            case PlayFabErrorCode.AccountNotFound:
                ShowLoginError("Incorrect username or password.");
                break;

            case PlayFabErrorCode.FailedLoginAttemptRateLimitExceeded:
                ShowLoginError("Too many login attempts. Please try again later.");
                break;

            default:
                ShowLoginError("Unable to log in. Please try again.");
                break;
        }
    }

    public void Register()
    {
        string displayName = registerUsernameInput.text.Trim();
        string username = displayName.Replace(" ", "");
        string email = registerEmailInput.text.Trim();
        string password = registerPasswordInput.text;
        string confirmPassword = registerConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(username))
        {
            ShowRegisterError("Please enter a username.");
            return;
        }

        if (string.IsNullOrEmpty(email))
        {
            ShowRegisterError("Please enter your email.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowRegisterError("Please enter a password.");
            return;
        }

        if (password.Length < 6)
        {
            ShowRegisterError("Password must be at least 6 characters.");
            return;
        }

        if (password.Length > 100)
        {
            ShowRegisterError("Password must not exceed 100 characters.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowRegisterError("Passwords do not match.");
            return;
        }

        HideMessages();

        RegisterPlayFabUserRequest request = new RegisterPlayFabUserRequest
        {
            Username = username,
            Email = email,
            Password = password,
            DisplayName = displayName,
            RequireBothUsernameAndEmail = false
        };

        PlayFabClientAPI.RegisterPlayFabUser(
            request,
            OnRegisterSuccess,
            OnRegisterFailure
        );
    }

    private void OnRegisterSuccess(RegisterPlayFabUserResult result)
    {
        Debug.Log("PlayFab Registration Successful!");
        Debug.Log("PlayFab ID: " + result.PlayFabId);

        loginUsernameInput.text =
            registerUsernameInput.text.Trim().Replace(" ", "");

        loginPasswordInput.text = "";

        ShowLoginPanel();

        ShowLoginError("Registration successful! Please log in.");
    }

    private void OnRegisterFailure(PlayFabError error)
    {
        ShowRegisterError(GetFriendlyRegisterError(error));
    }

    public void SendPasswordReset()
    {
        string email = forgotEmailInput.text.Trim();

        if (string.IsNullOrEmpty(email))
        {
            ShowForgotError("Please enter your email.");
            return;
        }

        HideMessages();

        SendAccountRecoveryEmailRequest request =
            new SendAccountRecoveryEmailRequest
            {
                Email = email,
                TitleId = PlayFabSettings.staticSettings.TitleId
            };

        PlayFabClientAPI.SendAccountRecoveryEmail(
            request,
            OnPasswordResetSuccess,
            OnPasswordResetFailure
        );
    }

    private void OnPasswordResetSuccess(
        SendAccountRecoveryEmailResult result)
    {
        Debug.Log("Password recovery email sent.");

        if (emailSentText != null)
        {
            emailSentText.text =
                "Email sent. Please check your inbox.";

            emailSentText.gameObject.SetActive(true);
        }
    }

    private void OnPasswordResetFailure(PlayFabError error)
    {
        if (error.Error == PlayFabErrorCode.InvalidEmailAddress)
        {
            ShowForgotError("Please enter a valid email address.");
        }
        else
        {
            ShowForgotError(
                "Unable to send email. Please check your email address."
            );
        }
    }

    private string GetFriendlyRegisterError(PlayFabError error)
    {
        if (error == null)
            return "Something went wrong.";

        switch (error.Error)
        {
            case PlayFabErrorCode.InvalidParams:
                return "Please check your information.";

            case PlayFabErrorCode.UsernameNotAvailable:
                return "That username is already taken.";

            case PlayFabErrorCode.EmailAddressNotAvailable:
                return "That email is already registered.";

            case PlayFabErrorCode.InvalidEmailAddress:
                return "Please enter a valid email address.";

            case PlayFabErrorCode.AccountNotFound:
                return "Account not found.";

            default:
                return "Unable to create account. Please try again.";
        }
    }

    private void ShowLoginError(string message)
    {
        if (loginErrorText == null)
            return;

        loginErrorText.text = message;
        loginErrorText.gameObject.SetActive(true);
    }

    private void ShowRegisterError(string message)
    {
        if (registerErrorText == null)
            return;

        registerErrorText.text = message;
        registerErrorText.gameObject.SetActive(true);
    }

    private void ShowForgotError(string message)
    {
        if (forgotErrorText == null)
            return;

        forgotErrorText.text = message;
        forgotErrorText.gameObject.SetActive(true);
    }

    private void HideMessages()
    {
        if (loginErrorText != null)
            loginErrorText.gameObject.SetActive(false);

        if (registerErrorText != null)
            registerErrorText.gameObject.SetActive(false);

        if (emailSentText != null)
            emailSentText.gameObject.SetActive(false);

        if (forgotErrorText != null)
            forgotErrorText.gameObject.SetActive(false);
    }
}
