using UnityEngine;

public class DailyCustomerSpawner : MonoBehaviour
{
    [Header("Customers are Day2 through Day7 in order.")]
    public GameObject[] customers;

    [Header("DLC Customers are Day1 through Day3 in order.")]
    public GameObject[] dlcCustomers;

    private void Start()
    {
        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager.currentDay;
        bool isDiabetesDLC = gameManager.currentGameMode == GameMode.DiabetesDLC;

        SetAllCustomersInactive();

        if (isDiabetesDLC)
        {
            SelectCustomerForDay(dlcCustomers, currentDay - 1, currentDay, "DiabetesDLC Day1-Day3");
            return;
        }

        SelectCustomerForDay(customers, currentDay - 2, currentDay, "MainStory Day2-Day7");
    }

    private void SelectCustomerForDay(GameObject[] customerArray, int customerIndex, int currentDay, string label)
    {
        if (customerIndex < 0 || customerIndex >= GetCustomerCount(customerArray))
        {
            Debug.LogWarning(
                "DailyCustomerSpawner found no customer for currentDay = " + currentDay +
                ". Expected " + label +
                ", calculated customerIndex = " + customerIndex + ".");
            LogCustomerStates(customerArray, label);
            return;
        }

        GameObject activeCustomer = customerArray[customerIndex];
        if (activeCustomer == null)
        {
            Debug.LogWarning(
                "DailyCustomerSpawner customer slot is empty. currentDay = " + currentDay +
                ", customerIndex = " + customerIndex +
                ", group = " + label + ".");
            LogCustomerStates(customerArray, label);
            return;
        }

        activeCustomer.SetActive(true);
        LogActiveCustomerTrigger(currentDay, activeCustomer);
        LogCustomerVisuals(activeCustomer);
        LogCustomerStates(customerArray, label);
    }

    private void SetAllCustomersInactive()
    {
        SetCustomersInactive(customers);
        SetCustomersInactive(dlcCustomers);
    }

    private void SetCustomersInactive(GameObject[] customerArray)
    {
        if (customerArray == null)
            return;

        for (int i = 0; i < customerArray.Length; i++)
        {
            if (customerArray[i] != null)
                customerArray[i].SetActive(false);
        }
    }

    private int GetCustomerCount(GameObject[] customerArray)
    {
        return customerArray != null ? customerArray.Length : 0;
    }

    private void LogCustomerStates(GameObject[] customerArray, string label)
    {
        if (customerArray == null)
        {
            Debug.LogWarning("DailyCustomerSpawner " + label + " customers array is null.");
            return;
        }

        for (int i = 0; i < customerArray.Length; i++)
        {
            GameObject customer = customerArray[i];
            string customerName = customer != null ? customer.name : "None";
            string activeState = customer != null ? customer.activeSelf.ToString() : "Missing";
        }
    }

    private void LogCustomerVisuals(GameObject customer)
    {
        SpriteRenderer[] renderers = customer.GetComponentsInChildren<SpriteRenderer>(true);
        if (renderers.Length == 0)
        {
            Debug.LogWarning("DailyCustomerSpawner active customer has no SpriteRenderer. activeCustomer = " + customer.name + ".");
        }

        for (int i = 0; i < renderers.Length; i++)
        {
            Sprite sprite = renderers[i].sprite;
            string spriteName = sprite != null ? sprite.name : "None";
        }

        Animator[] animators = customer.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            RuntimeAnimatorController controller = animators[i].runtimeAnimatorController;
            string controllerName = controller != null ? controller.name : "None";
        }
    }

    private void LogActiveCustomerTrigger(int currentDay, GameObject activeCustomer)
    {
        CustomerTrigger customerTrigger = activeCustomer.GetComponentInChildren<CustomerTrigger>(true);
        if (customerTrigger == null)
        {
            Debug.LogWarning("DailyCustomerSpawner activeCustomer has no CustomerTrigger. currentDay = " + currentDay + ", activeCustomer = " + activeCustomer.name + ".");
            return;
        }

        GameObject exclamationMark = customerTrigger.exclamationMark;
        string exclamationName = exclamationMark != null ? exclamationMark.name : "None";
        string exclamationActive = exclamationMark != null ? exclamationMark.activeSelf.ToString() : "Missing";

        if (exclamationMark == null)
            Debug.LogWarning("CustomerTrigger missing exclamationMark reference. customer = " + customerTrigger.gameObject.name + ".");
    }
}
