using UnityEngine;

public abstract class CesiumCameraAction : ScriptableObject
{
    /// <summary>
    /// Check if this action is triggered.
    /// </summary>
    protected abstract bool IsTriggered(ICesiumCameraContext context);

    /// <summary>
    /// Check if this action can be interrupted by another action.
    /// </summary>
    protected virtual bool CanBeInterruptedBy(CesiumCameraAction otherAction) { return true; }

    /// <summary>
    /// Called once when the action is started.
    /// </summary>
    protected virtual void OnBegin(ICesiumCameraContext context) { }

    /// <summary>
    /// Called every frame when the action is updated.
    /// </summary>
    /// <returns>If the action can be finished.</returns>
    protected virtual bool OnUpdate(ICesiumCameraContext context) { return true; }

    /// <summary>
    /// Called once when the action is finished.
    /// </summary>
    protected virtual void OnEnd(ICesiumCameraContext context) { }

    private bool CanStart(ICesiumCameraContext context)
    {
        /*
        if (context.CesiumCameraAction != null)
            Debug.Log("IsTriggered: " + IsTriggered(context) + " and CanBeInterruptedBy: " + context.CesiumCameraAction.CanBeInterruptedBy(this));
        else
            Debug.Log("context.CameraAction is null");
        */
        if (!IsTriggered(context))
            return false;

        if (context.CesiumCameraAction != null && !context.CesiumCameraAction.CanBeInterruptedBy(this))
            return false;

        return true;
    }

    public static void Process(ICesiumCameraContext context)
    {
        foreach (var action in context.CesiumCameraActions)
        {
            if (action.CanStart(context))
            {
                if (context.CesiumCameraAction != null)
                {
                    context.CesiumCameraAction.OnEnd(context);
                }
                //Debug.Log("Begin Action: " + action.name);
                context.CesiumCameraAction = action;
                context.CesiumCameraAction.OnBegin(context);
                break;
            }
        }

        if (context.CesiumCameraAction != null && context.CesiumCameraAction.OnUpdate(context))
        {
            context.CesiumCameraAction.OnEnd(context);
            context.CesiumCameraAction = null;
        }
    }
}
