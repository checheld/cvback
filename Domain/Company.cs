﻿
// тут прописать LIstы других компонентов
using Domain.Abstract;

namespace Domain
{
    public class Company : BaseDomain
    {
        public string Name { get; set; }
        public virtual List<WorkExperience>? WorkExperienceCompanyList { get; set; }
    }
}
