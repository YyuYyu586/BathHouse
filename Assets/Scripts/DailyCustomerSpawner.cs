using UnityEngine;

public class DailyCustomerSpawner : MonoBehaviour
{
    [Header("Customers are Day2 through Day7 in order.")]
    public GameObject[] customers;

    private void Start()
    {
        GameManager gameManager = GameManager.EnsureInstance();
        int currentDay = gameManager.currentDay;

        Debug.Log("DailyCustomerSpawner Start. currentDay = " + currentDay + ", customers count = " + GetCustomerCount() + ".");

        SetAllCustomersInactive();

        int customerIndex = currentDay - 2;
        if (customerIndex < 0 || customerIndex >= GetCustomerCount())
        {
            Debug.LogWarning("DailyCustomerSpawner found no customer for currentDay = " + currentDay + ". Expected Day2-Day7, calculated customerIndex = " + customerIndex + ".");
            LogCustomerStates();
            return;
        }

        GameObject activeCustomer = customers[customerIndex];
        if (activeCustomer == null)
        {
            Debug.LogWarning("DailyCustomerSpawner customer slot is empty. currentDay = " + currentDay + ", customerIndex = " + customerIndex + ".");
            LogCustomerStates();
            return;
        }

        activeCustomer.SetActive(true);
        Debug.Log("DailyCustomerSpawner selected customer. currentDay = " + currentDay + ", customerIndex = " + customerIndex + ", activeCustomer = " + activeCustomer.name + ".");
        LogActiveCustomerTrigger(currentDay, activeCustomer);
        LogCustomerVisuals(activeCustomer);
        LogCustomerStates();
    }

    private void SetAllCustomersInactive()
    {
        if (customers == null)
            return;

        for (int i = 0; i < customers.Length; i++)
        {
            if (customers[i] != null)
                customers[i].SetActive(false);
        }
    }

    private int GetCustomerCount()
    {
        return customers != null ? customers.Length : 0;
    }

    private void LogCustomerStates()
    {
        if (customers == null)
        {
            Debug.LogWarning("DailyCustomerSpawner customers array is null.");
            return;
        }

        for (int i = 0; i < customers.Length; i++)
        {
            GameObject customer = customers[i];
            string customerName = customer != null ? customer.name : "None";
            string activeState = customer != null ? customer.activeSelf.ToString() : "Missing";
            Debug.Log("DailyCustomerSpawner customer state. element=" + i + ", customer=" + customerName + ", activeSelf=" + activeState + ".");
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
            Debug.Log("DailyCustomerSpawner active customer SpriteRenderer. activeCustomer=" + customer.name + ", renderer=" + renderers[i].name + ", sprite=" + spriteName + ".");
        }

        Animator[] animators = customer.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            RuntimeAnimatorController controller = animators[i].runtimeAnimatorController;
            string controllerName = controller != null ? controller.name : "None";
            Debug.Log("DailyCustomerSpawner active customer Animator. activeCustomer=" + customer.name + ", animator=" + animators[i].name + ", controller=" + controllerName + ".");
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

        Debug.Log(
            "DailyCustomerSpawner active customer trigger. currentDay = " + currentDay +
            ", activeCustomer = " + activeCustomer.name +
            ", triggerObject = " + customerTrigger.gameObject.name +
            ", exclamationMarkBound = " + (exclamationMark != null) +
            ", exclamationMark = " + exclamationName +
            ", exclamationActive = " + exclamationActive + ".");

        if (exclamationMark == null)
            Debug.LogWarning("CustomerTrigger missing exclamationMark reference. customer = " + customerTrigger.gameObject.name + ".");
    }
}
