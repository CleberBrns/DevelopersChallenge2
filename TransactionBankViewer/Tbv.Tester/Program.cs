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
            string[] filePaths = Directory.GetFiles(@"D:\MyProjects\DevelopersChallenge2\OFX\", "*.ofx", SearchOption.AllDirectories);

            AccountBU accountBU = new AccountBU();
            accountBU.GetBankAccounts(filePaths);
        }
    }
}
