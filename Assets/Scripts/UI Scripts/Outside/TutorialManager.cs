using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    [Header("Tutorials")]
    public GameObject wasdTutorial;
    public GameObject spaceTutorial;
    public GameObject lShiftTutorial;
    public GameObject pickupTutorial;
    public GameObject dropTutorial;
    public GameObject leftClickTutorial;
    public GameObject rightClickTutorial;

    private int currentTutorial = 0;

    private bool hatchetFinished = false;
    private bool flashlightFinished = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        ShowTutorial(0);
    }

    private void ShowTutorial(int index)
    {
        wasdTutorial.SetActive(false);
        spaceTutorial.SetActive(false);
        lShiftTutorial.SetActive(false);
        pickupTutorial.SetActive(false);
        dropTutorial.SetActive(false);
        leftClickTutorial.SetActive(false);
        rightClickTutorial.SetActive(false);

        currentTutorial = index;

        switch (index)
        {
            case 0:
                wasdTutorial.SetActive(true);
                break;

            case 1:
                spaceTutorial.SetActive(true);
                break;

            case 2:
                lShiftTutorial.SetActive(true);
                break;

            case 3:
                pickupTutorial.SetActive(true);
                break;

            case 4:
                leftClickTutorial.SetActive(true);
                break;

            case 5:
                rightClickTutorial.SetActive(true);
                break;

            case 6:
                pickupTutorial.SetActive(true);
                break;

            case 7:
                leftClickTutorial.SetActive(true);
                break;

            case 8:
                rightClickTutorial.SetActive(true);
                break;

            case 9:
                dropTutorial.SetActive(true);
                break;

            case 10:
                break;
        }
    }

    public void CompleteWASD()
    {
        if (currentTutorial != 0)
            return;

        ShowTutorial(1);
    }

    public void CompleteSpace()
    {
        if (currentTutorial != 1)
            return;

        ShowTutorial(2);
    }

    public void CompleteLShift()
    {
        if (currentTutorial != 2)
            return;

        ShowTutorial(3);
    }

    public void CompletePickup(ItemSO pickedUpItem, ItemSO hatchetItem, ItemSO flashlightItem)
    {
        if (currentTutorial == 3)
        {
            if (pickedUpItem == hatchetItem)
            {
                ShowTutorial(4);
            }
            else if (pickedUpItem == flashlightItem)
            {
                ShowTutorial(5);
            }
        }
        else if (currentTutorial == 6)
        {
            if (pickedUpItem == hatchetItem && !hatchetFinished)
            {
                ShowTutorial(7);
            }
            else if (pickedUpItem == flashlightItem && !flashlightFinished)
            {
                ShowTutorial(8);
            }
        }
    }

    public void CompleteHatchetAttack()
    {
        if (currentTutorial != 4 && currentTutorial != 7)
            return;

        hatchetFinished = true;

        if (flashlightFinished)
        {
            ShowTutorial(9);
        }
        else
        {
            ShowTutorial(6);
        }
    }

    public void CompleteFlashlightUse()
    {
        if (currentTutorial != 5 && currentTutorial != 8)
            return;

        flashlightFinished = true;

        if (hatchetFinished)
        {
            ShowTutorial(9);
        }
        else
        {
            ShowTutorial(6);
        }
    }

    public void CompleteDrop()
    {
        if (currentTutorial != 9)
            return;

        ShowTutorial(10);
    }
}