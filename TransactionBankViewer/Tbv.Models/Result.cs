using System.Collections.Generic;

namespace Tbv.Models
{
    public class Result<T>
        where T : new ()
    {
        public Result()
        {
            this.Error = false;
        }

        public string Message { get; set; }
        public string ExceptionMessage { get; set; }
        public bool Status { get; set; }
        public bool Error { get; set; }
        public T Value { get; set; }
        public List<T> ListValues { get; set; }
    }
}
