﻿using My_Shop_Framework.Application;
using ShopManagement.Application.Contracts.Product;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DiscountManagement.Application.Contracts.ColleagueDiscount
{
    public class CreateColleagueDiscount
    {
        [Range(1,100000000000000,ErrorMessage = ValidationMessages.IsRequired)]
        public long ProductId { get;  set; }
        [Range(1, 99, ErrorMessage = ValidationMessages.IsRequired)]
        public int DiscountRate { get; set; }
        public List<ProductViewModel> Products { get; set; }
    }
}