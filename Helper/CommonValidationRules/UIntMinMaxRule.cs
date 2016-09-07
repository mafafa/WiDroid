using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace Helper.CommonValidationRules
{
    public class UIntMinMaxRule : ValidationRule
    {
        private uint _min = 0;
        private uint _max = uint.MaxValue;

        public UIntMinMaxRule()
        {

        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            uint inputInt = 0;

            try
            {
                if (((String)value).Length > 0)
                {
                    inputInt = uint.Parse((String)value);
                }
            }
            catch (Exception ex)
            {
                return new ValidationResult(false, "Illegal character input or " + ex.Message);
            }

            if ((inputInt < Min) || (inputInt > Max))
            {
                return new ValidationResult(false, "Please enter an integer in the range: " + Min + " - " + Max + ".");
            }
            else
            {
                return new ValidationResult(true, null);
            }
        }

        public uint Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public uint Max
        {
            get { return _max; }
            set { _max = value; }
        }
    }
}
