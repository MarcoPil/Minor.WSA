using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Minor.WSA.Infrastructure
{
    public class FunctionalException : Exception
    {
        private List<Error> _errorList;
        public IEnumerable<Error> ErrorList => _errorList;

        public FunctionalException()
        {
            _errorList = new List<Error>();
        }

        public void Add(Error error)
        {
            _errorList.Add(error);
        }
    }
}