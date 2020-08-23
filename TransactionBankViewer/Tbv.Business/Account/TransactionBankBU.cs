using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbv.Data.Repository;
using Tbv.Models;
using Tbv.Models.DataBase;

namespace Tbv.Business.Account
{
    public class TransactionBankBU
    {
        private BankAccountRepository bankAccountRepository;

        public TransactionBankBU()
        {
            this.bankAccountRepository = new BankAccountRepository();
        }

        public List<AccountTransaction> GetTransactions(string bankId, long accountId, DateTime initialDate, DateTime finalDate)
        {
            return bankAccountRepository.GetTransactionsByFilter(bankId, accountId, initialDate, finalDate);
        }
    }
}
