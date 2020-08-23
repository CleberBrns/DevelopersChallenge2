using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbv.Models;

namespace Tbv.Data.Repository
{
    public class BankAccountDAO
    {
        public List<BankTransaction> GetTransactionsByDates(DateTime initialDate, DateTime finalDate)
        {
            return new List<BankTransaction>();
        }
    }
}
