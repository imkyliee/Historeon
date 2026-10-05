using System.Collections;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Starting Tutorial")]
    public Collider tutorialTrigger;
    public GameObject tutorialUI1;
    public GameObject tutorialUI2;
    public GameObject tutorialUI3;
    public GameObject tutorialUI4;
    public GameObject tutorialUI5;

    [Header("Second Tutorial")]
    public Collider secondTutorialTrigger;
    public GameObject logbookBG;
    public GameObject logbookNotif;
    public GameObject pickupBG;
    public GameObject inventoryBG;
    public GameObject dropBG;
    public GameObject rightClickBG;

    [Header("Third Tutorial")]
    public GameObject logbookBG1;
    public GameObject openBG;
    public GameObject hatchetBG;
    public GameObject leftClicking;

    [Header("References")]
    public CinematicCamera cinematicCamera;
    public Inventory inventory;
    public TabMenuManager tabMenuManager;

    [Header("Logbook Notification")]
    public Animator UIanimation;
    public float logbookCloseDelay = 0.5f;

    private int startingTutorialIndex = 0;
    private int secondTutorialIndex = 0;
    private int thirdTutorialIndex = 0;

    private bool secondTutorialStarted = false;
    private bool logbookNotificationOpen = false;
    private bool thirdTutorialStarted = false;

    private bool flashlightPickedUp = false;
    private bool bagPickedUp = false;

    private bool flashlightFirst = false;
    private bool bagFirst = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        HideAllStartingTutorials();
        HideAllSecondTutorials();
        HideAllThirdTutorials();

        if (tutorialTrigger != null)
            tutorialTrigger.enabled = true;

        if (secondTutorialTrigger != null)
            secondTutorialTrigger.enabled = false;

        ShowStartingTutorial(0);
    }

    private void Update()
    {
        if (logbookNotificationOpen)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                CloseLogbookNotification();
            }
        }
    }

    private void HideAllStartingTutorials()
    {
        if (tutorialUI1 != null)
            tutorialUI1.SetActive(false);

        if (tutorialUI2 != null)
            tutorialUI2.SetActive(false);

        if (tutorialUI3 != null)
            tutorialUI3.SetActive(false);

        if (tutorialUI4 != null)
            tutorialUI4.SetActive(false);

        if (tutorialUI5 != null)
            tutorialUI5.SetActive(false);
    }

    private void ShowStartingTutorial(int index)
    {
        HideAllStartingTutorials();

        startingTutorialIndex = index;

        switch (index)
        {
            case 0:
                if (tutorialUI1 != null)
                    tutorialUI1.SetActive(true);
                break;

            case 1:
                if (tutorialUI2 != null)
                    tutorialUI2.SetActive(true);
                break;

            case 2:
                if (tutorialUI3 != null)
                    tutorialUI3.SetActive(true);
                break;

            case 3:
                if (tutorialUI4 != null)
                    tutorialUI4.SetActive(true);
                break;

            case 4:
                if (tutorialUI5 != null)
                    tutorialUI5.SetActive(true);
                break;
        }
    }

    public void CompleteWASD()
    {
        if (startingTutorialIndex != 0)
            return;

        ShowStartingTutorial(1);
    }

    public void CompleteSpace()
    {
        if (startingTutorialIndex != 1)
            return;

        ShowStartingTutorial(2);
    }

    public void CompleteLShift()
    {
        if (startingTutorialIndex != 2)
            return;

        ShowStartingTutorial(3);
    }

    public void CompleteCrouch()
    {
        if (startingTutorialIndex != 3)
            return;

        ShowStartingTutorial(4);
    }

    public void CompletePadlock()
    {
        if (startingTutorialIndex != 4)
            return;

        HideAllStartingTutorials();
    }

    public void StartSecondTutorial()
    {
        if (secondTutorialStarted)
            return;

        secondTutorialStarted = true;

        // Destroy the first tutorial UIs
        if (tutorialUI1 != null)
            Destroy(tutorialUI1);

        if (tutorialUI2 != null)
            Destroy(tutorialUI2);

        if (tutorialUI3 != null)
            Destroy(tutorialUI3);

        if (tutorialUI4 != null)
            Destroy(tutorialUI4);

        if (tutorialUI5 != null)
            Destroy(tutorialUI5);

        if (tutorialTrigger != null)
            tutorialTrigger.enabled = false;

        if (secondTutorialTrigger != null)
            secondTutorialTrigger.enabled = true;

        ShowSecondTutorial(0);
    }

    private void HideAllSecondTutorials()
    {
        if (logbookBG != null)
            logbookBG.SetActive(false);

        if (logbookNotif != null)
            logbookNotif.SetActive(false);

        if (pickupBG != null)
            pickupBG.SetActive(false);

        if (inventoryBG != null)
            inventoryBG.SetActive(false);

        if (dropBG != null)
            dropBG.SetActive(false);

        if (rightClickBG != null)
            rightClickBG.SetActive(false);
    }

    private void ShowSecondTutorial(int index)
    {
        HideAllSecondTutorials();

        secondTutorialIndex = index;

        switch (index)
        {
            case 0:
                if (logbookBG != null)
                    logbookBG.SetActive(true);
                break;

            case 1:
                ShowLogbookNotification();
                break;

            case 2:
                if (pickupBG != null)
                    pickupBG.SetActive(true);
                break;

            case 3:
                if (inventoryBG != null)
                    inventoryBG.SetActive(true);
                break;

            case 4:
                if (dropBG != null)
                    dropBG.SetActive(true);
                break;

            case 5:
                if (rightClickBG != null)
                    rightClickBG.SetActive(true);
                break;

            case 6:
                break;
        }
    }

    public void CompleteLogbookBG()
    {
        if (thirdTutorialStarted && thirdTutorialIndex == 0)
        {
            ShowThirdTutorial(1);
            return;
        }

        if (secondTutorialIndex != 0)
            return;

        HideAllSecondTutorials();

        secondTutorialIndex = 1;

        ShowLogbookNotification();
    }

    private void ShowLogbookNotification()
    {
        if (logbookNotif == null)
            return;

        logbookNotificationOpen = true;

        logbookNotif.SetActive(true);

        if (cinematicCamera != null)
            cinematicCamera.DisablePlayer();

        if (UIanimation != null)
            UIanimation.Play("Open", 0, 0f);
    }

    public void CloseLogbookNotification()
    {
        if (!logbookNotificationOpen)
            return;

        logbookNotificationOpen = false;

        if (UIanimation != null)
        {
            UIanimation.Play("Close", 0, 0f);

            StartCoroutine(FinishLogbookNotification());
        }
        else
        {
            FinishLogbookNotificationImmediately();
        }
    }

    private IEnumerator FinishLogbookNotification()
    {
        yield return new WaitForSeconds(logbookCloseDelay);

        if (logbookNotif != null)
            logbookNotif.SetActive(false);

        if (cinematicCamera != null)
            cinematicCamera.EnablePlayer();

        ShowSecondTutorial(2);
    }

    private void FinishLogbookNotificationImmediately()
    {
        if (logbookNotif != null)
            logbookNotif.SetActive(false);

        if (cinematicCamera != null)
            cinematicCamera.EnablePlayer();

        ShowSecondTutorial(2);
    }

    public void CompletePickup(
        ItemSO pickedUpItem,
        ItemSO hatchetItem,
        ItemSO flashlightItem)
    {
        if (thirdTutorialStarted &&
            thirdTutorialIndex == 2 &&
            pickedUpItem == hatchetItem)
        {
            ShowThirdTutorial(3);
            return;
        }

        if (secondTutorialIndex != 2)
            return;

        HideAllSecondTutorials();

        // Flashlight picked up first
        if (!flashlightPickedUp && pickedUpItem == flashlightItem)
        {
            flashlightPickedUp = true;
            flashlightFirst = true;

            secondTutorialIndex = 5;

            if (rightClickBG != null)
                rightClickBG.SetActive(true);

            return;
        }

        // Bag picked up first
        if (!bagPickedUp &&
            inventory != null &&
            pickedUpItem == inventory.bagitem)
        {
            bagPickedUp = true;
            bagFirst = true;

            secondTutorialIndex = 3;

            if (inventoryBG != null)
                inventoryBG.SetActive(true);

            return;
        }

        // Bag picked up after Flashlight
        if (flashlightFirst &&
            !bagPickedUp &&
            inventory != null &&
            pickedUpItem == inventory.bagitem)
        {
            bagPickedUp = true;

            secondTutorialIndex = 3;

            if (inventoryBG != null)
                inventoryBG.SetActive(true);

            return;
        }

        // Flashlight picked up after Bag
        if (bagFirst &&
            !flashlightPickedUp &&
            pickedUpItem == flashlightItem)
        {
            flashlightPickedUp = true;

            secondTutorialIndex = 5;

            if (rightClickBG != null)
                rightClickBG.SetActive(true);
        }
    }

    public void CompletePickupBG()
    {
        if (secondTutorialIndex != 2)
            return;

        ShowSecondTutorial(3);
    }

    public void CompleteInventoryBG()
    {
        if (thirdTutorialStarted && thirdTutorialIndex == 1)
        {
            ShowThirdTutorial(2);
            return;
        }

        if (secondTutorialIndex != 3)
            return;

        ShowSecondTutorial(4);
    }

    public void CompleteDrop()
    {
        if (secondTutorialIndex != 4)
            return;

        // Bag was picked up first.
        // After dropping the Bag, teach the player to pick up the Flashlight.
        if (bagFirst && !flashlightPickedUp)
        {
            ShowSecondTutorial(2);
            return;
        }

        // Flashlight was picked up first.
        // Q is the final tutorial step.
        if (flashlightFirst && bagPickedUp)
        {
            ShowSecondTutorial(6);
            return;
        }

        ShowSecondTutorial(5);
    }

    public void CompleteDropBG()
    {
        if (secondTutorialIndex != 4)
            return;

        // Bag was picked up first.
        // After dropping the Bag, teach the player to pick up the Flashlight.
        if (bagFirst && !flashlightPickedUp)
        {
            ShowSecondTutorial(2);
            return;
        }

        // Flashlight was picked up first.
        // Q is the final tutorial step.
        if (flashlightFirst && bagPickedUp)
        {
            ShowSecondTutorial(6);
            return;
        }

        ShowSecondTutorial(5);
    }

    public void CompleteRightClickBG()
    {
        if (secondTutorialIndex != 5)
            return;

        if (flashlightFirst && !bagPickedUp)
        {
            ShowSecondTutorial(2);
            return;
        }

        ShowSecondTutorial(6);
    }

    private void HideAllThirdTutorials()
    {
        if (logbookBG1 != null)
            logbookBG1.SetActive(false);

        if (openBG != null)
            openBG.SetActive(false);

        if (hatchetBG != null)
            hatchetBG.SetActive(false);

        if (leftClicking != null)
            leftClicking.SetActive(false);
    }

    private void ShowThirdTutorial(int index)
    {
        HideAllThirdTutorials();

        thirdTutorialIndex = index;

        switch (index)
        {
            case 0:
                if (logbookBG1 != null)
                    logbookBG1.SetActive(true);
                break;

            case 1:
                if (openBG != null)
                    openBG.SetActive(true);
                break;

            case 2:
                if (hatchetBG != null)
                    hatchetBG.SetActive(true);
                break;

            case 3:
                if (leftClicking != null)
                    leftClicking.SetActive(true);
                break;

            case 4:
                break;
        }
    }

    public void StartThirdTutorial()
    {
        if (thirdTutorialStarted)
            return;

        thirdTutorialStarted = true;

        // Destroy second tutorial UIs
        if (logbookBG != null)
            Destroy(logbookBG);

        if (logbookNotif != null)
            Destroy(logbookNotif);

        if (pickupBG != null)
            Destroy(pickupBG);

        if (inventoryBG != null)
            Destroy(inventoryBG);

        if (dropBG != null)
            Destroy(dropBG);

        if (rightClickBG != null)
            Destroy(rightClickBG);

        secondTutorialIndex = 6;

        // Disable Trigger 2
        if (secondTutorialTrigger != null)
            secondTutorialTrigger.enabled = false;

        // Start cinematic
        if (cinematicCamera != null)
            cinematicCamera.PlayCinematic1();
        else
            CinematicFinished();
    }

    public void CinematicFinished()
    {
        ShowThirdTutorial(0);
    }

    public void CompleteThirdLogbook()
    {
        if (thirdTutorialIndex != 0)
            return;

        ShowThirdTutorial(1);
    }

    public void CompleteThirdHatchet()
    {
        if (thirdTutorialIndex != 2)
            return;

        ShowThirdTutorial(3);
    }

    public void CompleteThirdLeftClick()
    {
        if (thirdTutorialIndex != 3)
            return;

        ShowThirdTutorial(4);
    }
}