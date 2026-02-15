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

    public void CheckDailyEvent(DailyReport report)
    {
        int dice = Random.Range(0, 100);

        if (dice < 3) TriggerBadEvent(report);
        else if (dice >= 98) TriggerGoodEvent(report);
    }

    void TriggerBadEvent(DailyReport report)
    {
        int type = Random.Range(0, 3);
        switch (type)
        {
            case 0:
                report.AddLog("<color=red>【炎上】</color> SNSで失言！ファン減少...");
                int lostLight = (int)(idolManager.groupData.fansLight * 0.2f);
                int lostCore = (int)(idolManager.groupData.fansCore * 0.05f);

                idolManager.groupData.fansLight -= lostLight;
                idolManager.groupData.fansCore -= lostCore;
                idolManager.groupData.mental -= 20;

                report.AddLog($"ファン離脱 (Light:-{lostLight} / Core:-{lostCore})");
                break;
            case 1:
                report.AddLog("<color=red>【破損】</color> 衣装トラブルで緊急出費！");
                financial.currentCash -= 300000;
                financial.dailyCashChange -= 300000;
                break;
            case 2:
                report.AddLog("<color=red>【内紛】</color> メンバー喧嘩発生。メンタル低下。");
                idolManager.groupData.mental -= 30;
                break;
        }
    }

    void TriggerGoodEvent(DailyReport report)
    {
        report.AddLog("<color=yellow>【バズり】</color> 動画が大ヒット！ファン急増！");
        int newLight = (int)(idolManager.groupData.fans * 0.3f);
        int newYakkai = Random.Range(1, 10);

        idolManager.groupData.fansLight += newLight;
        idolManager.groupData.fansYakkai += newYakkai;
        idolManager.groupData.mental += 10;

        report.AddLog($"新規ファン+{newLight}人 (うち厄介+{newYakkai}人)");
    }
}