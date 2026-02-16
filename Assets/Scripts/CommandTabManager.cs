using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // 標準のTextを使うために必要

[System.Serializable]
public class TabSet
{
    public string name;             // 識別用（例: Work, Live）
    public Button tabButton;        // タブのボタン
    public GameObject contentPanel; // 切り替えるパネル（コマンドグループ）
    //public Image underlineImage;    // アクティブ時に表示する下線（Imageコンポーネント）
    public Text buttonText;         // ★変更点：標準のTextコンポーネントを使用

    [Header("Color Settings")]
    public Color activeColor;       // アクティブ時のテーマカラー
}

public class CommandTabManager : MonoBehaviour
{
    [Header("Settings")]
    public List<TabSet> tabs = new List<TabSet>();

    // 非アクティブ時のテキスト色（グレー）
    [SerializeField] private Color defaultTextColor = new Color(0.5f, 0.55f, 0.55f);

    // フェードイン演出の時間
    [SerializeField] private float fadeDuration = 0.2f;

    void Start()
    {
        // 全てのボタンにイベントを登録
        foreach (var tab in tabs)
        {
            // ラムダ式で引数を渡す
            tab.tabButton.onClick.AddListener(() => SwitchTab(tab));

            // パネルにCanvasGroupがない場合は自動追加（フェード演出用）
            if (tab.contentPanel.GetComponent<CanvasGroup>() == null)
            {
                tab.contentPanel.AddComponent<CanvasGroup>();
            }
        }

        // 初期状態でリストの最初のタブを選択
        if (tabs.Count > 0)
        {
            SwitchTab(tabs[0]);
        }
    }

    public void SwitchTab(TabSet selectedTab)
    {
        foreach (var tab in tabs)
        {
            bool isActive = (tab == selectedTab);

            // 1. パネルの表示切り替え（アクティブなら表示＋フェードイン）
            if (isActive)
            {
                // すでにアクティブなら再フェードしない（オプション）
                if (!tab.contentPanel.activeSelf)
                {
                    tab.contentPanel.SetActive(true);
                    StartCoroutine(FadeInPanel(tab.contentPanel));
                }
            }
            else
            {
                tab.contentPanel.SetActive(false);
            }

            // 2. ボタンの見た目切り替え（下線の表示/非表示）
            /*if (tab.underlineImage != null)
            {
                tab.underlineImage.enabled = isActive;
                if (isActive)
                {
                    tab.underlineImage.color = tab.activeColor;
                }
            }*/

            // 3. 文字色の切り替え
            if (tab.buttonText != null)
            {
                tab.buttonText.color = isActive ? tab.activeColor : defaultTextColor;

                // ★変更点：標準Textのフォントスタイル変更
                tab.buttonText.fontStyle = isActive ? FontStyle.Bold : FontStyle.Normal;
            }
        }
    }

    // CSSの @keyframes fadeIn を再現するコルーチン
    IEnumerator FadeInPanel(GameObject panel)
    {
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();

        // 初期化
        cg.alpha = 0;
        // 少し下から浮き上がる演出のため位置をずらす
        Vector3 startPos = panel.transform.localPosition + (Vector3.up * 10f);
        Vector3 endPos = panel.transform.localPosition;

        panel.transform.localPosition = startPos;

        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // アルファ値と位置をアニメーション
            cg.alpha = Mathf.Lerp(0f, 1f, progress);
            panel.transform.localPosition = Vector3.Lerp(startPos, endPos, progress);

            yield return null;
        }

        // 最終的な値をセット
        cg.alpha = 1f;
        panel.transform.localPosition = endPos;
    }
}