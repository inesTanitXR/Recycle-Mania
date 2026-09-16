using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 
using System.Collections;
using TMPro;
using System.Linq;

public class ItemSpawner : MonoBehaviour
{
    public GameUI gameUI;
    public GameObject[] itemPrefabs;
    public Transform spawnPoint;

    [Header("Item Sizing")]
    [Tooltip("How large each item appears in world units (same for all items)")]
    public float targetWorldSize = 1.7f;

    private int currentIndex = 0;
    private int correctCount = 0;
    private GameObject currentItem;

    public int TotalItems => itemPrefabs != null ? itemPrefabs.Length : 0;

    [Header("UI Elements")]
    public TextMeshProUGUI itemCounterText;       
    public TextMeshProUGUI finalMessageText;      
    public Button continueButton;     

    void Start()
    {
        if (gameUI == null)
            gameUI = FindObjectOfType<GameUI>();

        ShuffleItems();
        SpawnNewItem();
        continueButton.gameObject.SetActive(false);
        continueButton.onClick.AddListener(OnContinueClicked);
    }
    public void ShuffleItems()
    {
        itemPrefabs = itemPrefabs.OrderBy(x => Random.value).ToArray();  
    }
    public void SpawnNewItem()
    {
        if (currentIndex < itemPrefabs.Length)
        {
            currentItem = Instantiate(itemPrefabs[currentIndex], spawnPoint.position, Quaternion.identity);
            NormalizeItemScale(currentItem);
            UpdateItemCounter();
        }
        else
        {
            if (itemCounterText != null)
                itemCounterText.text = "All items done!";

            StartCoroutine(ShowFinalMessage());
        }
    }

    
    public void OnContinueClicked()
    {
        
        SceneManager.LoadScene("LevelSelect");
    }

public void ItemSorted(bool correct)
{
    if (correct)
    {
        correctCount++;
        if (gameUI != null)
            gameUI.IncrementScore();
    }

    // Refresh the on-screen score right away. Previously this only happened
    // inside SpawnNewItem(), so the point for the LAST item was never drawn
    // and a perfect run always displayed 9/10.
    UpdateItemCounter();

    currentIndex++; 

    if (currentIndex < itemPrefabs.Length)
    {
        SpawnNewItem();  
    }
    else
    {
        StartCoroutine(ShowFinalMessage());
    }
}

IEnumerator ShowFinalMessage()
{
    foreach (var ui in FindObjectsOfType<GameUI>())
        ui.StopTimer();

    if (currentItem != null)
        Destroy(currentItem);

    if (finalMessageText != null)
    {
        finalMessageText.gameObject.SetActive(true);
        int total = itemPrefabs != null ? itemPrefabs.Length : 0;
        finalMessageText.text = (total > 0 && correctCount >= total)
            ? "Perfect score! " + correctCount + "/" + total
            : "Good job! " + correctCount + "/" + total;
    }
    yield return new WaitForSeconds(3f);
    if (finalMessageText != null)
    {
        finalMessageText.gameObject.SetActive(false);
    }
    continueButton.gameObject.SetActive(true);
}

public void TimeUp()
{
    StartCoroutine(ShowTimeUpAndRestart());
}

IEnumerator ShowTimeUpAndRestart()
{
    foreach (var ui in FindObjectsOfType<GameUI>())
        ui.StopTimer();

    if (currentItem != null)
        Destroy(currentItem);

    if (finalMessageText != null)
    {
        finalMessageText.gameObject.SetActive(true);
        finalMessageText.text = "Time's up!";
    }

    yield return new WaitForSeconds(2f);

    if (finalMessageText != null)
        finalMessageText.gameObject.SetActive(false);

    // Relabel the continue button to go to Main Menu instead
    if (continueButton != null)
    {
        var label = continueButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (label != null) label.text = "Main Menu";

        continueButton.onClick.RemoveAllListeners();
        continueButton.onClick.AddListener(() => SceneManager.LoadScene("Main Menu"));
        continueButton.gameObject.SetActive(true);
    }
}
    void NormalizeItemScale(GameObject item)
    {
        SpriteRenderer sr = item.GetComponent<SpriteRenderer>();
        if (sr != null && sr.sprite != null)
        {
            float spriteWidth  = sr.sprite.rect.width  / sr.sprite.pixelsPerUnit;
            float spriteHeight = sr.sprite.rect.height / sr.sprite.pixelsPerUnit;
            float largest = Mathf.Max(spriteWidth, spriteHeight);
            float scale = targetWorldSize / largest;
            item.transform.localScale = new Vector3(scale, scale, scale);
        }
        else
        {
            // fallback if no SpriteRenderer found
            item.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
        }
    }

    void UpdateItemCounter()
    {
        if (itemCounterText != null)
            itemCounterText.text = "Score: " + correctCount + "/" + itemPrefabs.Length;
    }
}