using UnityEngine;
using UnityEngine.UI;

public class SetupPanel : MonoBehaviour
{
    [Header("UI References")]
    public InputField nameInputField;
    public Slider memberCountSlider;
    public Text memberCountText;

    private GameManager gameManager;
    private int selectedMemberCount = 1;

    public void Setup(GameManager gm)
    {
        gameManager = gm;
        // デフォルト値
        nameInputField.text = "My Idol Group";
        memberCountSlider.value = 5;
        UpdateMemberCountText(5);

        // スライダー変更時のイベント登録
        memberCountSlider.onValueChanged.AddListener(UpdateMemberCountText);

        this.gameObject.SetActive(false);
    }

    public void Open()
    {
        this.gameObject.SetActive(true);
    }

    public void UpdateMemberCountText(float value)
    {
        selectedMemberCount = (int)value;
        memberCountText.text = $"メンバー数: {selectedMemberCount}人";
    }

    // 決定ボタン
    public void OnConfirmClick()
    {
        string groupName = nameInputField.text;
        if (string.IsNullOrEmpty(groupName)) groupName = "名無しアイドル";

        gameManager.OnSetupConfirmed(groupName, selectedMemberCount);
        this.gameObject.SetActive(false);
    }
}