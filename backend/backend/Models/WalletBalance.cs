using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend.Models
{
    public class WalletBalance
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int CryptoId { get; set; }
        public decimal Balance { get; set; } = 0;
        public DateTime LastUpdated { get; set; } = DateTime.Now;

        [ForeignKey("CryptoId")]
        public Cryptocurrency? Cryptocurrency { get; set; }

        [ForeignKey("UserId")]
        public User? User { get; set; }
    }
}
