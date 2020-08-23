using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using Tbv.Data.Entity;
using Tbv.Models.DataBase;

namespace Tbv.Data.Repository
{
    public class BankAccountRepository
    {
        /// <summary>
        /// Get BankAccount (if it's new insert)
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <returns></returns>
        public BankAccount GetOrInsertBankAccount(BankAccount bankAccount)
        {
            using (var context = new EntityContext())
            {
                BankAccount dbObj = context.BankAccount
                                           .Where(x => x.BankId == bankAccount.BankId && x.AccountId == bankAccount.AccountId)
                                           .FirstOrDefault();

                if (dbObj != null)
                    return dbObj;
                else
                {
                    context.BankAccount.Add(bankAccount);
                    context.SaveChanges();

                    return bankAccount;
                }
            }
        }

        /// <summary>
        /// Get BankAccounts (if it's new insert)
        /// </summary>
        /// <param name="bankAccounts"></param>
        /// <returns></returns>
        public List<BankAccount> GetOrInsertBankAccounts(List<BankAccount> bankAccounts)
        {
            List<BankAccount> listBanks = new List<BankAccount>();
            using (var context = new EntityContext())
            {
                int banksInserted = 0;
                foreach (var item in bankAccounts)
                {
                    BankAccount dbObj = context.BankAccount
                                               .Where(x => x.BankId == item.BankId && x.AccountId == item.AccountId)
                                               .FirstOrDefault();

                    if (dbObj != null)
                    {
                        listBanks.Add(dbObj);
                    }
                    else
                    {
                        context.BankAccount.Add(item);
                        listBanks.Add(item);
                        banksInserted++;
                    }

                    if (banksInserted > 0)                    
                        context.SaveChanges();
                    
                }                
            }

            return listBanks;
        }

        /// <summary>
        /// Insert AccountTransactions
        /// </summary>
        /// <param name="accountTransactions"></param>
        public void InsertAccountTransacions(List<AccountTransaction> accountTransactions)
        {
            using (var context = new EntityContext())
            {
                foreach (var item in accountTransactions)
                {
                    item.BankAccount = null;
                    context.AccountTransaction.AddOrUpdate(item);
                }
                //If you assure that there is not any obj alreary imported
                //context.AccountTransaction.AddRange(accountTransactions);
                context.SaveChanges();
            }
        }

        /// <summary>
        /// Get AccountTransactions using Initial and Final Dates
        /// </summary>
        /// <param name="initialDate"></param>
        /// <param name="finalDate"></param>
        /// <returns></returns>
        public List<AccountTransaction> GetTransactionsByDates(DateTime initialDate, DateTime finalDate)
        {
            if (initialDate > finalDate || finalDate < initialDate)
            {
                throw new Exception("To get AccountTransaction, the Initial date must to be lower than Final date");
            }

            using (var context = new EntityContext())
            {
                //Assuring first and last hour from dates
                initialDate = initialDate.Date;
                finalDate = finalDate.AddDays(1).AddTicks(-1);

                return context.AccountTransaction
                              .Where(x => x.DatePosted >= initialDate && x.DatePosted <= finalDate)
                              .Include(x => x.BankAccount)
                              .ToList();
            }
        }

        /// <summary>
        /// Get AccountTransactions using Bank Account informartion and Initial and Final Dates
        /// </summary>
        /// <param name="bankAccount"></param>
        /// <param name="initialDate"></param>
        /// <param name="finalDate"></param>
        /// <returns></returns>
        public List<AccountTransaction> GetTransactionsByFilter(string bankId, long accountId, DateTime initialDate, DateTime finalDate)
        {
            using (var context = new EntityContext())
            {
                //Assuring first and last hours from dates
                initialDate = initialDate.Date;
                finalDate = finalDate.AddDays(1).AddTicks(-1);

                return context.AccountTransaction
                              .Where(x => 
                                        (string.IsNullOrEmpty(bankId) || x.BankAccount.BankId.Equals(bankId)) &&
                                        (accountId == 0 || x.BankAccount.AccountId.Equals(accountId)) &&
                                        x.DatePosted >= initialDate && x.DatePosted <= finalDate
                                    )
                              .Include(x => x.BankAccount)
                              .ToList();
            }
        }
    }
}
