using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public class SetlistPanel : MonoBehaviour
{
    [Header("UI References")]
    public Text titleText;       // 「○○のセットリスト」
    public Text infoText;        // 「選択中: 3/5曲」など
    public Transform contentRoot; // ScrollViewのContent
    public GameObject songButtonPrefab; // 曲リストのボタンのプレハブ
    public Button confirmButton;

    private IdolManager idolManager;
    private VenueBooking targetBooking;
    private List<Song> tempSetlist = new List<Song>(); // 編集中のリスト

    // パネルを開くときの初期化処理
    public void Open(IdolManager manager)
    {
        idolManager = manager;
        this.gameObject.SetActive(true);

        // 直近のキャンセルされていないライブを探す
        targetBooking = idolManager.activeBookings
            .OrderBy(b => b.eventDay)
            .FirstOrDefault(b => !b.isCanceled);

        if (targetBooking == null)
        {
            titleText.text = "ライブの予定がありません";
            infoText.text = "先に会場を予約してください。";
            contentRoot.gameObject.SetActive(false);
            confirmButton.interactable = true; // 閉じる専用
            return;
        }

        // 現在保存されているリストをコピーして編集開始
        tempSetlist = new List<Song>(targetBooking.setlist);

        // UI表示更新
        titleText.text = $"{targetBooking.venue.venueName} (Day {targetBooking.eventDay})\nセットリスト構成会議";
        contentRoot.gameObject.SetActive(true);

        RefreshList();
    }

    // リストの再描画
    void RefreshList()
    {
        // 既存のボタンを削除
        foreach (Transform child in contentRoot) Destroy(child.gameObject);

        // 曲一覧を生成
        foreach (var song in idolManager.groupData.discography)
        {
            GameObject obj = Instantiate(songButtonPrefab, contentRoot);
            Button btn = obj.GetComponent<Button>();
            Text txt = obj.GetComponentInChildren<Text>();

            bool isSelected = tempSetlist.Contains(song);

            // 表示テキスト作成
            string mark = isSelected ? "<color=yellow>★選択中</color>" : "　";
            txt.text = $"{mark} {song.title}\n<size=12>Q:{song.quality} / {song.genre}</size>";

            // ボタンの色を変える（選択中は明るく、未選択は暗くなど）
            ColorBlock cb = btn.colors;
            cb.normalColor = isSelected ? new Color(0.6f, 1f, 0.6f) : Color.white;
            btn.colors = cb;

            // クリック時の動作
            btn.onClick.AddListener(() => OnSongClicked(song));
        }

        UpdateInfo();
    }

    // 曲をクリックしたときの処理
    void OnSongClicked(Song song)
    {
        if (tempSetlist.Contains(song))
        {
            // 既に選ばれていたら外す
            tempSetlist.Remove(song);
        }
        else
        {
            // 選ばれていない場合、上限数未満なら追加
            if (tempSetlist.Count < targetBooking.venue.maxSongs)
            {
                tempSetlist.Add(song);
            }
        }
        RefreshList();
    }

    void UpdateInfo()
    {
        if (targetBooking == null) return;
        int max = targetBooking.venue.maxSongs;
        int current = tempSetlist.Count;
        infoText.text = $"選択数: <color={(current == max ? "green" : "white")}>{current}</color> / {max} 曲";
    }

    // 決定ボタン
    public void OnConfirmClick()
    {
        if (targetBooking != null)
        {
            // マネージャーに保存を依頼（前回のコードで追加したメソッド）
            idolManager.RegisterSetlist(targetBooking, tempSetlist);
        }
        this.gameObject.SetActive(false); // 閉じる
    }
}