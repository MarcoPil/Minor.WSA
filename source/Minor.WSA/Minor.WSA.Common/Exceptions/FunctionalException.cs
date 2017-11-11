using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Minor.WSA.Common
{
    public class FunctionalException : Exception
    {
        private List<Error> _errorList;
        public IEnumerable<Error> ErrorList => _errorList;

        public FunctionalException(params Error[] errors)
        {
            _errorList = new List<Error>(errors);
        }

        public void Add(Error error)
        {
            _errorList.Add(error);
        }
    }
}