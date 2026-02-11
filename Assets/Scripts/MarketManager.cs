using UnityEngine;

public class MarketManager : MonoBehaviour
{
    [Header("現在の市場トレンド")]
    public IdolGenre currentTrend = IdolGenre.KAWAII;
    public bool isIceAge = false; // アイドル氷河期フラグ

    // トレンド更新（30日ごとに呼ぶなど）
    public void UpdateTrendRandomly()
    {
        // ランダムにトレンドを変更
        currentTrend = (IdolGenre)Random.Range(0, System.Enum.GetValues(typeof(IdolGenre)).Length);

        // 低確率で氷河期到来（5%）
        if (Random.Range(0, 100) < 5)
        {
            isIceAge = true;
            Debug.Log("<color=blue>【ニュース】アイドルブーム終了！？ 市場が冷え込んでいます（氷河期突入）</color>");
        }
        else
        {
            isIceAge = false; // 回復
            Debug.Log($"【トレンド変化】現在の流行は {currentTrend} です！");
        }
    }

    // 集客倍率を計算する
    public float GetMarketMultiplier(IdolGenre groupGenre)
    {
        float multiplier = 1.0f;

        // 氷河期なら半減
        if (isIceAge) multiplier *= 0.5f;

        // トレンド一致ならボーナス、不一致ならペナルティ
        if (groupGenre == currentTrend)
        {
            multiplier *= 1.5f; // ブームに乗っている
        }
        else
        {
            multiplier *= 0.8f; // 時代遅れ
        }

        return multiplier;
    }
}