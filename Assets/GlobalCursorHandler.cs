using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class GlobalCursorHandler : MonoBehaviour
{
    public Texture2D pointerCursor;
    private Texture2D defaultCursor;
    private static GlobalCursorHandler instance;

    void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject); // prevent duplicates
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject); // keep it across scenes
    }

    void Start()
    {
        defaultCursor = null;
        ApplyCursorToAllButtons();
    }

    void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        ApplyCursorToAllButtons(); // re-apply cursor handlers on new scene
    }

    void ApplyCursorToAllButtons()
    {
        Button[] buttons = FindObjectsOfType<Button>(true); // include inactive buttons
        foreach (Button btn in buttons)
        {
            AddCursorEvents(btn.gameObject);
        }
    }

    void AddCursorEvents(GameObject obj)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>();
        if (trigger == null)
            trigger = obj.AddComponent<EventTrigger>();

        // Avoid duplicating event handlers
        if (trigger.triggers.Exists(e => e.eventID == EventTriggerType.PointerEnter)) return;

        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => {
            Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
        });

        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        });

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }
}
