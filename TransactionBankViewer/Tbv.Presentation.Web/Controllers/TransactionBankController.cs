using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using Tbv.Business.Account;
using Tbv.Models;
using Tbv.Models.DataBase;

namespace Tbv.Presentation.Web.Controllers
{
    public class TransactionBankController : Controller
    {
        private TransactionBankBU transactionBankBU;

        public TransactionBankController()
        {
            this.transactionBankBU = new TransactionBankBU();
        }

        // GET: TransactionBank
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GetTransactions(string bankId, long? accountId, DateTime? initialDate, DateTime? finalDate)
        {
            Result<AccountTransaction> searchResult = new Result<AccountTransaction>();

            try
            {
                string msgDates = string.Empty;
                if (!initialDate.HasValue)
                    msgDates = "The Initial Date is mandatory, ";

                if (!initialDate.HasValue)
                    msgDates += "The Initial Date is mandatory";

                if (string.IsNullOrEmpty(msgDates))
                {
                    searchResult = new Result<AccountTransaction>
                    {
                        Status = false,
                        Message = msgDates                        
                    };
                }
                else
                {
                    accountId = !accountId.HasValue ? 0 : accountId;
                    searchResult.ListValues = transactionBankBU.GetTransactions(bankId, accountId.Value, initialDate.Value, finalDate.Value);
                }
            }
            catch (Exception ex)
            {
                searchResult = new Result<AccountTransaction> {
                    Error = true,
                    Message = "Exception trying to retrieving data",
                    ExceptionMessage = ex.Message
                };
            }

            return Json(new { searchResult }, JsonRequestBehavior.AllowGet);
        }
    }
}