using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Text = TMPro.TMP_Text;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class UIHooks : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField] Text healthText = default;
    [SerializeField] Image healthBar = default;
    [SerializeField] Image healthBarEffect = default;
    [SerializeField] Image fadeImage;
#pragma warning restore 0649
    public void SetHealth(int current, int total)
    {
        if (healthText != null)
        {
            healthText.text = $"{current}/{total}";
        }

        if (healthBar != null)
        {
            healthBar.fillAmount = current / (float) total;
        }

        if (healthBarEffect != null)
        {
            healthBarEffect.fillAmount = current / (float)total;
        }
    }

    public void ReloadScene()
    {
        fadeImage.DOFade(1, 3).OnComplete(() => SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().name));
    }
}
