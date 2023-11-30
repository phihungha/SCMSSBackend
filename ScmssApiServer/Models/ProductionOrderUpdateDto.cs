﻿using ScmssApiServer.DTOs;

namespace ScmssApiServer.Models
{
    public class ProductionOrderUpdateDto : OrderUpdateDto
    {
        public ApprovalStatus ApprovalStatus { get; set; }
        public string? ApproveProductionManagerId { get; set; }
    }
}
