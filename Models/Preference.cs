using Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Models
{
    public class Preference
    {
        public int Id { get; set; }
        public Enums.Preferences Name { get; set; }
        public string Value { get; set; } = null!;
    }
}
