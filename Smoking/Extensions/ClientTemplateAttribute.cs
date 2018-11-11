using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Smoking.Extensions
{
    public class ClientTemplate
    {
        public int ID { get; set; }
        public string Path { get; set; }
        public string Name { get; set; }
        public string Action { get; set; }
        public string Controller { get; set; }
        public string FileContent { get; set; }
        public bool IsModul { get; set; }
        
    }

    [AttributeUsage(AttributeTargets.Method, Inherited = false)]
    public class ClientTemplateAttribute:Attribute
    {
        public string Name { get; set; }
        public bool IsModule { get; set; }
        public ClientTemplateAttribute(string name)
        
        {
            this.Name = name;
            this.IsModule = true;
        }
        public ClientTemplateAttribute(string name, bool isModul):this(name)
        {
            this.IsModule = isModul;
        }
    }
}