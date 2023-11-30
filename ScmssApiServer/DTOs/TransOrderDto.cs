﻿using ScmssApiServer.Models;

namespace ScmssApiServer.DTOs
{
    public abstract class TransOrderDto
    {
        public DateTime CreateTime { get; set; }
        public UserDto CreateUser { get; set; } = null!;
        public required string CreateUserId { get; set; }
        public DateTime? DeliverTime { get; set; }
        public ICollection<OrderEvent> Events { get; set; } = new List<OrderEvent>();
        public DateTime? FinishTime { get; set; }
        public UserDto? FinishUser { get; set; }
        public string? FinishUserId { get; set; }
        public int Id { get; set; }
        public string? InvoiceUrl { get; set; }
        public TransOrderPaymentStatus PaymentStatus { get; set; }
        public string? ReceiptUrl { get; set; }
        public OrderStatus Status { get; set; }
        public decimal SubTotal { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime? UpdateTime { get; set; }
        public decimal VatAmount { get; set; }
        public double VatRate { get; set; }
    }
}
