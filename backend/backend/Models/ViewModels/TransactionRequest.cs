using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models.ViewModels
{
    public class TransactionRequest
    {
        public int UserId { get; set; } 
        public string CryptoCode { get; set; }
        public string Action { get; set; }
        public decimal CryptoAmount { get; set; }
        public DateTime Datetime { get; set; }

        
    }

}
