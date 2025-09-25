using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Infrastructure.options
{
    public class CustomLocalizationOptions
    {
        public string ResourcesPath { get; set; } = "Common/Application/Resources";
        public string ResourcesBaseName { get; set; } = "messages";
    }
}
