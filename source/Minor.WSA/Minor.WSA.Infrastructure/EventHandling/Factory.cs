using System;

namespace Minor.WSA.Infrastructure
{
    internal class Factory
    {
        private Type type;

        public Factory(Type type)
        {
            this.type = type;
        }
    }
}