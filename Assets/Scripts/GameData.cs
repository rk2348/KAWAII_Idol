using System;
using System.Collections.Generic;

// 取引データ（未来の支払い・入金予約）
[Serializable]
public class Transaction
{
    public string description; // 取引内容
    public long amount;        // 金額（プラスは入金、マイナスは出金）
    public int dueDay;         // 決済日
    public bool isProcessed;   // 処理済みフラグ
}

// アイドルグループのステータス
[Serializable]
public class IdolGroup
{
    public string groupName = "FRUITS ZIPPER";
    public int fans = 1000;
    public int performance = 10;
    public int mental = 100;
    public int fatigue = 0;
    public IdolGenre genre = IdolGenre.KAWAII;

    // ★ここが抜けていました
    public bool hasDoneDome = false;
}

// アイドルのジャンル定義
public enum IdolGenre
{
    KAWAII,
    COOL,
    ROCK,
    TRADITIONAL
}

// --- 以下、Phase 2で追加したクラス群 ---

// スタッフ職種
public enum StaffType { Trainer, Marketer, Manager }

[Serializable]
public class Staff
{
    public string name;
    public StaffType type;
    public int level;       // 1~5
    public long monthlySalary; // 月給

    // 効果値（レベル×係数）
    public float GetEffectMultiplier()
    {
        return 1.0f + (level * 0.1f); // 例: Lv5なら1.5倍の効果
    }
}

// 会場データ（マスターデータ）
[Serializable]
public class Venue
{
    public string venueName;
    public int capacity;     // キャパ
    public long baseCost;    // 基本使用料
    public int minFansReq;   // 予約に必要な最低ファン数（足切り）
}

// 予約データ
[Serializable]
public class VenueBooking
{
    public Venue venue;
    public int eventDay;     // 開催日
    public bool isCanceled;  // キャンセル済みか
}