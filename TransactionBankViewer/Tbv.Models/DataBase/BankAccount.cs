using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Tbv.Models.DataBase
{
    public class BankAccount
    {
        public BankAccount()
        {
            this.Transactions = new List<AccountTransaction>();
        }

        public long Id { get; set; }
        public long AccountId { get; set; }
        [Column(TypeName = "varchar")]
        public string BankId { get; set; }
        public virtual IEnumerable<AccountTransaction> Transactions { get; set; }
    }
}
