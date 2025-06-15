using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CursorManager : MonoBehaviour
{
    public Texture2D pointerCursor;
    private Texture2D defaultCursor;

    void Start()
    {
        defaultCursor = null;

        // Find all Buttons in the scene
        Button[] buttons = FindObjectsOfType<Button>();
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

        // Pointer Enter event
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => {
            Cursor.SetCursor(pointerCursor, Vector2.zero, CursorMode.Auto);
        });

        // Pointer Exit event
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => {
            Cursor.SetCursor(defaultCursor, Vector2.zero, CursorMode.Auto);
        });

        trigger.triggers.Add(enterEntry);
        trigger.triggers.Add(exitEntry);
    }
}
