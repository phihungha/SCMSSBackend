﻿namespace ScmssApiServer.Models
{
    public class WarehouseProductItemEvent : WarehouseItemEvent
    {
        public ProductionOrder? ProductionOrder { get; set; }
        public int? ProductionOrderId { get; set; }
        public SalesOrder? SalesOrder { get; set; }
        public int? SalesOrderId { get; set; }
        public WarehouseProductItem WarehouseProductItem { get; set; } = null!;
        public int WarehouseProductItemId { get; set; }
    }
}