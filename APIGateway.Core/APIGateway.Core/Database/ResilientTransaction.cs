﻿using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace APIGateway.Core.Database
{
    public class ResilientTransaction
    {
        private readonly DbContext _context;

        private ResilientTransaction(DbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public static ResilientTransaction New(DbContext context)
        {
            return new ResilientTransaction(context);
        }

        public async Task ExecuteAsync(Func<Task> action)
        {
            // Use of an EF Core resiliency strategy when using multiple DbContexts
            // within an explicit BeginTransaction():
            // https://docs.microsoft.com/ef/core/miscellaneous/connection-resiliency
            var strategy = _context.Database.CreateExecutionStrategy();
            await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync();
                await action();
                await transaction.CommitAsync();
            });
        }
    }
}