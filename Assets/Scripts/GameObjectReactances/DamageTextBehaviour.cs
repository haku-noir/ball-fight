using UnityEngine;
using TMPro;

public class DamageTextBehaviour : MonoBehaviour
{
    private TextMeshProUGUI textMeshProUGUI;

    void Start()
    {
        textMeshProUGUI = gameObject.GetComponent<TextMeshProUGUI>();
    }

    public void React(Damage damage)
    {
        textMeshProUGUI.SetText(damage.Value * 100 + "%");
    }
}
