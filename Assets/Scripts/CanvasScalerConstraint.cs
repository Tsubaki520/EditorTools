using UnityEngine;
using UnityEngine.UI;

public class CanvasScalerConstraint : MonoBehaviour
{
    [SerializeField] private CanvasScaler scaler;
    [SerializeField] private Constraint constraint;

    private enum Constraint
    {
        Fit,
        Fill,
        FitWidth,
        FitHeight,
    }

    private void Update()
    {
        UpdateScale();
    }

    private void UpdateScale()
    {
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;

        Vector2 reference = scaler.referenceResolution;
        float aspect = (float)Screen.width / Screen.height;
        float referenceAspect = reference.x / reference.y;

        switch (constraint)
        {
            case Constraint.FitWidth:
                scaler.matchWidthOrHeight = 0f;
                break;

            case Constraint.FitHeight:
                scaler.matchWidthOrHeight = 1f;
                break;

            case Constraint.Fit:
                if (referenceAspect > aspect)
                    scaler.matchWidthOrHeight = 0f;
                else
                    scaler.matchWidthOrHeight = 1f;
                break;

            case Constraint.Fill:
                if (referenceAspect > aspect)
                    scaler.matchWidthOrHeight = 1f;
                else
                    scaler.matchWidthOrHeight = 0f;
                break;
        }
    }
}