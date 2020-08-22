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
                        //Avoiding propagation when insert
                        item.Transactions = null;

                        //Add every account info to transaction item to help removing duplications
                        item.Transactions.ToList().ForEach(x => {
                            x.AccountId = item.AccountId;
                            x.BankId = item.BankId;
                            x.BankAccount = item;
                        });
                    }


                    List<BankTransaction> bankTransactions = listBankAccounts.SelectMany(x => x.Transactions).ToList();

                    if (bankTransactions.Count > 0)
                    {
                        bankTransactions = bankTransactions.OrderBy(x => x.DatePosted).ToList();

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
                    }

                }

            }

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
