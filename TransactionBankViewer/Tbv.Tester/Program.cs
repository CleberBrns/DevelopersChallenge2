using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tbv.Business.Account;

namespace Tbv.Tester
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] filePaths = Directory.GetFiles(@"C:\Users\WorkStudy\source\repos\ImportOFX\OFX_Files\", "*.ofx", SearchOption.AllDirectories);
            new AccountBU().GetBankAccounts(filePaths);
        }
    }
}
