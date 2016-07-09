using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Business.CommonModule
{
    public static class ComBaseReport
    {
        public static string template = AppDomain.CurrentDomain.BaseDirectory + System.Configuration.ConfigurationManager.AppSettings["TemplatePath"];       
    }
}
