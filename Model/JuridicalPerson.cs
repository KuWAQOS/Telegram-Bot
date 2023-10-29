using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace INNTELEGRAMBOT.Model
{
    internal class JuridicalPerson
    {
        public string ИНН { get; set; }
        public string ОГРН { get; set; }
        public string НаимСокрЮЛ { get; set; }
        public string НаимПолнЮЛ { get; set; }
        public string ДатаОГРН { get; set; }
        public string Статус { get; set; }
        public string АдресПолн { get; set; }
        public string ОснВидДеят { get; set; }
        public string ГдеНайдено { get; set; }
    }
}
