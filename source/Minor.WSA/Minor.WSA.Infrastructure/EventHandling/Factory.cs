using System;

namespace Minor.WSA.Infrastructure
{
    public class Factory : IFactory
    {
        private Type type;

        public Factory(Type type)
        {
            this.type = type;
        }

        public object GetInstance()
        {
            throw new NotImplementedException();
        }
    }
}