
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
    public Animator verificationAnimator;

    [Header("Animation Names")]
    public string loginOpen = "Login Open";
    public string loginClose = "Login Close";
    public string registerOpen = "Register Open";
    public string registerClose = "Register Close";
    public string forgotOpen = "ForgotPassword Open";
    public string forgotClose = "ForgotPassword Close";
    public string quitOpen = "Quit Open";
    public string quitClose = "Quit Close";
    public string verificationOpen = "Verification Open";
    public string verificationClose = "Verification Close";

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

    [Header("Email Verification")]
    public GameObject verificationPanel;
    public TMP_Text verificationEmailText;
    public TMP_Text verificationSentText;
    public TMP_Text verificationErrorText;

    [Header("Scene")]
    public string mainMenuSceneName = "Dynamic Main Menu";

    private GameObject currentPanel;
    private GameObject previousPanelBeforeQuit;
    private Coroutine panelTransition;

    private string pendingRegistrationUsername;
    private string pendingRegistrationEmail;
    private string pendingRegistrationPassword;
    private string verificationEmail;
    private string currentPlayFabId;
    private bool isRegistrationLogin;
    private bool isCheckingVerification;
    private bool isResendingVerification;
    private float nextResendAllowedTime;
    private const float ResendCooldownSeconds = 60f;

    private void Start()
    {
        if (loginPanel != null)
            loginPanel.SetActive(false);

        if (registerPanel != null)
            registerPanel.SetActive(false);

        if (forgotPasswordPanel != null)
            forgotPasswordPanel.SetActive(false);

        if (verificationPanel != null)
            verificationPanel.SetActive(false);

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

    public void ShowVerificationPanel()
    {
        if (verificationPanel == null)
        {
            Debug.LogError("Verification Panel is not assigned.");
            return;
        }

        SwitchPanel(verificationPanel, verificationAnimator, verificationOpen);
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

        if (verificationPanel != null)
            verificationPanel.SetActive(false);

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

        if (verificationPanel != null &&
            verificationPanel != targetPanel)
            verificationPanel.SetActive(false);

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

        if (panel == verificationPanel)
            return verificationAnimator;

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

        if (panel == verificationPanel)
            return verificationClose;

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

        if (panel == verificationPanel)
            return verificationOpen;

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

        currentPlayFabId = result.PlayFabId;

        if (isRegistrationLogin)
        {
            isRegistrationLogin = false;
            AddContactEmailToPlayer(pendingRegistrationEmail);
            return;
        }

        CheckVerificationStatus();
    }

    private void CompleteLogin()
    {
        string username = loginUsernameInput != null
            ? loginUsernameInput.text.Trim()
            : pendingRegistrationUsername;

        if (string.IsNullOrEmpty(username))
            username = pendingRegistrationUsername;

        PlayerPrefs.SetString("Username", username);
        PlayerPrefs.SetString("PlayFabId", currentPlayFabId);
        PlayerPrefs.Save();

        SceneManager.LoadScene(mainMenuSceneName);
    }

    public void CheckVerificationStatus()
    {
        if (isCheckingVerification)
            return;

        if (string.IsNullOrEmpty(currentPlayFabId))
        {
            ShowVerificationPanel();
            ShowVerificationError("Please log in again to check your email.");
            return;
        }

        isCheckingVerification = true;

        GetPlayerProfileRequest request = new GetPlayerProfileRequest
        {
            PlayFabId = currentPlayFabId,
            ProfileConstraints = new PlayerProfileViewConstraints
            {
                ShowContactEmailAddresses = true
            }
        };

        PlayFabClientAPI.GetPlayerProfile(
            request,
            OnVerificationProfileReceived,
            OnVerificationProfileFailure
        );
    }

    private void OnVerificationProfileReceived(GetPlayerProfileResult result)
    {
        isCheckingVerification = false;

        if (result.PlayerProfile == null ||
            result.PlayerProfile.ContactEmailAddresses == null ||
            result.PlayerProfile.ContactEmailAddresses.Count == 0)
        {
            GetAccountInfoRequest accountRequest = new GetAccountInfoRequest();

            PlayFabClientAPI.GetAccountInfo(
                accountRequest,
                OnAccountInfoForContactEmailReceived,
                OnAccountInfoForContactEmailFailure
            );
            return;
        }

        var contactEmail = result.PlayerProfile.ContactEmailAddresses[0];
        verificationEmail = contactEmail.EmailAddress;

        if (verificationEmailText != null)
            verificationEmailText.text = verificationEmail;

        if (contactEmail.VerificationStatus.ToString() == "Confirmed")
        {
            CompleteLogin();
        }
        else
        {
            ShowVerificationPanel();
            ShowVerificationError("Your email is not verified yet. Please check your inbox and click the verification link.");
        }
    }

    private void OnAccountInfoForContactEmailReceived(GetAccountInfoResult result)
    {
        string accountEmail = result.AccountInfo != null &&
                             result.AccountInfo.PrivateInfo != null
            ? result.AccountInfo.PrivateInfo.Email
            : null;

        if (string.IsNullOrWhiteSpace(accountEmail))
        {
            ShowVerificationPanel();
            ShowVerificationError("No email address is associated with this account. Please contact support.");
            return;
        }

        AddContactEmailToPlayer(accountEmail);
    }

    private void OnAccountInfoForContactEmailFailure(PlayFabError error)
    {
        ShowVerificationPanel();
        ShowVerificationError("Unable to retrieve your account email. Please try again.");
        Debug.LogError(error.GenerateErrorReport());
    }

    private void OnVerificationProfileFailure(PlayFabError error)
    {
        isCheckingVerification = false;
        ShowVerificationPanel();
        ShowVerificationError("Unable to check verification status. Please try again.");
        Debug.LogError(error.GenerateErrorReport());
    }

    private void AddContactEmailToPlayer(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            ShowVerificationPanel();
            ShowVerificationError("Your email address is missing. Please contact support.");
            return;
        }

        verificationEmail = email;

        Debug.Log("Adding contact email to PlayFab: " + email);

        AddOrUpdateContactEmailRequest request = new AddOrUpdateContactEmailRequest
        {
            EmailAddress = email
        };

        PlayFabClientAPI.AddOrUpdateContactEmail(
            request,
            OnContactEmailAdded,
            OnContactEmailAddFailure
        );
    }

    private void OnContactEmailAdded(AddOrUpdateContactEmailResult result)
    {
        Debug.Log("CONTACT EMAIL ADDED SUCCESSFULLY!");
        Debug.Log("PlayFab accepted the contact email request. Check the email rule for delivery.");

        if (verificationEmailText != null)
            verificationEmailText.text = verificationEmail;

        ShowVerificationPanel();
        ShowVerificationSentMessage("Please check your inbox and spam folder for the verification email.");
    }

    private void OnContactEmailAddFailure(PlayFabError error)
    {
        Debug.LogError("CONTACT EMAIL ERROR: " + error.GenerateErrorReport());

        ShowVerificationPanel();
        ShowVerificationError("Unable to send the verification email. Please try again.");
    }

    public void ResendVerificationEmail()
    {
        if (isResendingVerification)
            return;

        if (Time.unscaledTime < nextResendAllowedTime)
        {
            int secondsLeft = Mathf.CeilToInt(nextResendAllowedTime - Time.unscaledTime);
            ShowVerificationError("Please wait " + secondsLeft + " seconds before requesting another email.");
            return;
        }

        if (string.IsNullOrWhiteSpace(verificationEmail))
        {
            ShowVerificationError("Email address not found. Please log in again.");
            return;
        }

        isResendingVerification = true;

        AddOrUpdateContactEmailRequest request = new AddOrUpdateContactEmailRequest
        {
            EmailAddress = verificationEmail
        };

        PlayFabClientAPI.AddOrUpdateContactEmail(
            request,
            OnVerificationResent,
            OnVerificationResendFailure
        );
    }

    private void OnVerificationResent(AddOrUpdateContactEmailResult result)
    {
        isResendingVerification = false;
        nextResendAllowedTime = Time.unscaledTime + ResendCooldownSeconds;
        ShowVerificationSentMessage("Verification email resent! Please check your inbox and spam folder.");
    }

    private void OnVerificationResendFailure(PlayFabError error)
    {
        isResendingVerification = false;
        ShowVerificationError("Unable to resend the verification email. Please try again later.");
        Debug.LogError(error.GenerateErrorReport());
    }

    private void ShowVerificationSentMessage(string message)
    {
        if (verificationSentText != null)
        {
            verificationSentText.text = message;
            verificationSentText.gameObject.SetActive(true);
        }

        if (verificationErrorText != null)
            verificationErrorText.gameObject.SetActive(false);
    }

    private void ShowVerificationError(string message)
    {
        if (verificationSentText != null)
            verificationSentText.gameObject.SetActive(false);

        if (verificationErrorText == null)
        {
            Debug.LogWarning(message);
            return;
        }

        verificationErrorText.text = message;
        verificationErrorText.gameObject.SetActive(true);
    }

    private void OnLoginFailure(PlayFabError error)
    {
        if (isRegistrationLogin)
        {
            isRegistrationLogin = false;
            ShowRegisterError("Your account was created, but automatic sign-in failed. Please log in manually.");
            Debug.LogError(error.GenerateErrorReport());
            return;
        }

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

        // 1. Length check (8 characters minimum)
        if (password.Length < 8)
        {
            ShowRegisterError("Password must be at least 8 characters.");
            return;
        }

        if (password.Length > 100)
        {
            ShowRegisterError("Password must not exceed 100 characters.");
            return;
        }

        // 2. Uppercase check
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[A-Z]"))
        {
            ShowRegisterError("Password must contain (uppercase letter, Number, Special Character.)");
            return;
        }

        // 3. Number check
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, "[0-9]"))
        {
            ShowRegisterError("\"Password must contain (uppercase letter, Number, Special Character.)");
            return;
        }

        // 4. Special character check
        if (!System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]"))
        {
            ShowRegisterError("\"Password must contain (uppercase letter, Number, Special Character.)");
            return;
        }

        // 5. Confirm password check
        if (password != confirmPassword)
        {
            ShowRegisterError("Passwords do not match.");
            return;
        }

        HideMessages();

        pendingRegistrationUsername = username;
        pendingRegistrationEmail = email;
        pendingRegistrationPassword = password;

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

        if (loginUsernameInput != null)
            loginUsernameInput.text = pendingRegistrationUsername;

        if (loginPasswordInput != null)
            loginPasswordInput.text = "";

        isRegistrationLogin = true;

        LoginWithPlayFabRequest loginRequest = new LoginWithPlayFabRequest
        {
            Username = pendingRegistrationUsername,
            Password = pendingRegistrationPassword
        };

        PlayFabClientAPI.LoginWithPlayFab(
            loginRequest,
            OnLoginSuccess,
            OnLoginFailure
        );
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

        if (verificationSentText != null)
            verificationSentText.gameObject.SetActive(false);

        if (verificationErrorText != null)
            verificationErrorText.gameObject.SetActive(false);
    }
}
