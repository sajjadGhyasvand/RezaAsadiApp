﻿

using CustomerDiscountManagement.Infrastructure.EFCore.Repository;
using DiscountManagement.Application;
using DiscountManagement.Application.Contracts.ColleagueDiscount;
using DiscountManagement.Application.Contracts.CustomerDiscount;
using DiscountManagement.Domain.ColleagueDiscountAgg;
using DiscountManagement.Domain.CustomerDiscountAgg;
using DiscountManagement.Infrastructure.EFCore;
using DiscountManagement.Infrastructure.EFCore.Repository;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DiscountManagement.Configuration
{
    public class DiscountManagementBootStrapper
    {
        public static void Configure(IServiceCollection services, string connectionString)
        {
            services.AddTransient<ICustomerDiscountRepository, CustomerDiscountRepository>();
            services.AddTransient<ICustomerDiscountApplication, CustomerDiscountApplication>();

            services.AddTransient<IColleagueDiscountRepository, ColleagueDiscountRepository>();
            services.AddTransient<IColleagueDiscountApplication, ColleagueDiscountApplication>();



            services.AddDbContext<DiscountContext>(x=>x.UseSqlServer(connectionString));
        }
    }
}
