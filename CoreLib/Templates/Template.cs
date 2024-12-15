using CoreLib.Commands;
using CoreLib.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    public abstract class Template
    {
        public string ID { get; }
        protected List<ICommand> Commands = new List<ICommand>();
        public TemplateResult Result { get; protected set; }

        protected Template(string id)
        {
            ID = id;
        }

        public async Task RunAsync(IDevice device)
        {
            string commandResult = null;

            foreach (var command in Commands)
            {
                string result = await command.ExecuteAsync(device);
                if (result != null) // 結果を返すコマンドの場合
                {
                    commandResult = result;
                }
            }

            FinalizeResult(commandResult);
        }

        public abstract void Initialize(List<ICommand> commands, Dictionary<string, object> parameters);
        protected abstract void FinalizeResult(string commandResult);
    }
}
