using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CryptoId { get; set; }
        public string Action { get; set; } = string.Empty;
        public decimal CryptoAmount { get; set; }
        public decimal Money { get; set; }
        public DateTime Datetime { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey("CryptoId")]
        public Cryptocurrency? Cryptocurrency { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
