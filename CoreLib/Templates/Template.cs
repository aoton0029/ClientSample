using CoreLib.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    public abstract class Template<T> : BaseTemplate
    {
        protected List<ICommand> Commands = new List<ICommand>();
        public Result<T> Result { get; protected set; } // ジェネリック型のResult

        public override void Run()
        {
            Initialize();
            ExecuteCommands();
            FinalizeResult();
        }

        protected abstract void Initialize();

        private void ExecuteCommands()
        {
            foreach (var command in Commands)
            {
                command.Execute();
            }
        }

        protected abstract void FinalizeResult();

        public override string GetNextTemplateId()
        {
            return Result?.NextTemplateId;
        }
    }
}
