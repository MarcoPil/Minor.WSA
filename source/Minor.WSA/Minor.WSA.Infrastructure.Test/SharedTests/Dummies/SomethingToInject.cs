using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Minor.WSA.Infrastructure.Test.SharedTests.Dummies
{
    public interface ISomethingToInject
    {
        string InjectionValue();
    }

    public class SomethingToInject : ISomethingToInject
    {
        public string InjectionValue()
        {
            return "injection succeeded";
        }
    }
}
