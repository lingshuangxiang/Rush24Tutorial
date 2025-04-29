using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class CustomToggle : MonoBehaviour
{
    [SerializeField] private Text leftText;
    [SerializeField] private Text rightText;
    [SerializeField] private RectTransform highlightUI;
    [SerializeField] private float transitionDuration = 0.3f;
    [SerializeField] private Color selectedColor = Color.white;
    [SerializeField] private Color unselectedColor = Color.black;

    public UnityEvent onLeftSelected;
    public UnityEvent onRightSelected;

    private bool isLeftSelected = true;

    private void Start()
    {
        UpdateHighlightPosition(true);
        UpdateTextColors(true);
    }

    public void OnLeftClicked()
    {
        if (!isLeftSelected)
        {
            isLeftSelected = true;
            StartCoroutine(MoveHighlight(true));
            StartCoroutine(ChangeTextColors(true));
            onLeftSelected.Invoke();
        }
    }

    public void OnRightClicked()
    {
        if (isLeftSelected)
        {
            isLeftSelected = false;
            StartCoroutine(MoveHighlight(false));
            StartCoroutine(ChangeTextColors(false));
            onRightSelected.Invoke();
        }
    }

    private IEnumerator MoveHighlight(bool toLeft)
    {
        Vector2 startPos = highlightUI.anchoredPosition;    
        Vector2 endpos = toLeft? GetLeftPosition(): GetRightPosition();
        float elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t=elapsedTime / transitionDuration;   
            highlightUI.anchoredPosition = Vector2.Lerp(startPos, endpos, t);
            yield return null;
        }

       highlightUI.anchoredPosition = endpos;   
    }
    
    
    
    
    private void UpdateHighlightPosition(bool immediate)
    {
       Vector2 targetPos=isLeftSelected? GetLeftPosition() : GetRightPosition();
       if (immediate)
       {
           highlightUI.anchoredPosition = targetPos;
       }
       else
       {
           StartCoroutine(MoveHighlight(isLeftSelected));
       }
    }
    
    
    
    private Vector2 GetLeftPosition()
    {
        return new Vector2(leftText.rectTransform.anchoredPosition.x, highlightUI.anchoredPosition.y);
    }
    
    private Vector2 GetRightPosition()
    {
       return new Vector2(rightText.rectTransform.anchoredPosition.x, highlightUI.anchoredPosition.y);  
    }

    private IEnumerator ChangeTextColors(bool leftSelected)
    {
        float elapsedTime = 0f;
        Color leftStartColor = leftText.color;
        Color rightStartColor = rightText.color;
        Color leftTargetColor = leftSelected ? selectedColor : unselectedColor;
        Color rightTargetColor = leftSelected ? unselectedColor : selectedColor;

        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            leftText.color = Color.Lerp(leftStartColor, leftTargetColor, t);
            rightText.color = Color.Lerp(rightStartColor, rightTargetColor, t);
            yield return null;
        }

        leftText.color = leftTargetColor;
        rightText.color = rightTargetColor;
    }

    private void UpdateTextColors(bool immediate)
    {
        if (immediate)
        {
            leftText.color = isLeftSelected ? selectedColor : unselectedColor;
            rightText.color = isLeftSelected ? unselectedColor : selectedColor;
        }
        else
        {
            StartCoroutine(ChangeTextColors(isLeftSelected));
        }
    }
}