using System;

namespace Minor.WSA.Infrastructure
{
    public class Factory
    {
        private Type type;

        public Factory(Type type)
        {
            this.type = type;
        }

        public virtual object GetInstance()
        {
            throw new NotImplementedException();
        }
    }
}