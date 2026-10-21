
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

    [Header("Timer Settings")]
    public float quizDuration = 45f;
    public TextMeshProUGUI TimerText;

    [Tooltip("Seconds removed from the timer for each wrong answer.")]
    [Min(0f)]
    public float wrongAnswerTimePenalty = 5f;

    [Header("Quiz Points Settings")]
    public int maxPointsPerQuiz = 1000;
    [Tooltip("Text shown after the quiz is completed, displaying this quiz's score and the overall total.")]
    public TextMeshProUGUI CompletionSummaryText;
    [Min(0f)]
    [Tooltip("How long the score summary stays visible before the quiz closes and the padlock unlocks.")]
    public float completionSummaryDuration = 3f;
    [Tooltip("Unique ID for this quiz, e.g. TabonCave_Quiz1. Do not reuse across quizzes.")]
    public string quizID = "Quiz1";

    [Header("Forfeit Confirmation")]
    public GameObject ForfeitConfirmationUI;

    [Header("Player Animation")]
    public PlayerAnimation playerAnimation;

    [Header("Wrong Answer UI")]
    public GameObject WrongUI;
    public float wrongUIDuration = 1f;

    [Header("Completion UI")]
    public GameObject ExitButton;

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
    private bool hasForfeited = false;
    private bool scoreAwarded = false;
    private float timeRemaining;
    private Coroutine timerCoroutine;
    private List<QuestionData> runtimeQuestionPool = new List<QuestionData>();

    public void StartQuiz()
    {
        // Reset the attempt state whenever this quiz is opened.
        correctAnswersCount = 0;
        hasQuestion = false;
        isCheckingAnswer = false;
        scoreAwarded = false;
        currentQuestion = null;
        shuffledAnswers.Clear();
        runtimeQuestionPool = questionPool == null
            ? new List<QuestionData>()
            : new List<QuestionData>(questionPool);

        IsQuizOpen = true;

        if (ExitButton != null)
            ExitButton.SetActive(true);

        if (Movement.Instance != null)
            Movement.Instance.SetAttackEnabled(false);

        hasForfeited = PlayFabSaveManager.Instance != null &&
            PlayFabSaveManager.Instance.IsQuizForfeited(quizID);

        timeRemaining = hasForfeited ? 0f : Mathf.Max(0f, quizDuration);

        // Re-enable the text objects that are hidden during the completion summary.
        if (QuestionTextDisplay != null) QuestionTextDisplay.gameObject.SetActive(true);
        if (CheckerText != null) CheckerText.gameObject.SetActive(true);
        if (ScoreText != null) ScoreText.gameObject.SetActive(true);
        if (TimerText != null) TimerText.gameObject.SetActive(true);
        if (CompletionSummaryText != null) CompletionSummaryText.gameObject.SetActive(false);
        if (ForfeitConfirmationUI != null) ForfeitConfirmationUI.SetActive(false);
        if (WrongUI != null) WrongUI.SetActive(false);

        UpdateTimerText();

        if (timerCoroutine != null)
            StopCoroutine(timerCoroutine);
        timerCoroutine = null;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        DisplayNextQuestion();

        if (!hasForfeited && timeRemaining > 0f)
            timerCoroutine = StartCoroutine(RunTimer());
    }

    private void OnEnable()
    {
        Current = this;
        IsQuizOpen = false;

        if (ForfeitConfirmationUI != null)
            ForfeitConfirmationUI.SetActive(false);
        if (CompletionSummaryText != null)
            CompletionSummaryText.gameObject.SetActive(false);
        if (WrongUI != null)
            WrongUI.SetActive(false);

        // Enabling the GameObject alone must not start the quiz or timer.
    }

    private void OnDisable()
    {
        if (Current == this)
            Current = null;

        IsQuizOpen = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }

    private void Update()
    {
        if (IsQuizOpen && Input.GetKeyDown(KeyCode.X))
            ShowForfeitConfirmation();
    }

    public void DisplayNextQuestion()
    {
        if (runtimeQuestionPool == null || runtimeQuestionPool.Count == 0)
        {
            hasQuestion = false;
            currentQuestion = null;

            // Never unlock the padlock unless the required correct-answer goal was reached.
            if (correctAnswersCount >= requiredCorrectAnswers)
            {
                StartCoroutine(UnlockAfterFeedback());
            }
            else
            {
                if (QuestionTextDisplay != null)
                    QuestionTextDisplay.text = "Quiz incomplete. Please check the question pool and required answers.";
                if (CheckerText != null)
                    CheckerText.text = "You haven't reached the required score yet.";
                isCheckingAnswer = true;
            }
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

        int randomIndex = Random.Range(0, runtimeQuestionPool.Count);
        currentQuestion = runtimeQuestionPool[randomIndex];

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
        runtimeQuestionPool.Remove(currentQuestion);

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
            // Apply the wrong-answer time penalty immediately.
            if (!hasForfeited && timeRemaining > 0f)
            {
                timeRemaining = Mathf.Max(0f, timeRemaining - wrongAnswerTimePenalty);
                UpdateTimerText();

                // Reaching zero forfeits points, but the player can keep answering
                // to progress and unlock the padlock.
                if (timeRemaining <= 0f)
                    ForfeitQuiz();
            }

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

        // Wrong answers can be retried; otherwise a 5-question / 5-correct quiz
        // would become impossible after the first wrong answer.
        if (currentQuestion != null && !runtimeQuestionPool.Contains(currentQuestion))
            runtimeQuestionPool.Add(currentQuestion);

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

        // Show the player's score before closing the quiz and unlocking the padlock.
        yield return StartCoroutine(ShowCompletionSummaryThenUnlock());
    }

    
    private IEnumerator ShowCompletionSummaryThenUnlock()
    {
        isCheckingAnswer = true;

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        // Award the score first so the summary displays the saved total.
        int earnedThisQuiz = AwardQuizPoints();
        int overallTotal = PlayFabSaveManager.Instance != null
            ? PlayFabSaveManager.Instance.GetQuizPoints()
            : earnedThisQuiz;

        if (QuestionTextDisplay != null)
            QuestionTextDisplay.gameObject.SetActive(false);

        if (CheckerText != null)
            CheckerText.gameObject.SetActive(false);

        if (WrongUI != null)
            WrongUI.SetActive(false);

        if (AnswerButtons != null)
        {
            foreach (GameObject answerButton in AnswerButtons)
            {
                if (answerButton != null)
                    answerButton.SetActive(false);
            }
        }

        if (TimerText != null)
            TimerText.gameObject.SetActive(false);

        if (ScoreText != null)
            ScoreText.gameObject.SetActive(false);

        // Hide Exit1 while the completion summary is displayed.
        if (ExitButton != null)
            ExitButton.SetActive(false);

        if (CompletionSummaryText != null)
        {
            CompletionSummaryText.text =
                "QUIZ COMPLETED!\n\n" +
                "Quiz Score: " + earnedThisQuiz +
                "\nTotal Score: " + overallTotal;

            CompletionSummaryText.gameObject.SetActive(true);
            CompletionSummaryText.transform.SetAsLastSibling();
        }
        else
        {
            Debug.LogWarning(
                "QuizManager: Completion Summary Text is not assigned for quiz " + quizID,
                this
            );
        }

        yield return new WaitForSeconds(
            Mathf.Max(0f, completionSummaryDuration)
        );

        UnlockAndDestroyLock();
    }

    public void ExitQuiz()
    {
        ShowForfeitConfirmation();
    }

    public void ShowForfeitConfirmation()
    {
        if (!IsQuizOpen || isCheckingAnswer)
            return;

        if (ForfeitConfirmationUI != null)
            ForfeitConfirmationUI.SetActive(true);
    }

    // Wire this to the Forfeit Quiz button. The quiz continues without timer or points.
   public void ForfeitQuiz()
    {
        if (!IsQuizOpen || hasForfeited)
        {
            if (ForfeitConfirmationUI != null)
                ForfeitConfirmationUI.SetActive(false);

            return;
        }

        RegisterForfeitFromPadlock();

        if (padlockInspector != null)
        {
            padlockInspector.CloseQuiz();
        }
        else
        {
            CloseUI();
        }
    }

    private IEnumerator RunTimer()
    {
        while (IsQuizOpen && !hasForfeited && timeRemaining > 0f)
        {
            timeRemaining -= Time.deltaTime;
            if (timeRemaining < 0f)
                timeRemaining = 0f;

            UpdateTimerText();
            yield return null;
        }

        if (IsQuizOpen && !hasForfeited && timeRemaining <= 0f)
            ForfeitQuiz();
    }

    private void UpdateTimerText()
    {
        if (TimerText == null)
            return;

        int seconds = Mathf.CeilToInt(timeRemaining);
        TimerText.text = string.Format("{0:00}:{1:00}", seconds / 60, seconds % 60);
    }

    private int CalculateCurrentQuizPoints()
    {
        if (hasForfeited || quizDuration <= 0f || timeRemaining <= 0f)
            return 0;

        return Mathf.Clamp(
            Mathf.FloorToInt(maxPointsPerQuiz * (timeRemaining / quizDuration)),
            0,
            maxPointsPerQuiz
        );
    }

    private int AwardQuizPoints()
    {
        if (scoreAwarded)
            return 0;

        scoreAwarded = true;

        // A forfeited quiz still allows progression, but awards no points.
        if (hasForfeited)
            return 0;

        int earned = CalculateCurrentQuizPoints();

        if (PlayFabSaveManager.Instance != null)
        {
            int total = PlayFabSaveManager.Instance.GetQuizPoints() + earned;
            PlayFabSaveManager.Instance.SetQuizPoints(total);
        }

        return earned;
    }

    private void UnlockAndDestroyLock()
    {
        // Unlock the assigned door after the score summary.
        if (doorOpenerScript is OpenDoor openDoor)
        {
            openDoor.UnlockDoor();
        }
        else if (doorOpenerScript is EnterCave enterCave)
        {
            enterCave.UnlockDoor();
        }

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

        if (padlockInspector != null)
            padlockInspector.CloseQuiz();
        else
            CloseUI();

        gameObject.SetActive(false);
    }

    public void CloseUI()
    {
        IsQuizOpen = false;
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

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

    public void RegisterForfeitFromPadlock()
    {
        if (hasForfeited)
            return;

        hasForfeited = true;
        timeRemaining = 0f;

        UpdateTimerText();

        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }

        if (PlayFabSaveManager.Instance != null)
        {
            PlayFabSaveManager.Instance.MarkQuizForfeited(quizID);
        }

        if (ForfeitConfirmationUI != null)
        {
            ForfeitConfirmationUI.SetActive(false);
        }

        Debug.Log("Quiz forfeited: " + quizID);
    }
}