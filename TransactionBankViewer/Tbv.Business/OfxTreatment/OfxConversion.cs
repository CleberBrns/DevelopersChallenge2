using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Tbv.Models;
using Tbv.Models.DataBase;

namespace Tbv.Business.OfxTreatment
{
    public class OfxConversion
    {
        private OfxReader ofxReader;

        public OfxConversion()
        {
            this.ofxReader = new OfxReader();
        }

        public Result<BankAccount> GetBankAccountResult(string ofxFilePath)
        {
            XElement xElement = ofxReader.OfxToXElement(ofxFilePath);
            return OfxToBankAccount(xElement);
        }
        
        /// <summary>
        /// Read and convert ofxXElement to BankAccount object 
        /// </summary>
        /// <param name="ofxXElement"></param>
        /// <returns></returns>
        private Result<BankAccount> OfxToBankAccount(XElement ofxXElement)
        {
            Result<BankAccount> result = new Result<BankAccount>();

            try
            {
                //Check if the tags we want have values
                if (ofxXElement != null && 
                    ofxXElement.Descendants("BANKACCTFROM").Count() > 0 &&
                    ofxXElement.Descendants("STMTTRN").Count() > 0)
                {
                    //Read the bank information
                    BankAccount bankAccount = (from c in ofxXElement.Descendants("BANKACCTFROM")
                                               select new BankAccount
                                               {
                                                   BankId = c.Element("BANKID").Value,
                                                   AccountId = long.Parse(c.Element("ACCTID").Value,
                                                                           NumberFormatInfo.InvariantInfo)
                                               }).FirstOrDefault();

                    //Read the transactions informations
                    IEnumerable<AccountTransaction> transactions =
                        (from c in ofxXElement.Descendants("STMTTRN")
                         select new AccountTransaction
                         {
                             TrType = c.Element("TRNTYPE").Value,
                             Amount = decimal.Parse(c.Element("TRNAMT").Value,
                                             NumberFormatInfo.InvariantInfo),
                             DatePosted = DateTime.ParseExact(c.Element("DTPOSTED").Value,
                                                 "yyyyMMdd", null),
                             TrDescription = c.Element("MEMO").Value,
                             DateImportation = DateTime.Now
                         });

                    if (bankAccount != null)
                    {
                        if (transactions != null && transactions.Count() > 0)
                        {
                            bankAccount.Transactions = transactions.ToList();
                        }                       

                        result = new Result<BankAccount> 
                        {
                            Status = true,
                            Value = bankAccount
                        };
                    }
                }
                else
                {
                    result = new Result<BankAccount>
                    {
                        Status = false,
                        Message = "OFX file without specifics transaction and bank tags"                       
                    };
                }

            }
            catch (Exception ex)
            {
                result = new Result<BankAccount> {
                    Error = true,
                    Message = "Error during the Ofx Conversion to the Account object",
                    ExceptionMessage = ex.Message
                };
            }

            return result;
        }
    }
}
