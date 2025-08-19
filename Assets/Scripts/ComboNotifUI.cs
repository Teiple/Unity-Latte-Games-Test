using UnityEngine;
using UnityEngine.UI;
using DigitalRuby.Tween;

public class ComboNotifUI : MonoBehaviour
{
    [SerializeField] private string comboTextFormat = "Combo x{0}\n+{1} coins";
    [SerializeField] private Text comboNotifText;

    [Header("Animation Settings")]
    [SerializeField] private AnimationCurve fadeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float animationDuration = 2f;

    private void Start()
    {
        SetAlpha(0f);
    }

    public void SetCombo(int combo, int rewards)
    {
        comboNotifText.text = string.Format(comboTextFormat, combo, rewards);
        comboNotifText.gameObject.SetActive(true);

        gameObject.Tween("ComboFade", 0f, 1f, animationDuration,
            TweenScaleFunctions.Linear,
            (t) =>
            {
                float alpha = fadeCurve.Evaluate(t.CurrentValue);
                SetAlpha(alpha);
            },
            (t) =>
            {
                comboNotifText.gameObject.SetActive(false);
            });
    }

    private void SetAlpha(float alpha)
    {
        Color c = comboNotifText.color;
        c.a = alpha;
        comboNotifText.color = c;
    }
}
