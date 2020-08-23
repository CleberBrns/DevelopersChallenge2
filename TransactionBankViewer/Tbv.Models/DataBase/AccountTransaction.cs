using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tbv.Models.DataBase
{
    public class AccountTransaction
    {
        public long Id { get; set; }

        [ForeignKey("BankAccount")]
        public long IdBankAccount { get; set; }
        [Column(TypeName = "varchar")]
        public string TrType { get; set; }
        public DateTime DatePosted { get; set; }
        public decimal Amount { get; set; }
        [Column(TypeName = "varchar")]
        public string TrDescription { get; set; }
        public DateTime DateImportation { get; set; }

        public virtual BankAccount BankAccount { get; set; }

        //NotMapped only for exibition and helping remove duplications
        [NotMapped]
        public long AccountId { get; set; }

        [NotMapped]
        public string BankId { get; set; }

        [NotMapped]
        public bool IsAlreadyImported { get; set; }
    }
}
