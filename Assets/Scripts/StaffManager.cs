using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class StaffManager : MonoBehaviour
{
    [Header("雇用中のスタッフ")]
    public List<Staff> hiredStaffs = new List<Staff>();

    private FinancialManager financial;

    public void Initialize(FinancialManager fm)
    {
        financial = fm;
    }

    // スタッフを雇う
    public void HireStaff(StaffType type, int level)
    {
        Staff newStaff = new Staff();
        newStaff.type = type;
        newStaff.level = level;
        newStaff.name = $"{type} Lv.{level}";

        // 給料計算（適当な係数）
        newStaff.monthlySalary = 200000 * level; // Lv1=20万, Lv5=100万

        // 契約金（給料の3ヶ月分とする）
        long contractFee = newStaff.monthlySalary * 3;

        if (financial.currentCash < contractFee)
        {
            Debug.LogError("資金不足で雇えません！");
            return;
        }

        financial.currentCash -= contractFee;
        hiredStaffs.Add(newStaff);

        Debug.Log($"【採用】{newStaff.name} を雇いました。契約金: -{contractFee:N0}円");
    }

    // 毎月の給料支払い（GameManagerから月末に呼ばれる）
    public void PayMonthlySalaries()
    {
        long totalSalary = hiredStaffs.Sum(s => s.monthlySalary);
        if (totalSalary > 0)
        {
            financial.currentCash -= totalSalary;
            Debug.Log($"<color=red>【人件費】スタッフ給与総額: -{totalSalary:N0}円</color>");
        }
    }

    // 特定の職種のボーナス効果を取得
    public float GetStaffBonus(StaffType type)
    {
        // その職種のスタッフの中で最強のレベルを適用（重複不可）
        var staff = hiredStaffs.Where(s => s.type == type).OrderByDescending(s => s.level).FirstOrDefault();
        return staff != null ? staff.GetEffectMultiplier() : 1.0f;
    }
}