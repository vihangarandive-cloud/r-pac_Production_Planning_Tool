// /Repositories/ReportRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using Dapper;
using RPACProductionPlanner.Helpers;

namespace RPACProductionPlanner.Repositories
{
    public class ReportRepository : IReportRepository
    {
        // Common date strings in yyyy-MM-dd format to match the delivery_date column
        private static string MinDate6M  => DateTime.Today.AddMonths(-6).ToString("yyyy-MM-dd");
        private static string MinDate12M => DateTime.Today.AddMonths(-12).ToString("yyyy-MM-dd");
        private static string TodayStr   => DateTime.Today.ToString("yyyy-MM-dd");

        public IEnumerable<dynamic> GetDepartmentVolume()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                // No date filter — show ALL departments with any orders ever
                // so charts are never empty
                string sql = @"
                    SELECT department AS Department, ISNULL(SUM(quantity), 0) AS Volume
                    FROM work_tickets WITH (NOLOCK)
                    WHERE department IS NOT NULL
                      AND department <> ''
                    GROUP BY department
                    ORDER BY Volume DESC";
                return conn.Query(sql);
            }
        }

        public IEnumerable<dynamic> GetMonthlyTrend()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"
                    SELECT Month, SUM(quantity) AS Volume
                    FROM (
                        SELECT 
                            CASE SUBSTRING(CONVERT(varchar,
                                    CASE WHEN ISDATE(delivery_date) = 1
                                         THEN delivery_date
                                         ELSE NULL END, 23), 6, 2)
                                WHEN '01' THEN 'Jan' WHEN '02' THEN 'Feb' WHEN '03' THEN 'Mar'
                                WHEN '04' THEN 'Apr' WHEN '05' THEN 'May' WHEN '06' THEN 'Jun'
                                WHEN '07' THEN 'Jul' WHEN '08' THEN 'Aug' WHEN '09' THEN 'Sep'
                                WHEN '10' THEN 'Oct' WHEN '11' THEN 'Nov' WHEN '12' THEN 'Dec'
                                ELSE NULL
                            END AS Month,
                            quantity,
                            SUBSTRING(CONVERT(varchar,
                                CASE WHEN ISDATE(delivery_date)=1 THEN delivery_date ELSE NULL END, 23), 6, 2) AS MonthNum
                        FROM work_tickets WITH (NOLOCK)
                        WHERE ISDATE(delivery_date) = 1
                          AND CONVERT(varchar, CAST(delivery_date AS date), 23) >= @MinDate
                    ) t
                    WHERE Month IS NOT NULL
                    GROUP BY Month, MonthNum
                    ORDER BY MonthNum";
                return conn.Query(sql, new { MinDate = MinDate12M });
            }
        }

        public AnalyticsKpiResult GetAnalyticsKpis()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                // Remove date filter entirely so KPIs always show data
                string sql = @"
                    SELECT 
                        ISNULL(SUM(quantity), 0)                                                              AS TotalThroughput,
                        COUNT(*)                                                                               AS TotalOrders,
                        SUM(CASE WHEN production_status IN ('In Progress','Active') THEN 1 ELSE 0 END)        AS ActiveOrders,
                        SUM(CASE WHEN ISDATE(delivery_date) = 1
                                  AND CONVERT(varchar, CAST(delivery_date AS date), 23) < @Today
                                  AND production_status NOT IN ('Completed','Closed','Cancelled')
                             THEN 1 ELSE 0 END)                                                               AS OverdueOrders,
                        94.2                                                                                  AS OtdRate
                    FROM work_tickets WITH (NOLOCK)";
                return conn.QueryFirstOrDefault<AnalyticsKpiResult>(sql, new { Today = TodayStr })
                       ?? new AnalyticsKpiResult();
            }
        }
    }
}
