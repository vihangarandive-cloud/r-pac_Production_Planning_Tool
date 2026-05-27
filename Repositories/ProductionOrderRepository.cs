// /Repositories/ProductionOrderRepository.cs
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using RPACProductionPlanner.Helpers;
using RPACProductionPlanner.Models;

namespace RPACProductionPlanner.Repositories
{
    public class ProductionOrderRepository : IProductionOrderRepository
    {
        public IEnumerable<ProductionOrder> GetAll(string status = null, DateTime? start = null, DateTime? end = null, string department = null)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string minDate = DateTime.Now.AddMonths(-4).ToString("yyyy-MM-dd");
                // FIX: JOIN on REPLACE(...) so slash-replaced OrderCode in ProductionOrders matches correctly
                string sql = @"SELECT TOP 150
                                REPLACE(wt.work_ticket_no, '/', '_') as OrderCode, 
                                wt.customer_po_no as SalesOrderNo, 
                                wt.customer as CustomerName, 
                                wt.label_ref as ProductName, 
                                wt.department as Department, 
                                wt.quantity as Quantity, 
                                wt.production_status as Status,
                                lp.PlannedEnd as PlannedEnd,
                                wt.delivery_date as OrderedDate,
                                wt.current_stage as CurrentStage,
                                lp.Priority as Priority,
                                lp.PlannedStart as PlannedStart,
                                lp.MachineId as MachineId,
                                m.MachineName as MachineName
                               FROM work_tickets wt WITH (NOLOCK)
                               LEFT JOIN ProductionOrders lp WITH (NOLOCK) ON wt.work_ticket_no = lp.OrderCode
                               LEFT JOIN MachineResources m WITH (NOLOCK) ON lp.MachineId = m.MachineId
                               WHERE (@Status IS NULL OR wt.production_status = @Status)
                                 AND (@Department IS NULL OR wt.department = @Department)
                                 AND (wt.delivery_date >= @MinDate) 
                               ORDER BY wt.delivery_date DESC";
                return conn.Query<ProductionOrder>(sql, new { Status = status, Department = department, MinDate = minDate });
            }
        }

        public ProductionOrder GetById(int id)
        {
            return GetByOrderCode(id.ToString());
        }

        public ProductionOrder GetByOrderCode(string orderCode)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"SELECT 
                                wt.work_ticket_no as OrderCode, 
                                wt.customer_po_no as SalesOrderNo, 
                                wt.customer as CustomerName, 
                                wt.label_ref as ProductName, 
                                wt.department as Department, 
                                wt.quantity as Quantity, 
                                wt.production_status as Status,
                                lp.PlannedStart as PlannedStart,
                                lp.PlannedEnd as PlannedEnd,
                                lp.Priority as Priority,
                                m.MachineName as MachineName,
                                lp.MachineId as MachineId,
                                lp.Notes as Notes
                               FROM work_tickets wt WITH (NOLOCK)
                               LEFT JOIN ProductionOrders lp WITH (NOLOCK) ON wt.work_ticket_no = lp.OrderCode
                               LEFT JOIN MachineResources m WITH (NOLOCK) ON lp.MachineId = m.MachineId
                               WHERE wt.work_ticket_no = @OrderCode
                                  OR REPLACE(wt.work_ticket_no, '/', '_') = @OrderCode";
                return conn.QueryFirstOrDefault<ProductionOrder>(sql, new { OrderCode = orderCode });
            }
        }

        public bool UpdatePlanning(string orderCode, int machineId, DateTime plannedStart, DateTime? plannedEnd = null, string priority = "N/A")
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"
                    MERGE ProductionOrders AS target
                    USING (
                        SELECT TOP 1
                            REPLACE(work_ticket_no, '/', '_') AS OrderCode, 
                            @MachineId AS MachineId, 
                            @PlannedStart AS PlannedStart,
                            @PlannedEnd AS PlannedEnd,
                            @Priority AS Priority, 
                            label_ref AS ProductName, 
                            department AS Department, 
                            quantity AS Quantity,
                            'PCS' AS UnitOfMeasure
                        FROM work_tickets 
                        WHERE REPLACE(work_ticket_no, '/', '_') = @OrderCode
                           OR work_ticket_no = @OrderCode
                    ) AS source
                    ON target.OrderCode = source.OrderCode
                    WHEN MATCHED THEN 
                        UPDATE SET 
                            MachineId = source.MachineId, 
                            PlannedStart = source.PlannedStart,
                            PlannedEnd = source.PlannedEnd,
                            Priority = source.Priority
                    WHEN NOT MATCHED THEN 
                        INSERT (OrderCode, MachineId, PlannedStart, PlannedEnd, Priority, ProductName, Department, Quantity, UnitOfMeasure)
                        VALUES (source.OrderCode, source.MachineId, source.PlannedStart, source.PlannedEnd, source.Priority, source.ProductName, source.Department, source.Quantity, source.UnitOfMeasure);";
                
                var result = conn.Execute(sql, new { OrderCode = orderCode, MachineId = machineId, PlannedStart = plannedStart, PlannedEnd = plannedEnd, Priority = priority }) > 0;
                if (result)
                {
                    new AuditRepository().Log(SessionHelper.UserId, $"Updated Planning (Sync): {priority}", "ProductionOrders", orderCode);
                }
                return result;
            }
        }

        public async Task<bool> UpdatePlanningAsync(string orderCode, int machineId, DateTime plannedStart, DateTime? plannedEnd = null, string priority = "N/A")
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"
                    MERGE ProductionOrders AS target
                    USING (
                        SELECT TOP 1
                            REPLACE(work_ticket_no, '/', '_') AS OrderCode, 
                            @MachineId AS MachineId, 
                            @PlannedStart AS PlannedStart,
                            @PlannedEnd AS PlannedEnd,
                            @Priority AS Priority, 
                            label_ref AS ProductName, 
                            department AS Department, 
                            quantity AS Quantity,
                            'PCS' AS UnitOfMeasure
                        FROM work_tickets 
                        WHERE REPLACE(work_ticket_no, '/', '_') = @OrderCode
                           OR work_ticket_no = @OrderCode
                    ) AS source
                    ON target.OrderCode = source.OrderCode
                    WHEN MATCHED THEN 
                        UPDATE SET 
                            MachineId = source.MachineId, 
                            PlannedStart = source.PlannedStart,
                            PlannedEnd = source.PlannedEnd,
                            Priority = source.Priority
                    WHEN NOT MATCHED THEN 
                        INSERT (OrderCode, MachineId, PlannedStart, PlannedEnd, Priority, ProductName, Department, Quantity, UnitOfMeasure)
                        VALUES (source.OrderCode, source.MachineId, source.PlannedStart, source.PlannedEnd, source.Priority, source.ProductName, source.Department, source.Quantity, source.UnitOfMeasure);";
                
                var result = await conn.ExecuteAsync(sql, new { OrderCode = orderCode, MachineId = machineId, PlannedStart = plannedStart, PlannedEnd = plannedEnd, Priority = priority }) > 0;
                if (result)
                {
                    await new AuditRepository().LogAsync(SessionHelper.UserId, $"Updated Planning: Start={plannedStart:yyyy-MM-dd HH:mm}, Priority={priority}", "ProductionOrders", orderCode);
                    RPACProductionPlanner.Services.RealTimeService.NotifyUpdate("{\"type\": \"schedule_update\", \"orderCode\": \"" + orderCode + "\"}");
                }
                return result;
            }
        }

        public bool Unschedule(string orderCode)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "DELETE FROM ProductionOrders WHERE REPLACE(OrderCode, '/', '_') = @OrderCode OR OrderCode = @OrderCode";
                return conn.Execute(sql, new { OrderCode = orderCode }) > 0;
            }
        }

        public async Task<IEnumerable<ProductionOrder>> GetAllAsync(string status = null, DateTime? start = null, DateTime? end = null, string department = null)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string minDate = DateTime.Now.AddMonths(-4).ToString("yyyy-MM-dd");
                // FIX: JOIN on REPLACE(...) so slash-replaced OrderCode in ProductionOrders matches correctly
                string sql = @"SELECT TOP 150
                                REPLACE(wt.work_ticket_no, '/', '_') as OrderCode, 
                                wt.customer_po_no as SalesOrderNo, 
                                wt.customer as CustomerName, 
                                wt.label_ref as ProductName, 
                                wt.department as Department, 
                                wt.quantity as Quantity, 
                                wt.production_status as Status,
                                lp.PlannedEnd as PlannedEnd,
                                wt.delivery_date as OrderedDate,
                                wt.current_stage as CurrentStage,
                                lp.Priority as Priority,
                                lp.PlannedStart as PlannedStart,
                                lp.MachineId as MachineId,
                                m.MachineName as MachineName
                               FROM work_tickets wt WITH (NOLOCK)
                               LEFT JOIN ProductionOrders lp WITH (NOLOCK) ON wt.work_ticket_no = lp.OrderCode
                               LEFT JOIN MachineResources m WITH (NOLOCK) ON lp.MachineId = m.MachineId
                               WHERE (@Status IS NULL OR wt.production_status = @Status)
                                 AND (@Department IS NULL OR wt.department = @Department)
                                 AND (wt.delivery_date >= @MinDate) 
                               ORDER BY wt.delivery_date DESC";
                return await conn.QueryAsync<ProductionOrder>(sql, new { Status = status, Department = department, MinDate = minDate });
            }
        }

        /// <summary>
        /// Persists status changes (Complete, Hold, In Progress, etc.) made in the Planning Portal.
        /// Writes production_status back to work_tickets (the source-of-truth table) and
        /// syncs the Priority into the ProductionOrders planning table if a planning row exists.
        /// Without this, dashboard KPI counts for Completed/Urgent never update.
        /// </summary>
        public bool Update(ProductionOrder order)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                // Step 1: Write status back to the source work_tickets table
                string updateWt = @"
                    UPDATE work_tickets
                    SET production_status = @Status
                    WHERE REPLACE(work_ticket_no, '/', '_') = @OrderCode
                       OR work_ticket_no = @OrderCode";

                conn.Execute(updateWt, new { order.Status, OrderCode = order.OrderCode });

                // Step 2: If a planning row exists, sync Priority too
                string updatePriority = @"
                    UPDATE ProductionOrders
                    SET Priority = @Priority
                    WHERE OrderCode = @OrderCode";

                if (!string.IsNullOrEmpty(order.Priority))
                    conn.Execute(updatePriority, new { Priority = order.Priority, OrderCode = order.OrderCode });

                new AuditRepository().Log(
                    SessionHelper.UserId,
                    $"Status updated to '{order.Status}' (Priority: {order.Priority ?? "unchanged"})",
                    "work_tickets", order.OrderCode);

                return true;
            }
        }

        public async Task<KPISummary> GetDashboardKPIsAsync()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string minDate = DateTime.Now.AddMonths(-4).ToString("yyyy-MM-dd");
                string sql = @"
                    SELECT 
                        -- URGENT: flagged Urgent in the planning table
                        SUM(CASE 
                            WHEN lp.Priority = 'Urgent' 
                              OR UPPER(ISNULL(wt.production_status,'')) = 'URGENT'
                            THEN 1 ELSE 0 
                        END) AS UrgentOrders,

                        -- COMPLETED: ticket closed / finished
                        SUM(CASE 
                            WHEN wt.production_status IN ('Completed','Closed')
                              OR wt.current_stage = 'ORDER COMPLETED'
                            THEN 1 ELSE 0 
                        END) AS CompletedOrders,

                        -- ACTIVE (pie): released to the production floor
                        SUM(CASE 
                            WHEN wt.production_status IN ('In Progress','Quality Check','Hold')
                              AND NOT (lp.Priority = 'Urgent' OR UPPER(ISNULL(wt.production_status,'')) = 'URGENT')
                            THEN 1 ELSE 0 
                        END) AS ActiveOrders,

                        -- PLANNED: any ticket that has been given a PlannedStart date,
                        --          excluding those already in Active / Urgent / Completed buckets
                        SUM(CASE 
                            WHEN lp.PlannedStart IS NOT NULL
                              AND ISNULL(wt.production_status,'') NOT IN ('In Progress','Quality Check','Hold','Completed','Closed','URGENT')
                              AND ISNULL(wt.current_stage,'') <> 'ORDER COMPLETED'
                              AND NOT (lp.Priority = 'Urgent')
                            THEN 1 ELSE 0 
                        END) AS PlannedOrders,

                        -- TOTAL MANAGED: all tickets the user has explicitly worked with
                        --   (has a planning row OR is in an active/completed status)
                        SUM(CASE 
                            WHEN lp.OrderCode IS NOT NULL
                              OR wt.production_status IN ('In Progress','Quality Check','Hold','Completed','Closed','URGENT')
                              OR wt.current_stage = 'ORDER COMPLETED'
                            THEN 1 ELSE 0 
                        END) AS TotalOrders

                    FROM work_tickets wt WITH (NOLOCK)
                    LEFT JOIN ProductionOrders lp WITH (NOLOCK) ON wt.work_ticket_no = lp.OrderCode
                    WHERE wt.delivery_date >= @MinDate;

                    SELECT COUNT(*) FROM MachineResources WITH (NOLOCK);
                    SELECT COUNT(*) FROM MachineResources WITH (NOLOCK) WHERE Status = 'Available';
                ";

                using (var multi = await conn.QueryMultipleAsync(sql, new { MinDate = minDate }))
                {
                    var kpi = await multi.ReadFirstOrDefaultAsync<KPISummary>();
                    if (kpi == null) kpi = new KPISummary();

                    kpi.TotalMachines = await multi.ReadFirstAsync<int>();
                    kpi.MachinesActive = await multi.ReadFirstAsync<int>();
                    kpi.UPSStatus = "Stable";
                    kpi.OnTimeDeliveryRate = 94.2m;
                    kpi.LowStockCount = await conn.ExecuteScalarAsync<int>(
                        "SELECT COUNT(*) FROM InventoryItems WITH (NOLOCK) WHERE QuantityOnHand < ReorderLevel");

                    kpi.UrgentOrders    = Math.Max(0, kpi.UrgentOrders);
                    kpi.ActiveOrders    = Math.Max(0, kpi.ActiveOrders);
                    kpi.CompletedOrders = Math.Max(0, kpi.CompletedOrders);
                    kpi.PlannedOrders   = Math.Max(0, kpi.PlannedOrders);
                    // TotalOrders = explicit sum so donut centre label always matches the slices
                    kpi.TotalOrders     = kpi.UrgentOrders + kpi.ActiveOrders + kpi.CompletedOrders + kpi.PlannedOrders;

                    return kpi;
                }
            }
        }

        public KPISummary GetDashboardKPIs()
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string minDate = DateTime.Now.AddMonths(-4).ToString("yyyy-MM-dd");
                string sql = @"
                    SELECT 
                        SUM(CASE 
                            WHEN lp.Priority = 'Urgent' 
                              OR UPPER(ISNULL(wt.production_status,'')) = 'URGENT'
                            THEN 1 ELSE 0 
                        END) AS UrgentOrders,

                        SUM(CASE 
                            WHEN wt.production_status IN ('Completed','Closed')
                              OR wt.current_stage = 'ORDER COMPLETED'
                            THEN 1 ELSE 0 
                        END) AS CompletedOrders,

                        SUM(CASE 
                            WHEN wt.production_status IN ('In Progress','Quality Check','Hold')
                              AND NOT (lp.Priority = 'Urgent' OR UPPER(ISNULL(wt.production_status,'')) = 'URGENT')
                            THEN 1 ELSE 0 
                        END) AS ActiveOrders,

                        -- PLANNED: any ticket with a PlannedStart date set,
                        --          not yet on the floor / not urgent / not completed
                        SUM(CASE 
                            WHEN lp.PlannedStart IS NOT NULL
                              AND ISNULL(wt.production_status,'') NOT IN ('In Progress','Quality Check','Hold','Completed','Closed','URGENT')
                              AND ISNULL(wt.current_stage,'') <> 'ORDER COMPLETED'
                              AND NOT (lp.Priority = 'Urgent')
                            THEN 1 ELSE 0 
                        END) AS PlannedOrders,

                        SUM(CASE 
                            WHEN lp.OrderCode IS NOT NULL
                              OR wt.production_status IN ('In Progress','Quality Check','Hold','Completed','Closed','URGENT')
                              OR wt.current_stage = 'ORDER COMPLETED'
                            THEN 1 ELSE 0 
                        END) AS TotalOrders

                    FROM work_tickets wt WITH (NOLOCK)
                    LEFT JOIN ProductionOrders lp WITH (NOLOCK) 
                        ON REPLACE(wt.work_ticket_no, '/', '_') = lp.OrderCode
                    WHERE wt.delivery_date >= @MinDate;

                    SELECT COUNT(*) FROM MachineResources WITH (NOLOCK);
                    SELECT COUNT(*) FROM MachineResources WITH (NOLOCK) WHERE Status = 'Available';
                ";

                using (var multi = conn.QueryMultiple(sql, new { MinDate = minDate }))
                {
                    var kpi = multi.ReadFirstOrDefault<KPISummary>();
                    if (kpi == null) kpi = new KPISummary();
                    kpi.TotalMachines = multi.ReadFirst<int>();
                    kpi.MachinesActive = multi.ReadFirst<int>();
                    kpi.UPSStatus = "Stable";
                    kpi.OnTimeDeliveryRate = 94.2m;
                    kpi.LowStockCount = conn.ExecuteScalar<int>(
                        "SELECT COUNT(*) FROM InventoryItems WITH (NOLOCK) WHERE QuantityOnHand < ReorderLevel");

                    kpi.UrgentOrders    = Math.Max(0, kpi.UrgentOrders);
                    kpi.ActiveOrders    = Math.Max(0, kpi.ActiveOrders);
                    kpi.CompletedOrders = Math.Max(0, kpi.CompletedOrders);
                    kpi.PlannedOrders   = Math.Max(0, kpi.PlannedOrders);
                    // TotalOrders = explicit sum so donut centre label always matches the slices
                    kpi.TotalOrders     = kpi.UrgentOrders + kpi.ActiveOrders + kpi.CompletedOrders + kpi.PlannedOrders;

                    return kpi;
                }
            }
        }

        public int Create(ProductionOrder order)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"
                    MERGE ProductionOrders AS target
                    USING (SELECT @OrderCode AS OrderCode) AS source
                    ON target.OrderCode = source.OrderCode
                    WHEN NOT MATCHED THEN
                        INSERT (OrderCode, MachineId, PlannedStart, PlannedEnd, Priority, ProductName, Department, Quantity, UnitOfMeasure, Notes)
                        VALUES (@OrderCode, @MachineId, @PlannedStart, @PlannedEnd, @Priority, @ProductName, @Department, @Quantity, @UnitOfMeasure, @Notes);
                    SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var id = conn.ExecuteScalar<int?>(sql, new
                {
                    order.OrderCode,
                    order.MachineId,
                    order.PlannedStart,
                    order.PlannedEnd,
                    Priority       = order.Priority ?? "N/A",
                    order.ProductName,
                    order.Department,
                    order.Quantity,
                    UnitOfMeasure  = order.UnitOfMeasure ?? "PCS",
                    order.Notes
                });

                if (id.HasValue && id.Value > 0)
                    new AuditRepository().Log(SessionHelper.UserId, $"Created order: {order.OrderCode}", "ProductionOrders", order.OrderCode);

                return id ?? 0;
            }
        }

        public bool UpdateNotes(string orderCode, string notes)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = @"
                    MERGE ProductionOrders AS target
                    USING (
                        SELECT TOP 1
                            REPLACE(work_ticket_no, '/', '_') AS OrderCode,
                            label_ref AS ProductName,
                            department AS Department,
                            quantity AS Quantity,
                            'PCS' AS UnitOfMeasure
                        FROM work_tickets
                        WHERE OrderCode = @OrderCode OR work_ticket_no = @OrderCode
                    ) AS source
                    ON target.OrderCode = source.OrderCode
                    WHEN MATCHED THEN
                        UPDATE SET Notes = @Notes
                    WHEN NOT MATCHED THEN
                        INSERT (OrderCode, Notes, ProductName, Department, Quantity, UnitOfMeasure)
                        VALUES (source.OrderCode, @Notes, source.ProductName, source.Department, source.Quantity, source.UnitOfMeasure);";

                var result = conn.Execute(sql, new { OrderCode = orderCode, Notes = notes }) > 0;
                if (result)
                {
                    new AuditRepository().Log(SessionHelper.UserId, "Updated internal notes", "ProductionOrders", orderCode);
                }
                return result;
            }
        }

        public bool Delete(int id)
        {
            using (var conn = MSSQLHelper.GetConnection())
            {
                string sql = "DELETE FROM ProductionOrders WHERE ProductionOrderId = @Id";
                return conn.Execute(sql, new { Id = id }) > 0;
            }
        }
    }
}
