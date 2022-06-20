﻿using Data.Repositories.Abstract;
using Microsoft.Extensions.DependencyInjection;
using RazorEngine.Templating;
using RazorEngine;
using Services.Abstract;
using Domain;
using AutoMapper;
using Services.Utility;
using Entities;
using Microsoft.Playwright;

namespace Services
{
    public class PdfService : IPdfService
    {
        private readonly ICVsRepository _CVsRepository;
        private readonly IUsersRepository _usersRepository;
        private readonly IProjectsRepository _projectsRepository;
        private readonly IMapper _mapper;
        public PdfService(IMapper mapper, IServiceProvider _serviceProvider)
        {
            _mapper = mapper;
            _CVsRepository = _serviceProvider.GetService<ICVsRepository>();
            _usersRepository = _serviceProvider.GetService<IUsersRepository>();
            _projectsRepository = _serviceProvider.GetService<IProjectsRepository>();
        }

        public class AppMappingProject : Profile
        {
            public AppMappingProject()
            {
                CreateMap<ProjectCVDTO, ProjectCVEntity>().ForMember(x => x.Project, y => y.MapFrom(t => t.Project)).ReverseMap();
                CreateMap<ProjectDTO, ProjectEntity>().ForMember(x => x.TechnologyList, y => y.MapFrom(t => t.TechnologyList)).ReverseMap();
                CreateMap<UserDTO, UserEntity>().ForMember(x => x.TechnologyList, y => y.MapFrom(t => t.TechnologyList)).
                    ForMember(x => x.EducationList, y => y.MapFrom(t => t.EducationList)).
                    ForMember(x => x.WorkExperienceList, y => y.MapFrom(t => t.WorkExperienceList)).ReverseMap();
                CreateMap<TechnologyDTO, TechnologyEntity>().ForMember(x => x.UserList, y => y.MapFrom(t => t.UserList)).ReverseMap();
                CreateMap<EducationDTO, EducationEntity>().ForMember(x => x.University, y => y.MapFrom(t => t.University)).ReverseMap();
                CreateMap<WorkExperienceDTO, WorkExperienceEntity>().ForMember(x => x.Company, y => y.MapFrom(t => t.Company)).ReverseMap();
                CreateMap<UniversityDTO, UniversityEntity>().ForMember(x => x.EducationUniversityList, y => y.MapFrom(t => t.EducationUniversityList)).ReverseMap();
                CreateMap<CompanyDTO, CompanyEntity>().ForMember(x => x.WorkExperienceCompanyList, y => y.MapFrom(t => t.WorkExperienceCompanyList)).ReverseMap();
            }
        }

        public async Task<byte[]> GetPdf(int id)
        {
            var getCV = await this._CVsRepository.GetCVById(id);
            var getUser = await this._usersRepository.GetUserById(getCV.UserId);

            var projectCVList = new List<ProjectCVDTO>();
            var projectCVs = getCV.ProjectCVList;
            foreach (var projectCV in projectCVs)
            {
                projectCVList.Add(_mapper.Map<ProjectCVDTO>(projectCV));
            }

            var UserEducationList = new List<EducationDTO>();
            var Educations = getUser.EducationList;
            foreach (var Education in Educations)
            {
                UserEducationList.Add(_mapper.Map<EducationDTO>(Education));
            }

            var UserWorkExperienceList = new List<WorkExperienceDTO>();
            var WorkExperiences = getUser.WorkExperienceList;
            foreach (var WorkExperience in WorkExperiences)
            {
                UserWorkExperienceList.Add(_mapper.Map<WorkExperienceDTO>(WorkExperience));
            }

            var UserTechnologyList = new List<TechnologyDTO>();
            var Technologies = getUser.TechnologyList;
            foreach (var Technology in Technologies)
            {
                UserTechnologyList.Add(_mapper.Map<TechnologyDTO>(Technology));
            }

            object CvforPdf = new PdfDTO
            {
                    FirstName = getUser.FirstName,
                    LastName = getUser.LastName,
                    Description = getUser.Description,
                    photoUrl = getUser.photoUrl,
                    EducationList = UserEducationList,
                    WorkExperienceList = UserWorkExperienceList,
                    TechnologyList = UserTechnologyList,
                    ProjectCVList = projectCVList
                };
           
            string template = RazorConfiguration.myPdf;
            string result = Engine.Razor.RunCompile(template, "tempalateKey", null, CvforPdf);
            /*File.WriteAllText("input.html", result);*/
            /* PdfDocument pdfDocument = PdfGenerator.GeneratePdf(result, PageSize.A4);
             *//*pdfDocument.Save("D:/Work/cv/cvback/LeviossaCV/HTML to PDF Document.pdf");*//*
             MemoryStream stream = new MemoryStream();
             pdfDocument.Save(stream, false);
             byte[] bytes = stream.ToArray();*/
            Microsoft.Playwright.Program.Main(new[] { "install" });
            var dataUrl = "data:text/html;base64," + Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(result));
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
            });
            /*var browser = await playwright.Webkit.LaunchAsync();*/
            await using var context = await browser.NewContextAsync();
            var page = await context.NewPageAsync();
            await page.GotoAsync(dataUrl, new PageGotoOptions { WaitUntil = WaitUntilState.NetworkIdle });

            var output = await page.PdfAsync(new PagePdfOptions
            {
                Format = "A4",
                /*Format = PaperFormat.*/
                Landscape = false,
                
            });
            await File.WriteAllBytesAsync("output.pdf", output);

            return output;
        }
    }
}