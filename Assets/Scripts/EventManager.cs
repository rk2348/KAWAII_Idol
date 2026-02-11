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
        else if (dice >= 98) TriggerGoodEvent(report); // 2%
    }

    void TriggerBadEvent(DailyReport report)
    {
        int type = Random.Range(0, 3);
        switch (type)
        {
            case 0:
                report.AddLog("<color=red>y‰Šãz</color> SNS‚Å¸Œ¾Iƒtƒ@ƒ“Œ¸­...");
                idolManager.groupData.fans = (int)(idolManager.groupData.fans * 0.9f);
                idolManager.groupData.mental -= 20;
                break;
            case 1:
                report.AddLog("<color=red>y”j‘¹z</color> ˆß‘•ƒgƒ‰ƒuƒ‹‚Å‹Ù‹}o”ïI");
                financial.currentCash -= 300000;
                financial.dailyCashChange -= 300000;
                break;
            case 2:
                report.AddLog("<color=red>y“à•´z</color> ƒƒ“ƒo[Œ–‰Ü”­¶Bƒƒ“ƒ^ƒ‹’á‰ºB");
                idolManager.groupData.mental -= 30;
                break;
        }
    }

    void TriggerGoodEvent(DailyReport report)
    {
        report.AddLog("<color=yellow>yƒoƒY‚èz</color> “®‰æ‚ª‘åƒqƒbƒgIƒtƒ@ƒ“‹}‘I");
        idolManager.groupData.fans = (int)(idolManager.groupData.fans * 1.3f);
        idolManager.groupData.mental += 10;
    }
}