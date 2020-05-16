using ExtCore.Infrastructure;

namespace APICore.Actions
{
    class Extension : ExtensionBase
    {
        public override string Name
        {
            get
            {
                return "Test name";
            }
        }
    }
}
