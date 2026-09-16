using UnityEngine;

public class dragitems1 : MonoBehaviour
{
    private Vector3 offset;
    private bool isDragging = false;
    private Vector3 startPos;

    [Header("Bin Settings")]
    public string correctBinTag; 

    [Header("Feedback Prefabs")]
    public GameObject wrongXPrefab;  
    public GameObject correctStarPrefab; 
    public float feedbackDuration = 1.0f; 
    public float floatSpeed = 1.5f;     

    private ItemSpawner spawner;
    private BinZoom currentHighlightedBin;

    void Start()
    {
        startPos = transform.position;
        spawner = FindObjectOfType<ItemSpawner>();
    }

    void Update()
    {
        HandleMouseDrag();
        HandleTouchDrag();
    }

    void HandleMouseDrag()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mouseWorld = GetWorldPosition(Input.mousePosition);
            if (GetComponent<Collider2D>() == Physics2D.OverlapPoint(mouseWorld))
            {
                offset = transform.position - mouseWorld;
                isDragging = true;
            }
        }
        else if (Input.GetMouseButton(0) && isDragging)
        {
            transform.position = GetWorldPosition(Input.mousePosition) + offset;
            UpdateBinHighlight();
        }
        else if (Input.GetMouseButtonUp(0) && isDragging)
        {
            ClearBinHighlight();
            CheckDrop();
        }
    }

    void HandleTouchDrag()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector3 touchWorld = GetWorldPosition(touch.position);

            if (touch.phase == TouchPhase.Began)
            {
                if (GetComponent<Collider2D>() == Physics2D.OverlapPoint(touchWorld))
                {
                    offset = transform.position - touchWorld;
                    isDragging = true;
                }
            }
            else if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) && isDragging)
            {
                transform.position = touchWorld + offset;
                UpdateBinHighlight();
            }
            else if ((touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled) && isDragging)
            {
                ClearBinHighlight();
                CheckDrop();
            }
        }
    }

    void CheckDrop()
{
    isDragging = false;

    // Sync transforms so physics knows the item's current position before checking
    Physics2D.SyncTransforms();

    Collider2D myCollider = GetComponent<Collider2D>();
    Collider2D[] results = new Collider2D[10];
    int count = myCollider.Overlap(new ContactFilter2D().NoFilter(), results);

    Collider2D binHit = null;
    for (int i = 0; i < count; i++)
    {
        if (results[i] != null && results[i] != myCollider &&
            (results[i].CompareTag("recycle") || results[i].CompareTag("compost") || results[i].CompareTag("landfill")))
        {
            binHit = results[i];
            break;
        }
    }

    if (binHit != null)
    {
        /*BinZoom binZoom = binHit.GetComponent<BinZoom>();
        if (binZoom != null)
        {
            binZoom.PlayZoom();
        }*/

        Vector3 binPos = binHit.transform.position + Vector3.up * 1f;
        bool isCorrect = binHit.CompareTag(correctBinTag);  

        if (isCorrect)
        {
           
            if (correctStarPrefab != null)
            {
                GameObject star = Instantiate(correctStarPrefab, binPos, Quaternion.identity);
                FloatingFeedback ff = star.GetComponent<FloatingFeedback>();
                if (ff != null)
                {
                    ff.Initialize(floatSpeed, feedbackDuration, isCorrect); 
                }
            }

           
            FeedbackSoundManager soundManager = FindObjectOfType<FeedbackSoundManager>();
            if (soundManager != null)
            {
                soundManager.PlayCorrectSound(); 
            }

            spawner.ItemSorted(true);
            Destroy(gameObject, 0.25f);
        }
        else
        {
            
            if (wrongXPrefab != null)
            {
                GameObject x = Instantiate(wrongXPrefab, binPos, Quaternion.identity);
                FloatingFeedback ff = x.GetComponent<FloatingFeedback>();
                if (ff != null)
                {
                    ff.Initialize(floatSpeed, feedbackDuration, isCorrect); 
                }
            }

            
            FeedbackSoundManager soundManager = FindObjectOfType<FeedbackSoundManager>();
            if (soundManager != null)
            {
                soundManager.PlayWrongSound();  
            }

            transform.position = startPos; 
        }
    }
    else
    {
       
        transform.position = startPos;
    }
}
    Vector3 GetWorldPosition(Vector3 screenPos)
    {
        screenPos.z = Camera.main.WorldToScreenPoint(transform.position).z;
        return Camera.main.ScreenToWorldPoint(screenPos);
    }

    void UpdateBinHighlight()
    {
        Collider2D myCollider = GetComponent<Collider2D>();
        Collider2D[] results = new Collider2D[5];
        int count = myCollider.Overlap(new ContactFilter2D().NoFilter(), results);

        BinZoom newHighlightedBin = null;

        for (int i = 0; i < count; i++)
        {
            if (results[i] != null &&
                (results[i].CompareTag("recycle") || results[i].CompareTag("compost") || results[i].CompareTag("landfill")))
            {
                newHighlightedBin = results[i].GetComponent<BinZoom>();

                if (newHighlightedBin == null)
                    newHighlightedBin = results[i].GetComponentInParent<BinZoom>();

                break;
            }
        }

        if (currentHighlightedBin != newHighlightedBin)
        {
            if (currentHighlightedBin != null)
                currentHighlightedBin.SetHighlighted(false);

            currentHighlightedBin = newHighlightedBin;

            if (currentHighlightedBin != null)
                currentHighlightedBin.SetHighlighted(true);
        }
    }

    void ClearBinHighlight()
    {
        if (currentHighlightedBin != null)
        {
            currentHighlightedBin.SetHighlighted(false);
            currentHighlightedBin = null;
        }
    }
}