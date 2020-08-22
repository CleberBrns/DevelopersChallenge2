using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Tbv.Business.OfxTreatment
{
    public class OfxReader
    {
        /// <summary>
        /// Read specific tags from OFX file and convert to XElement
        /// </summary>
        /// <param name="pathToOfxFile">Valid path to a OFX file</param>
        /// <returns></returns>
        public XElement OfxToXElement(string pathToOfxFile)
        {
            //use LINQ TO GET ONLY THE LINES THAT WE WANT
            var tags = from line in File.ReadAllLines(pathToOfxFile)
                       where
                           line.Contains("<BANKACCTFROM>") ||
                           line.Contains("<BANKID>") ||
                           line.Contains("<ACCTID>") ||
                           line.Contains("<STMTTRN>") ||
                           line.Contains("<TRNTYPE>") ||
                           line.Contains("<DTPOSTED>") ||
                           line.Contains("<TRNAMT>") ||
                           line.Contains("<MEMO>")
                       select line;

            XElement el = new XElement("root");
            XElement son = null;

            string[] arrayParentEl = new string[2] { "BANKACCTFROM", "STMTTRN" };
            foreach (var l in tags)
            {
                var tagName = GetTagName(l);

                if (arrayParentEl.Contains(tagName))
                {
                    son = new XElement(tagName);
                    el.Add(son);
                    continue;
                }

                var elSon = new XElement(tagName);
                elSon.Value = GetTagValue(l);
                son.Add(elSon);
            }

            return el;
        }

        #region Private Methods

        /// <summary>
        /// Get the Tag name to create an Xelement
        /// </summary>
        /// <param name="line">One line from the file</param>
        /// <returns></returns>
        private string GetTagName(string line)
        {
            int pos_init = line.IndexOf("<") + 1;
            int pos_end = line.IndexOf(">");
            pos_end = pos_end - pos_init;
            return line.Substring(pos_init, pos_end);
        }

        /// <summary>
        /// Get the value of the tag to put on the Xelement
        /// </summary>
        /// <param name="line">The line</param>
        /// <returns></returns>
        private string GetTagValue(string line)
        {
            int pos_init = line.IndexOf(">") + 1;
            string retValue = line.Substring(pos_init).Trim();
            if (retValue.IndexOf("[") != -1)
            {
                //date--lets get only the 8 date digits
                retValue = retValue.Substring(0, 8);
            }
            return retValue;
        }

        #endregion
    }
}
