using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Reflection;

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

    private readonly List<MonoBehaviour> targets = new();

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
            if (HasTargetMethods(behaviour))
            {
                targets.Add(behaviour);
            }
        }
    }

    private bool HasTargetMethods(MonoBehaviour behaviour)
    {
        return behaviour != null
            && behaviour.GetType().GetMethod("Hold", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null
            && behaviour.GetType().GetMethod("LeftClick", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null
            && behaviour.GetType().GetMethod("RightClick", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) != null;
    }

    private void InvokeTargetMethod(MonoBehaviour target, string methodName)
    {
        if (target == null)
        {
            return;
        }

        MethodInfo method = target.GetType().GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        method?.Invoke(target, null);
    }

    private void InvokeHold()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            InvokeTargetMethod(targets[i], "Hold");
        }
    }

    private void InvokeLeftClick()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            InvokeTargetMethod(targets[i], "LeftClick");
        }
    }

    private void InvokeRightClick()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            InvokeTargetMethod(targets[i], "RightClick");
        }
    }
}