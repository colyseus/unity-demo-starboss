using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class StarBossPlayerTag : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playerTag;

    [SerializeField]
    private Image crosshairIcon;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform rectTransform;

    [SerializeField]
    private Color friendlyColor;

    [SerializeField]
    private Color enemyColor;

    private bool friendly = false;

    void Awake()
    {
        SetColors();
    }

    public void SetPlayerTag(string tag)
    {
        playerTag.text = tag;
    }

    public void UpdateTag(Vector2 position, float alpha, bool isFriendly)
    {
        rectTransform.anchoredPosition = position;
        canvasGroup.alpha = alpha;

        if (isFriendly != friendly)
        {
            friendly = isFriendly;
            SetColors();
        }
    }

    private void SetColors()
    {
        crosshairIcon.color = friendly ? friendlyColor : enemyColor;
        playerTag.color = friendly ? friendlyColor : enemyColor;
    }
}
