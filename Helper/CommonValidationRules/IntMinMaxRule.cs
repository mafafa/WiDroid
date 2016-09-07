using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;


namespace Helper.CommonValidationRules
{
    public class IntMinMaxRule : ValidationRule
    {
        private int _min = int.MinValue;
        private int _max = int.MaxValue;

        public IntMinMaxRule()
        {

        }

        public override ValidationResult Validate(object value, System.Globalization.CultureInfo cultureInfo)
        {
            int inputInt = 0;

            try
            {
                if (((String)value).Length > 0)
                {
                    inputInt = int.Parse((String)value);
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

        public int Min
        {
            get { return _min; }
            set { _min = value; }
        }

        public int Max
        {
            get { return _max; }
            set { _max = value; }
        }
    }
}
