using UnityEngine;

public class MarketManager : MonoBehaviour
{
    public IdolGenre currentTrend = IdolGenre.KAWAII;
    public bool isIceAge = false;

    public void UpdateTrendRandomly(DailyReport report)
    {
        // 30“ú‚²‚Æ‚ÌƒgƒŒƒ“ƒh•Ï‰»
        IdolGenre prevTrend = currentTrend;
        currentTrend = (IdolGenre)Random.Range(0, System.Enum.GetValues(typeof(IdolGenre)).Length);

        if (prevTrend != currentTrend)
        {
            report.AddLog($"ysêzƒgƒŒƒ“ƒh‚ª {prevTrend} ‚©‚ç {currentTrend} ‚É•Ï‰»I");
        }

        // •X‰ÍŠú”»’è (5%)
        if (Random.Range(0, 100) < 5)
        {
            isIceAge = true;
            report.AddLog("<color=blue>yƒjƒ…[ƒXzƒAƒCƒhƒ‹•X‰ÍŠú“—ˆIsê‚ª—â‚¦‚ñ‚Å‚¢‚Ü‚·...</color>");
        }
        else if (isIceAge)
        {
            isIceAge = false;
            report.AddLog("yƒjƒ…[ƒXzsê‚ÌŒi‹C‚ª‰ñ•œ‚µ‚Ü‚µ‚½I");
        }
    }

    public float GetMarketMultiplier(IdolGenre groupGenre)
    {
        float multiplier = 1.0f;
        if (isIceAge) multiplier *= 0.5f;
        if (groupGenre == currentTrend) multiplier *= 1.5f;
        else multiplier *= 0.8f;
        return multiplier;
    }
}