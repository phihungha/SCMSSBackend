﻿namespace ScmssApiServer.DTOs
{
    public class SalesOrderUpdateDto : SalesOrderInputDto
    {
        public IList<TransOrderItemInputDto>? Items { get; set; }
        public decimal? PaymentAmount { get; set; }
        public bool? PaymentCompleted { get; set; }
        public string? Problem { get; set; }
        public TransOrderStatusSelection? Status { get; set; }
    }
}