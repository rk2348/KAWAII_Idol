using UnityEngine;

public class EventManager : MonoBehaviour
{
    private IdolManager idolManager;
    private FinancialManager financial;

    public void Initialize(IdolManager im, FinancialManager fm)
    {
        idolManager = im;
        financial = fm;
    }

    // 毎日呼び出してイベント判定
    public void CheckDailyEvent()
    {
        int dice = Random.Range(0, 100);

        // 3%の確率でトラブル発生
        if (dice < 3)
        {
            TriggerBadEvent();
        }
        // 1%の確率でラッキーイベント
        else if (dice >= 99)
        {
            TriggerGoodEvent();
        }
    }

    void TriggerBadEvent()
    {
        // 簡易的なランダム分岐
        int type = Random.Range(0, 3);
        switch (type)
        {
            case 0: // 炎上
                Debug.LogWarning("【炎上発生】SNSでの不用意な発言が炎上！ファン減少...");
                idolManager.groupData.fans = (int)(idolManager.groupData.fans * 0.9f);
                idolManager.groupData.mental -= 20;
                break;
            case 1: // 衣装破損
                Debug.LogWarning("【トラブル】衣装のサイズが合わず作り直し！修繕費発生。");
                financial.currentCash -= 300000;
                break;
            case 2: // メンバー喧嘩
                Debug.LogWarning("【内紛】メンバー間で喧嘩勃発。メンタル激減。");
                idolManager.groupData.mental -= 30;
                break;
        }
    }

    void TriggerGoodEvent()
    {
        Debug.Log("<color=yellow>【バズり】TikTok動画が大バズり！ファン急増！</color>");
        idolManager.groupData.fans = (int)(idolManager.groupData.fans * 1.3f);
        idolManager.groupData.mental += 10;
    }
}