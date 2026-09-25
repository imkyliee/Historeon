using UnityEngine;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class QuestionData
{
    [TextArea(2, 4)]
    public string questionText;

    [Header("Answers")]
    public string correctAnswer;    // Type the right answer text here
    public string wrongAnswer1;     // Type wrong answer 1
    public string wrongAnswer2;     // Type wrong answer 2
    public string wrongAnswer3;     // Type wrong answer 3
}

public class QuizManager : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI QuestionTextDisplay;
    public GameObject[] AnswerButtons;        // Drag B1, B2, B3, B4 here
    public TextMeshProUGUI CheckerText;        // Feedback text ("Correct!" / "Incorrect!")
    public TextMeshProUGUI ScoreText;          // Drag ScoreText here for progress display

    [Header("Question Goal Settings")]
    [Tooltip("How many correct answers are required to destroy the padlock.")]
    public int requiredCorrectAnswers = 5;

    [Header("Question Pool")]
    public List<QuestionData> questionPool;   // Add your questions here

    [Header("Lock & Door References")]
    public GameObject padlockObject;          // Drag Padlock here
    public MonoBehaviour doorOpenerScript;     // Drag RaycastDoorOpener script object here
    public MonoBehaviour PlayerMovement;      // Drag player object here

    private QuestionData currentQuestion;
    private List<string> shuffledAnswers = new List<string>();
    private int correctAnswersCount = 0;
    private bool hasQuestion = false;

    private void OnEnable()
    {
        // Update UI score display when opened
        if (ScoreText != null)
        {
            ScoreText.text = "Progress: " + correctAnswersCount + " / " + requiredCorrectAnswers;
        }

        // Only pick a new question if we don't already have an active one
        if (!hasQuestion)
        {
            DisplayNextQuestion();
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

        // Update Progress Tracker UI
        if (ScoreText != null)
        {
            ScoreText.text = "Progress: " + correctAnswersCount + " / " + requiredCorrectAnswers;
        }

        // Pick a question randomly from the pool
        int randomIndex = Random.Range(0, questionPool.Count);
        currentQuestion = questionPool[randomIndex];

        // Display Question Text
        if (QuestionTextDisplay != null)
            QuestionTextDisplay.text = currentQuestion.questionText;

        // Combine all 4 answers into a list
        shuffledAnswers.Clear();
        shuffledAnswers.Add(currentQuestion.correctAnswer);
        shuffledAnswers.Add(currentQuestion.wrongAnswer1);
        shuffledAnswers.Add(currentQuestion.wrongAnswer2);
        shuffledAnswers.Add(currentQuestion.wrongAnswer3);

        // Shuffle the list randomly
        for (int i = 0; i < shuffledAnswers.Count; i++)
        {
            string temp = shuffledAnswers[i];
            int randIndex = Random.Range(i, shuffledAnswers.Count);
            shuffledAnswers[i] = shuffledAnswers[randIndex];
            shuffledAnswers[randIndex] = temp;
        }

        // Assign shuffled text to each of the 4 buttons
        for (int i = 0; i < AnswerButtons.Length; i++)
        {
            if (i < shuffledAnswers.Count)
            {
                AnswerButtons[i].SetActive(true);
                TextMeshProUGUI buttonText = AnswerButtons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (buttonText != null)
                {
                    buttonText.text = shuffledAnswers[i];
                }
            }
        }
    }

    public void SelectAnswer(int buttonIndex)
    {
        if (currentQuestion == null || buttonIndex >= shuffledAnswers.Count) return;

        string selectedText = shuffledAnswers[buttonIndex];

        // Check if clicked button's text matches the correct answer
        if (selectedText == currentQuestion.correctAnswer)
        {
            correctAnswersCount++;

            // Check if player met the required target
            if (correctAnswersCount >= requiredCorrectAnswers)
            {
                if (CheckerText != null) CheckerText.text = "<color=#00FF00>Unlocked!</color>";
                UnlockAndDestroyLock();
            }
            else
            {
                if (CheckerText != null) CheckerText.text = "<color=#00FF00>Correct!</color>";
                DisplayNextQuestion();
            }
        }
        else
        {
            // Set text to inline red color for incorrect answer
            if (CheckerText != null) CheckerText.text = "<color=#FF0000>Incorrect!</color>";

            // Force a new random question and reshuffle choices
            DisplayNextQuestion();
        }
    }

    // Call this from your Exit Button OnClick event
    public void ExitQuiz()
    {
        CloseUI();
    }

    private void UnlockAndDestroyLock()
    {
        // Permanently destroy the padlock object
        if (padlockObject != null)
        {
            Destroy(padlockObject);
        }

        // Enable door script
        if (doorOpenerScript != null)
        {
            doorOpenerScript.enabled = true;
        }

        CloseUI();
    }

    public void CloseUI()
    {
        // Re-enable player movement
        if (PlayerMovement != null)
        {
            PlayerMovement.enabled = true;
        }

        // Hide UI panel
        gameObject.SetActive(false);

        // Lock mouse cursor back to game mode
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }
}