using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    [TextArea(2, 4)]
    public string questionText;

    [Header("Answers")]
    public string correctAnswer;
    public string wrongAnswer1;
    public string wrongAnswer2;
    public string wrongAnswer3;
}

public class QuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI QuestionTextDisplay;
    public GameObject[] AnswerButtons;
    public TextMeshProUGUI CheckerText;
    public TextMeshProUGUI ScoreText;

    [Header("Wrong Answer UI")]
    public GameObject WrongUI;
    public float wrongUIDuration = 1f;

    [Header("Question Goal Settings")]
    public int requiredCorrectAnswers = 5;

    [Header("Question Pool")]
    public List<QuestionData> questionPool;

    [Header("Lock & Door References")]
    public GameObject padlockObject;
    public MonoBehaviour doorOpenerScript;
    public MonoBehaviour PlayerMovement;

    [Header("Camera Bob References")]
    public CameraBob mainCameraBob;
    public CameraBob itemCameraBob;

    private QuestionData currentQuestion;
    private List<string> shuffledAnswers = new List<string>();

    private int correctAnswersCount = 0;
    private bool hasQuestion = false;
    private bool isCheckingAnswer = false;

    private void OnEnable()
    {
        // Disable player movement
        DisablePlayerMovement();

        // Disable both camera bob scripts
        DisableCameraBobs();

        if (WrongUI != null)
            WrongUI.SetActive(false);

        if (ScoreText != null)
        {
            ScoreText.text =
                "Progress: " +
                correctAnswersCount +
                " / " +
                requiredCorrectAnswers;
        }

        if (CheckerText != null)
            CheckerText.text = "";

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        if (!hasQuestion)
        {
            DisplayNextQuestion();
        }
    }

    private void DisablePlayerMovement()
    {
        if (PlayerMovement != null)
        {
            PlayerMovement.enabled = false;
        }
    }

    private void DisableCameraBobs()
    {
        if (mainCameraBob != null)
        {
            mainCameraBob.enabled = false;
        }

        if (itemCameraBob != null)
        {
            itemCameraBob.enabled = false;
        }
    }

    private void EnableCameraBobs()
    {
        if (mainCameraBob != null)
        {
            mainCameraBob.enabled = true;
        }

        if (itemCameraBob != null)
        {
            itemCameraBob.enabled = true;
        }
    }

    public void DisplayNextQuestion()
    {
        if (questionPool == null || questionPool.Count == 0)
        {
            if (QuestionTextDisplay != null)
                QuestionTextDisplay.text = "No questions remaining!";

            return;
        }

        hasQuestion = true;

        if (ScoreText != null)
        {
            ScoreText.text =
                "Progress: " +
                correctAnswersCount +
                " / " +
                requiredCorrectAnswers;
        }

        int randomIndex = Random.Range(0, questionPool.Count);
        currentQuestion = questionPool[randomIndex];

        if (QuestionTextDisplay != null)
            QuestionTextDisplay.text = currentQuestion.questionText;

        shuffledAnswers.Clear();

        shuffledAnswers.Add(currentQuestion.correctAnswer);
        shuffledAnswers.Add(currentQuestion.wrongAnswer1);
        shuffledAnswers.Add(currentQuestion.wrongAnswer2);
        shuffledAnswers.Add(currentQuestion.wrongAnswer3);

        // Shuffle answers
        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            string temp = shuffledAnswers[i];

            int randIndex =
                Random.Range(i, shuffledAnswers.Count);

            shuffledAnswers[i] =
                shuffledAnswers[randIndex];

            shuffledAnswers[randIndex] = temp;
        }

        // Update buttons
        for (int i = 0; i < AnswerButtons.Length; i++)
        {
            if (i < shuffledAnswers.Count)
            {
                AnswerButtons[i].SetActive(true);

                TextMeshProUGUI buttonText =
                    AnswerButtons[i]
                    .GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                {
                    buttonText.text = shuffledAnswers[i];
                }
            }
            else
            {
                AnswerButtons[i].SetActive(false);
            }
        }
    }

    public void SelectAnswer(int buttonIndex)
    {
        if (isCheckingAnswer)
            return;

        if (currentQuestion == null)
            return;

        if (buttonIndex < 0 ||
            buttonIndex >= shuffledAnswers.Count)
            return;

        string selectedText =
            shuffledAnswers[buttonIndex];

        // Correct answer
        if (selectedText == currentQuestion.correctAnswer)
        {
            correctAnswersCount++;

            if (CheckerText != null)
                CheckerText.text = "<color=#00FF00>Correct!</color>";

            if (ScoreText != null)
            {
                ScoreText.text =
                    "Progress: " +
                    correctAnswersCount +
                    " / " +
                    requiredCorrectAnswers;
            }

            if (correctAnswersCount >= requiredCorrectAnswers)
            {
                StartCoroutine(UnlockAfterFeedback());
            }
            else
            {
                StartCoroutine(NextQuestionAfterFeedback());
            }
        }
        else
        {
            // Wrong answer
            StartCoroutine(WrongAnswerFeedback());
        }
    }

    private IEnumerator WrongAnswerFeedback()
    {
        isCheckingAnswer = true;

        if (WrongUI != null)
            WrongUI.SetActive(true);

        if (CheckerText != null)
            CheckerText.text = "";

        yield return new WaitForSeconds(wrongUIDuration);

        if (WrongUI != null)
            WrongUI.SetActive(false);

        isCheckingAnswer = false;

        DisplayNextQuestion();
    }

    private IEnumerator NextQuestionAfterFeedback()
    {
        isCheckingAnswer = true;

        yield return new WaitForSeconds(1f);

        if (CheckerText != null)
            CheckerText.text = "";

        isCheckingAnswer = false;

        DisplayNextQuestion();
    }

    private IEnumerator UnlockAfterFeedback()
    {
        isCheckingAnswer = true;

        yield return new WaitForSeconds(1f);

        UnlockAndDestroyLock();
    }

    public void ExitQuiz()
    {
        CloseUI();
    }

    private void UnlockAndDestroyLock()
    {
        // Unlock the door
        if (doorOpenerScript != null)
        {
            OpenDoor door =
                doorOpenerScript as OpenDoor;

            if (door != null)
            {
                door.UnlockDoor();
            }
        }

        // Destroy the padlock
        if (padlockObject != null)
        {
            Destroy(padlockObject);
            padlockObject = null;
        }

        // Close quiz
        CloseUI();
    }

    public void CloseUI()
    {
        if (WrongUI != null)
            WrongUI.SetActive(false);

        // Re-enable player movement
        if (PlayerMovement != null)
        {
            PlayerMovement.enabled = true;
        }

        // Re-enable both camera bob scripts
        EnableCameraBobs();

        // Hide quiz UI
        gameObject.SetActive(false);

        // Lock mouse back to gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}