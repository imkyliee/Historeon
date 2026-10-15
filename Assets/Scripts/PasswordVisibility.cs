using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PasswordVisibility : MonoBehaviour
{
    public TMP_InputField passwordInput;
    public Button viewButton;
    public Button hideButton;

    private bool isPasswordVisible = false;

    private void Start()
    {
        if (passwordInput == null)
            passwordInput = GetComponent<TMP_InputField>();

        if (viewButton != null)
            viewButton.onClick.AddListener(ShowPassword);

        if (hideButton != null)
            hideButton.onClick.AddListener(HidePassword);

        HidePassword();
    }

    private void ShowPassword()
    {
        isPasswordVisible = true;
        UpdatePasswordVisibility();
    }

    private void HidePassword()
    {
        isPasswordVisible = false;
        UpdatePasswordVisibility();
    }

    private void UpdatePasswordVisibility()
    {
        if (passwordInput == null)
            return;

        passwordInput.contentType = isPasswordVisible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        passwordInput.ForceLabelUpdate();

        if (viewButton != null)
            viewButton.gameObject.SetActive(!isPasswordVisible);

        if (hideButton != null)
            hideButton.gameObject.SetActive(isPasswordVisible);
    }

    private void OnDestroy()
    {
        if (viewButton != null)
            viewButton.onClick.RemoveListener(ShowPassword);

        if (hideButton != null)
            hideButton.onClick.RemoveListener(HidePassword);
    }
}