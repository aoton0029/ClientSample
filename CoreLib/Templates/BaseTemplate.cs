using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreLib.Templates
{
    public abstract class BaseTemplate
    {
        public string Id { get; protected set; }
        public string Name { get; protected set; }
        public abstract string GetNextTemplateId(); // 次のテンプレートIDを取得
        public abstract void Run();
    }
}
