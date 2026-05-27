// /Repositories/IReportRepository.cs
using System;
using System.Collections.Generic;

namespace RPACProductionPlanner.Repositories
{
    public class AnalyticsKpiResult
    {
        public long TotalThroughput { get; set; }
        public int TotalOrders { get; set; }
        public int ActiveOrders { get; set; }
        public int OverdueOrders { get; set; }
        public double OtdRate { get; set; }
    }

    public interface IReportRepository
    {
        IEnumerable<dynamic> GetDepartmentVolume();
        IEnumerable<dynamic> GetMonthlyTrend();
        AnalyticsKpiResult GetAnalyticsKpis();
    }
}
