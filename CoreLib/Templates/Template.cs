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
        public string Name { get; set; }
        public string Id { get; set; }
        protected List<ICommand> Commands = new List<ICommand>();
        public TemplateResult Result { get; protected set; }

        public async Task RunAsync(IDevice device, IEnumerable<ICommand> commands, params object[] paramters)
        {
            Commands = commands.ToList();
            await InitializeAsync(paramters);
            
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

        protected abstract Task InitializeAsync(params object[] parameters);
        protected abstract void FinalizeResult(string commandResult);
    }
}
