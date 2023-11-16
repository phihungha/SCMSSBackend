﻿namespace ScmssApiServer.Models
{
    public enum OrderEventType
    {
        Processing,
        DeliveryStarted,
        Left,
        Arrived,
        Delivered,
        Completed,
        PaymentDue,
        PaymentCompleted,
        Canceled,
        Returned,
        Interrupted,
    }
}
