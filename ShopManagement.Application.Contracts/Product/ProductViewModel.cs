﻿using System;

namespace ShopManagement.Application.Contracts.Product
{
    public class ProductViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Picture { get; set; }
        public string Code { get; set; }
        public string Category { get; set; }
        public long CategoryId { get; set; }
        public DateTime CreationDate { get; set; }
        public long LanguageId { get; set; }
        public long ShopType { get; set; }
        public string ManualFile { get; set; }
    }
}