using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Tbv.Business.OfxTreatment;
using Tbv.Models;

namespace Tbv.Business.Account
{
    public class AccountBU
    {
        private OfxConversion ofxConversion;
        public AccountBU()
        {
            this.ofxConversion = new OfxConversion();
        }

        public void GetBankAccounts(string[] pathOfxFiles)
        {
            Result<BankAccount> resultBankAccounts = ConfigureBankAccounts(pathOfxFiles);

            if (resultBankAccounts.Status)
            {
                List<BankAccount> listBankAccounts = resultBankAccounts.ListValues;
                if (listBankAccounts.Count > 0)
                {                    
                    foreach (var item in listBankAccounts)
                    {
                        //Add every account info to transaction item to help removing duplications
                        item.Transactions.ToList().ForEach(x => {
                            x.AccountId = item.AccountId;
                            x.BankId = item.BankId;
                            x.BankAccount = item;
                        });

                        //To do; remove on insert
                        //Avoiding propagation when insert
                        //item.Transactions = null;
                    }

                    //Using transactions only for working better with duplications
                    List<BankTransaction> bankTransactions = listBankAccounts.SelectMany(x => x.Transactions).ToList();

                    if (bankTransactions.Count > 0)                    
                        bankTransactions = RemoveDuplications(bankTransactions);                    

                }
            }
        }

        /// <summary>
        /// Remove file duplications and search on bank to find transactions from same dates from imported transactions
        /// </summary>
        /// <param name="bankTransactions"></param>
        /// <returns></returns>
        private List<BankTransaction> RemoveDuplications(List<BankTransaction> bankTransactions)
        {
            //Ordering for get the first and final date
            bankTransactions = bankTransactions.OrderBy(x => x.DatePosted).ToList();

            List<BankTransaction> fakeBdlist = bankTransactions.Take(10).ToList();

            //Get first and final date from searching on dababase
            DateTime initialDate = bankTransactions.OrderBy(x => x.DatePosted).Select(x => x.DatePosted).FirstOrDefault();
            DateTime finalDate = bankTransactions.OrderByDescending(x => x.DatePosted).Select(x => x.DatePosted).FirstOrDefault();

            //Remove transaction duplications
            bankTransactions =
                bankTransactions.GroupBy(x => new {
                    x.BankId,
                    x.AccountId,
                    x.BankAccountId,
                    x.Type,
                    x.DatePosted,
                    x.Amount,
                    x.Description,
                    x.BankAccount
                })
                .Select(c => c.First())
                .ToList();


            List<BankTransaction> noDuplicationList = new List<BankTransaction>();

            //Merge database transactions into imported transactions
            if (fakeBdlist.Count > 0)
            {
                //Make a new list with new transactions and possible stored transactions from dates from OFX files 
                foreach (var item in bankTransactions)
                {
                    BankTransaction dBBankTransaction =
                        fakeBdlist.Where(x =>
                                        x.BankId == item.BankId && x.AccountId == item.AccountId && x.BankAccountId == item.BankAccountId &&
                                        x.Type == item.Type && x.DatePosted == item.DatePosted && x.Amount == item.Amount &&
                                        x.Description == item.Description
                                    )
                                  .FirstOrDefault();

                    if (dBBankTransaction != null)
                    {
                        dBBankTransaction.IsAlreadyImported = true;
                        noDuplicationList.Add(dBBankTransaction);
                        continue;
                    }

                    noDuplicationList.Add(item);
                }

                return noDuplicationList;
            }

            return bankTransactions;
        }

        /// <summary>
        /// Read array of OFX files and convert to a list of BankAccount objects
        /// </summary>
        /// <param name="pathOfxFiles"></param>
        /// <returns></returns>
        private Result<BankAccount> ConfigureBankAccounts(string[] pathOfxFiles)
        {
            List<BankAccount> listBankAccounts = new List<BankAccount>();
            Result<BankAccount> result = new Result<BankAccount>();

            string msgErros = string.Empty;
            string msgAlerts = string.Empty;

            if (pathOfxFiles != null && pathOfxFiles.Length > 0)
            {
                foreach (var item in pathOfxFiles)
                {
                    if (!string.IsNullOrEmpty(item))
                    {
                        Result<BankAccount> getResult = ofxConversion.GetBankAccountResult(item);

                        if (getResult.Status)
                        {
                            listBankAccounts.Add(getResult.Value);
                        }
                        else if (getResult.Error || !getResult.Status)
                        {
                            msgAlerts += string.Format(", {0}", result.Message);
                            msgErros += string.Format(", {0}", result.ExceptionMessage);
                        }
                    }
                }
            }

            if (!string.IsNullOrEmpty(msgAlerts))
                result.Message = string.Format("Alerts; {0}", msgAlerts);

            if (!string.IsNullOrEmpty(msgErros))
                result.ExceptionMessage = string.Format("Errors; {0}", msgErros);

            result.ListValues = listBankAccounts;
            result.Status = listBankAccounts.Count > 0;

            return result;
        }
    }
}
