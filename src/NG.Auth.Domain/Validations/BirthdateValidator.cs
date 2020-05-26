using System;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain.Validations
{
    public class BirthdateValidator : ValidationAttribute
    {
        public int MinAge { get; set; }
        public int MaxAge { get; set; }

        public override bool IsValid(object value)
        {
            if (DateTime.TryParse(value.ToString(), out DateTime BirthDate))
            {
                if (IsOlderThan(BirthDate) && IsYoungerThan(BirthDate))
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsOlderThan(DateTime BirthDate)
        {
            var IsOlder = BirthDate.AddYears(MinAge) < DateTime.Now;
            return IsOlder;
        }
        private bool IsYoungerThan(DateTime BirthDate)
        {
            var IsYounger = BirthDate.AddYears(MaxAge) > DateTime.Now;
            return IsYounger;
        }
    }
}
