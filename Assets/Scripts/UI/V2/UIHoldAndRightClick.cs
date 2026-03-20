using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public interface IHoldableAndClickable
{
    void Hold();
    void LeftClick();
    void RightClick();
}

public class UIHoldAndRightClick : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerClickHandler,
    IPointerExitHandler
{
    [Header("Hold Settings")]
    [SerializeField] private float holdTime = 0.5f;
    [SerializeField] private bool triggerHoldOnlyOnce = true;
    [SerializeField] private bool cancelHoldWhenPointerLeaves = true;

    private readonly List<IHoldableAndClickable> targets = new();

    private bool isHoldingLeft;
    private float holdTimer;
    private bool holdTriggered;

    private void Awake()
    {
        CacheTargets();
    }

    private void OnEnable()
    {
        ResetHoldState();
    }

    private void Update()
    {
        if (!isHoldingLeft)
            return;

        holdTimer += Time.unscaledDeltaTime;

        if (holdTimer >= holdTime)
        {
            if (!holdTriggered || !triggerHoldOnlyOnce)
            {
                holdTriggered = true;
                InvokeHold();
            }

            if (triggerHoldOnlyOnce)
            {
                isHoldingLeft = false;
            }
            else
            {
                holdTimer = 0f;
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isHoldingLeft = true;
        holdTimer = 0f;
        holdTriggered = false;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        ResetHoldState();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        switch (eventData.button)
        {
            case PointerEventData.InputButton.Left:
                InvokeLeftClick();
                break;

            case PointerEventData.InputButton.Right:
                InvokeRightClick();
                break;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (cancelHoldWhenPointerLeaves && isHoldingLeft)
        {
            ResetHoldState();
        }
    }

    private void ResetHoldState()
    {
        isHoldingLeft = false;
        holdTimer = 0f;
        holdTriggered = false;
    }

    private void CacheTargets()
    {
        targets.Clear();

        var behaviours = GetComponents<MonoBehaviour>();
        foreach (var behaviour in behaviours)
        {
            if (behaviour is IHoldableAndClickable target)
            {
                targets.Add(target);
            }
        }
    }

    private void InvokeHold()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].Hold();
        }
    }

    private void InvokeLeftClick()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].LeftClick();
        }
    }

    private void InvokeRightClick()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            targets[i].RightClick();
        }
    }
}