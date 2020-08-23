using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Tbv.Business.OfxTreatment;
using Tbv.Data.Repository;
using Tbv.Models;
using Tbv.Models.DataBase;

namespace Tbv.Business.Account
{
    public class AccountBU
    {
        private OfxConversion ofxConversion;
        private BankAccountRepository bankAccountRepository;
        public AccountBU()
        {
            this.ofxConversion = new OfxConversion();
            this.bankAccountRepository = new BankAccountRepository();
        }

        public Result<AccountTransaction> GetBankAccounts(string[] pathOfxFiles)
        {
            Result<AccountTransaction> result = new Result<AccountTransaction>();

            try
            {
                Result<BankAccount> resultBankAccounts = ConfigureBankAccounts(pathOfxFiles);

                if (resultBankAccounts.Status)
                {
                    List<BankAccount> listBankAccounts = resultBankAccounts.ListValues;
                    if (listBankAccounts.Count > 0)
                    {
                        listBankAccounts = CheckBankAccountsOnDataBase(listBankAccounts);

                        foreach (var item in listBankAccounts)
                        {
                            //Add every account info to transaction item to help removing duplications
                            item.Transactions.ToList().ForEach(x => {
                                x.IdBankAccount = item.Id;
                                x.AccountId = item.AccountId;
                                x.BankId = item.BankId;
                            });
                        }

                        //Using transactions only for working better with duplications
                        List<AccountTransaction> bankTransactions = listBankAccounts.SelectMany(x => x.Transactions).ToList();

                        if (bankTransactions.Count > 0)
                        {
                            bankTransactions = RemoveDuplications(bankTransactions);

                            List<AccountTransaction> listInsert = bankTransactions.Where(x => x.Id == 0).ToList();
                            if (listInsert.Count > 0)
                                bankAccountRepository.InsertAccountTransacions(listInsert);

                            result = new Result<AccountTransaction> {
                                Status = true,
                                ListValues = bankTransactions
                            };
                        }
                    }
                }
                else
                {
                    result = new Result<AccountTransaction> {
                        Status = resultBankAccounts.Status,
                        Error = resultBankAccounts.Error,
                        Message = resultBankAccounts.Message,
                        ExceptionMessage = resultBankAccounts.ExceptionMessage
                    };
                }
            }
            catch (Exception ex)
            {
                result = new Result<AccountTransaction> {
                    Error = true,
                    Message = "Error during the reading OFX files",
                    ExceptionMessage = ex.Message
                };
            }           

            return result;
        }

        /// <summary>
        /// Check BankAccounts on database
        /// </summary>
        /// <param name="listBankAccounts"></param>
        /// <returns></returns>
        private List<BankAccount> CheckBankAccountsOnDataBase(List<BankAccount> listBankAccounts)
        {
            //New list to avoid Transactions inserts
            List<BankAccount> listToCheck = new List<BankAccount>();
            listBankAccounts.ForEach(x => { 
                listToCheck.Add(new BankAccount { AccountId = x.AccountId, BankId = x.BankId }); 
            });

            //Remove bank duplications
            listToCheck =
                listToCheck.GroupBy(x => new {                    
                    x.BankId,
                    x.AccountId
                })
                .Select(c => c.First())
                .ToList();

            listToCheck = bankAccountRepository.GetOrInsertBankAccounts(listToCheck);

            //Fill BankAccount information from DataBase
            foreach (var item in listToCheck)
            {
                IEnumerable<AccountTransaction> accountTransactions =
                    listBankAccounts.Where(x => x.BankId == item.BankId && x.AccountId == item.AccountId)
                                    .SelectMany(x => x.Transactions);

                item.Transactions = accountTransactions;
            }

            return listToCheck;
        }

        /// <summary>
        /// Remove transaction duplications and search on bank to find transactions from same dates from imported transactions
        /// </summary>
        /// <param name="bankTransactions"></param>
        /// <returns></returns>
        private List<AccountTransaction> RemoveDuplications(List<AccountTransaction> bankTransactions)
        {
            //Ordering for get the first and final date
            bankTransactions = bankTransactions.OrderBy(x => x.DatePosted).ToList();           

            //Get first and final date from searching on dababase
            DateTime initialDate = bankTransactions.OrderBy(x => x.DatePosted).Select(x => x.DatePosted).FirstOrDefault();
            DateTime finalDate = bankTransactions.OrderByDescending(x => x.DatePosted).Select(x => x.DatePosted).FirstOrDefault();

            List<AccountTransaction> dataBaseList = bankAccountRepository.GetTransactionsByDates(initialDate, finalDate);

            //Remove transaction duplications
            bankTransactions =
                bankTransactions.GroupBy(x => new {
                    x.BankId,
                    x.AccountId,
                    x.IdBankAccount,
                    x.TrType,
                    x.DatePosted,
                    x.Amount,
                    x.TrDescription                    
                })
                .Select(c => c.First())
                .ToList();


            List<AccountTransaction> noDuplicationList = new List<AccountTransaction>();

            //Merge database transactions into imported transactions
            if (dataBaseList.Count > 0)
            {
                //Make a new list with new transactions and possible stored transactions from dates from OFX files 
                foreach (var item in bankTransactions)
                {
                    AccountTransaction dBBankTransaction =
                        dataBaseList.Where(x =>
                                        x.BankAccount.BankId == item.BankId && x.BankAccount.AccountId == item.AccountId && 
                                        x.IdBankAccount == item.IdBankAccount &&
                                        x.TrType == item.TrType && x.DatePosted == item.DatePosted && x.Amount == item.Amount &&
                                        x.TrDescription == item.TrDescription
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
