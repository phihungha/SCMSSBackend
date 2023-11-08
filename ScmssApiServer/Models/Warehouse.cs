﻿namespace ScmssApiServer.Models
{
    public class Warehouse
    {
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public required string Location { get; set; }
    }
}
