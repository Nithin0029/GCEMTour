using UnityEngine;

public class BottomDrawer : MonoBehaviour
{
    public RectTransform panel;
    public float shownY = 0f;
    public float hiddenY = -350f;
    public float speed = 8f;
    private bool isOpen = false;

    void Start()
    {
        Vector2 pos = panel.anchoredPosition;
        pos.y = hiddenY;
        panel.anchoredPosition = pos;
    }

    void Update()
    {
        float target = isOpen ? shownY : hiddenY;

        Vector2 pos = panel.anchoredPosition;
        pos.y = Mathf.Lerp(pos.y, target, Time.deltaTime * speed);
        panel.anchoredPosition = pos;
    }

    public void Toggle()
    {
        isOpen = !isOpen;
    }
}
