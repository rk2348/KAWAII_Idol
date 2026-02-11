using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("UI Objects")]
    public Text dateText;
    public Text cashText;
    public Text statusText;
    public Text ledgerText;
    public Text trendText;
    public Text bookingText; // ’Ç‰ÁF—\–ñƒŠƒXƒg•\¦—p

    private GameManager gameManager;

    public void Initialize(GameManager gm)
    {
        gameManager = gm;
    }

    public void RefreshUI()
    {
        if (gameManager == null) return;

        dateText.text = $"Day: {gameManager.currentDay}";
        cashText.text = $"Cash: ?{gameManager.financial.currentCash:N0}";
        cashText.color = gameManager.financial.currentCash < 0 ? Color.red : Color.white;

        var g = gameManager.idol.groupData;
        statusText.text = $"Fans: {g.fans:N0}\nPerf: {g.performance}\nMental: {g.mental}\nFatigue: {g.fatigue}";

        var m = gameManager.market;
        trendText.text = $"Trend: {m.currentTrend} {(m.isIceAge ? "<color=cyan>(ICE AGE)</color>" : "")}";

        // ’ •ëƒŠƒXƒg
        string ledgerStr = "y“üo‹à—\’èz\n";
        int count = 0;
        foreach (var t in gameManager.financial.pendingTransactions)
        {
            if (count >= 5) break;
            int daysLeft = t.dueDay - gameManager.currentDay;
            string color = t.amount >= 0 ? "green" : "red";
            ledgerStr += $"<color={color}>{t.description}: {t.amount:N0} ({daysLeft}“úŒã)</color>\n";
            count++;
        }
        ledgerText.text = ledgerStr;

        // —\–ñƒŠƒXƒg•\¦
        string bookingStr = "yƒ‰ƒCƒu—\–ñó‹µz\n";
        if (gameManager.idol.activeBookings.Count == 0)
        {
            bookingStr += "—\–ñ‚È‚µ";
        }
        else
        {
            foreach (var b in gameManager.idol.activeBookings)
            {
                int daysLeft = b.eventDay - gameManager.currentDay;
                bookingStr += $"{b.venue.venueName} (‚ ‚Æ{daysLeft}“ú)\n";
            }
        }
        bookingText.text = bookingStr;
    }
}