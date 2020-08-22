using System.Collections.Generic;

namespace Tbv.Models
{
    public class BankAccount
    {
        public BankAccount()
        {
            this.Transactions = new List<BankTransaction>();
        }

        public long Id { get; set; }
        public long AccountId { get; set; }
        public string BankId { get; set; }
        public virtual IEnumerable<BankTransaction> Transactions { get; set; }
    }
}
