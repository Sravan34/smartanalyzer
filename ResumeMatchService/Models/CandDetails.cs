using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ResumeMatchServices.Models
{
    public class CandDetails
    {
        public int CandId { get; set; }

        public string candName { get; set; }

        public string primarySkill { get; set; }

        public string fileName { get; set; }

        public int weightage { get; set; }

        public string weightageDetails { get; set; }
    }
}
