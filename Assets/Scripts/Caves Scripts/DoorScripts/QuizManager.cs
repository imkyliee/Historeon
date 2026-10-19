
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
    public static QuizManager Current { get; private set; }

    public bool IsQuizOpen { get; private set; }

    [Header("UI Elements")]
    public TextMeshProUGUI QuestionTextDisplay;
    public GameObject[] AnswerButtons;
    public TextMeshProUGUI CheckerText;
    public TextMeshProUGUI ScoreText;

    [Header("Player Animation")]
    public PlayerAnimation playerAnimation;

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
    public PadlockInspector padlockInspector;

    private QuestionData currentQuestion;
    private List<string> shuffledAnswers = new List<string>();

    private int correctAnswersCount = 0;
    private bool hasQuestion = false;
    private bool isCheckingAnswer = false;

    public void StartQuiz()
    {
        if (Movement.Instance != null)
            Movement.Instance.SetAttackEnabled(false);

        IsQuizOpen = true;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    private void OnEnable()
    {
        Current = this;
        IsQuizOpen = true;

        if (Movement.Instance != null)
            Movement.Instance.SetAttackEnabled(false);

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
            DisplayNextQuestion();
    }

    private void OnDisable()
    {
        if (Current == this)
            Current = null;

        IsQuizOpen = false;
    }

    private void Update()
    {
        if (IsQuizOpen && Input.GetKeyDown(KeyCode.X))
            CloseUI();
    }

    public void DisplayNextQuestion()
    {
        if (questionPool == null || questionPool.Count == 0)
        {
            hasQuestion = false;
            currentQuestion = null;

            FinishQuiz();
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
        if (AnswerButtons == null)
            return;

        for (int i = 0; i < AnswerButtons.Length; i++)
        {
            if (AnswerButtons[i] == null)
                continue;

            if (i < shuffledAnswers.Count)
            {
                AnswerButtons[i].SetActive(true);

                TextMeshProUGUI buttonText =
                    AnswerButtons[i]
                    .GetComponentInChildren<TextMeshProUGUI>();

                if (buttonText != null)
                    buttonText.text = shuffledAnswers[i];
            }
            else
            {
                AnswerButtons[i].SetActive(false);
            }
        }
    }

    public void SelectAnswer(int buttonIndex)
    {
        if (!IsQuizOpen || isCheckingAnswer || currentQuestion == null)
            return;

        if (buttonIndex < 0 ||
            buttonIndex >= shuffledAnswers.Count)
            return;

        string selectedText = shuffledAnswers[buttonIndex];

        // Remove the answered question so it cannot repeat
        questionPool.Remove(currentQuestion);

        // Correct answer
        if (selectedText == currentQuestion.correctAnswer)
        {
            isCheckingAnswer = true;
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
                StartCoroutine(UnlockAfterFeedback());
            else
                StartCoroutine(NextQuestionAfterFeedback());
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
                door.UnlockDoor();
        }

        // Destroy the padlock
        if (padlockObject != null)
        {
            Destroy(padlockObject);
            padlockObject = null;
        }

        FinishQuiz();
    }

    
    private void FinishQuiz()
    {
        IsQuizOpen = false;

        if (padlockInspector != null)
            padlockInspector.PlayQuizCompletionSound();

        hasQuestion = false;
        currentQuestion = null;

        if (doorOpenerScript != null)
        {
            OpenDoor door = doorOpenerScript as OpenDoor;

            if (door != null)
                door.UnlockDoor();
        }

        if (padlockObject != null)
        {
            Destroy(padlockObject);
            padlockObject = null;
        }

        if (padlockInspector != null)
            padlockInspector.CloseQuiz();
        else
            CloseUI();

        gameObject.SetActive(false);
    }


    public void CloseUI()
    {
        IsQuizOpen = false;

        if (WrongUI != null)
            WrongUI.SetActive(false);

        // Re-enable player animation and reset the action
        if (playerAnimation != null)
        {
            playerAnimation.enabled = true;
            playerAnimation.ResetActionAfterQuiz();
        }

        // Re-enable attacks
        if (Movement.Instance != null)
            Movement.Instance.SetAttackEnabled(true);

        // Hide quiz UI
        gameObject.SetActive(false);

        // Lock mouse back to gameplay
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}
