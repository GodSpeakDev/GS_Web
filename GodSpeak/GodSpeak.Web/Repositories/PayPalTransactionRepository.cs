using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using GodSpeak.Web.Models;

namespace GodSpeak.Web.Repositories
{
    public interface IPayPalTransactionRepository
    {
        Task<bool> TranscationExists(string paypalTransactionId);

        Task SaveTransactions(PayPalTransaction transaction);
    }

    public class PayPalTransactionRepository:IPayPalTransactionRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public PayPalTransactionRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> TranscationExists(string paypalPaymentId)
        {
            return await _dbContext.PayPalTransactions.AnyAsync(t => t.PayPalPaymentId == paypalPaymentId);
        }

        public async Task SaveTransactions(PayPalTransaction transaction)
        {
            _dbContext.PayPalTransactions.Add(transaction);
            await _dbContext.SaveChangesAsync();
        }
    }
}