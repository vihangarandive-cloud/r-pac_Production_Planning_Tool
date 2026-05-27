// /Repositories/SchedulerRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Dapper;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public class SchedulerRepository : ISchedulerRepository
    {
        public IEnumerable<ProductionOrder> GetMachineSchedule(DateTime start, DateTime end, string department = null)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                // Align with Dashboard logic but optimized for 1-second load
                string minDate = DateTime.Today.AddMonths(-6).ToString("yyyy-MM-dd");
                
                string sql = @"
                    -- 1. All Planned Orders (so Gantt chart has all of them)
                    SELECT 
                        wt.work_ticket_no as OrderCode, 
                        wt.customer_po_no as SalesOrderNo, 
                        wt.customer as CustomerName, 
                        wt.label_ref as ProductName, 
                        wt.department as Department, 
                        wt.quantity as Quantity, 
                        wt.production_status as Status,
                        wt.delivery_date as OrderedDate,
                        lp.PlannedStart as PlannedStart,
                        lp.PlannedEnd as PlannedEnd,
                        lp.MachineId as MachineId,
                        lp.Priority as Priority,
                        lp.Notes as Notes
                    FROM work_tickets wt WITH (NOLOCK)
                    INNER JOIN ProductionOrders lp WITH (NOLOCK) ON wt.work_ticket_no = lp.OrderCode
                    WHERE (@Department IS NULL OR wt.department = @Department)
                      AND lp.PlannedStart IS NOT NULL

                    UNION ALL

                    -- 2. Top 150 Unplanned Orders (for sidebar bin)
                    SELECT TOP 150
                        wt.work_ticket_no as OrderCode, 
                        wt.customer_po_no as SalesOrderNo, 
                        wt.customer as CustomerName, 
                        wt.label_ref as ProductName, 
                        wt.department as Department, 
                        wt.quantity as Quantity, 
                        wt.production_status as Status,
                        wt.delivery_date as OrderedDate,
                        NULL as PlannedStart,
                        NULL as PlannedEnd,
                        0 as MachineId,
                        'N/A' as Priority,
                        NULL as Notes
                    FROM work_tickets wt WITH (NOLOCK)
                    LEFT JOIN ProductionOrders lp WITH (NOLOCK) ON wt.work_ticket_no = lp.OrderCode
                    WHERE (@Department IS NULL OR wt.department = @Department)
                      AND (lp.PlannedStart IS NULL OR lp.OrderCode IS NULL)
                      AND wt.delivery_date >= @MinDate
                    ORDER BY OrderedDate DESC";

                return conn.Query<ProductionOrder>(sql, new { MinDate = minDate, Department = department }).ToList();
            }
        }

        public IEnumerable<MachineResource> GetAllMachines()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                return conn.Query<MachineResource>("SELECT * FROM MachineResources WITH (NOLOCK)");
            }
        }

        public IEnumerable<dynamic> GetUPSData() { return new List<dynamic>(); }

        public IEnumerable<string> GetDepartments()
        {
            // Hardcoded list is faster than DISTINCT query on large table
            return new List<string> { "PFL", "RFID", "Thermal", "Heat Transfer", "Offset", "Flexo", "Digital" };
        }
    }
}
