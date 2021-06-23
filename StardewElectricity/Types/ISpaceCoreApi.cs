using System;

namespace StardewElectricity.Types
{
    public interface ISpaceCoreApi
    {
        void RegisterSerializerType(Type type);
    }
}