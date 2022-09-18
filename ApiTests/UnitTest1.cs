using AutoMapper;
using ContactAPI.Controllers;
using MassTransit;
using Moq;
using Shared.Business;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using Shared.Business.Services;
using Shared.Data.Models;
using Shared.Data.UoW;

namespace ApiTests
{
    public class UnitTest1
    {
        /// <summary>
        /// Constructor
        /// </summary>


        public readonly IContactRepository MockContactRepository;
        public readonly IReportRepository MockReportRepository;
        public readonly IContactInformationRepository MockContactInformationRepository;
        public IMapper LoadMapper()
        {
            var mapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Contact, ContactDetailResponseDto>().ReverseMap();
                cfg.CreateMap<Contact, ApiResult<ContactDetailResponseDto>>().ReverseMap();
                cfg.CreateMap<Contact, ContactListResponseDto>().ReverseMap();
                cfg.CreateMap<ContactDetailResponseDto, ContactListResponseDto>().ReverseMap();
                cfg.CreateMap<Contact, ContactAddRequestDto>().ReverseMap();
                cfg.CreateMap<ContactInformation, ContactInformationAddRequestDto>().ReverseMap();
                cfg.CreateMap<ContactInformation, ContactInformationDto>().ReverseMap();
                cfg.CreateMap<Report, ReportAddRequestDto>().ReverseMap();
                cfg.CreateMap<Report, ReportListDto>().ReverseMap();
                cfg.CreateMap<ReportDetailDto, ReportListDto>().ReverseMap();
            });
            return mapper.CreateMapper();
        }
        public UnitTest1()
        {
            var mapper = LoadMapper();

            #region Contact Mock
            var userList = new List<Contact>
            {
                new Contact {
                    Id = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                    FirstName = "Faruk",
                    CompanyName = "Company",
                    LastName = "Durgut"
                }
            };

            var contactList = mapper.Map<List<ContactListResponseDto>>(userList);
            var apiList = new ApiResult<List<ContactListResponseDto>>();

            apiList.ResultObject = contactList;


            var mockUserRepository = new Mock<IContactRepository>();
            mockUserRepository.Setup(mr => mr.GetAll()).ReturnsAsync(apiList);

            var contactDetail = new ApiResult<ContactDetailResponseDto>();
            mockUserRepository.Setup(mr => mr.Get(It.IsAny<Guid>()).Result).Returns((Guid i) =>
                apiList.ResultObject.Where(x => x.Id == i)
                .Select(x => new ApiResult<ContactDetailResponseDto> { ResultObject = mapper.Map<ContactDetailResponseDto>(x) })
                .FirstOrDefault());


            mockUserRepository.Setup(mr => mr.Add(It.IsAny<ContactAddRequestDto>())).Callback(
                (ContactAddRequestDto target) =>
                {
                    apiList.ResultObject.Add(new ContactListResponseDto
                    {
                        FirstName = target.FirstName,
                        LastName = target.LastName,
                        CompanyName = target.CompanyName,
                    });
                });


            mockUserRepository.Setup(mr => mr.Delete(It.IsAny<Guid>())).Callback(
                (Guid target) =>
                {
                    apiList.ResultObject.Remove(apiList.ResultObject.Where(x => x.Id == target).FirstOrDefault());

                });


            this.MockContactRepository = mockUserRepository.Object;
            #endregion

            #region Report Mock
            var reportList = new List<Report>
            {
                new Report {
                    Id = Guid.Parse("d24b2cf7-667b-4516-9aaa-105ebb98cff7"),
                    RequestDate = DateTime.Now,
                    FileUrl = "FileUrl",
                    Status= Shared.Data.Enum.ReportStatus.PROCESSING
                }
            };

            var reportDtoList = mapper.Map<List<ReportListDto>>(reportList);
            var apiReportList = new ApiResult<List<ReportListDto>>();
            var apiReportViewList = new ApiResult<List<ReportViewDto>>();
            apiReportViewList.ResultObject = new List<ReportViewDto>();

            apiReportList.ResultObject = reportDtoList;
            var mockReportRepository = new Mock<IReportRepository>();
            
            mockReportRepository.Setup(mr => mr.GetAll()).ReturnsAsync(apiReportList);

            mockReportRepository.Setup(mr => mr.Get(It.IsAny<Guid>()).Result).Returns((Guid i) =>
               apiReportList.ResultObject.Where(x => x.Id == i)
               .Select(x => new ApiResult<ReportDetailDto> { ResultObject = mapper.Map<ReportDetailDto>(x) })
               .FirstOrDefault());


            mockReportRepository.Setup(mr => mr.Add(It.IsAny<ReportAddRequestDto>())).Callback(
                (ReportAddRequestDto target) =>
                {
                    apiReportList.ResultObject.Add(new ReportListDto
                    {
                        Id = Guid.NewGuid(),
                        RequestDate = DateTime.Now,
                        FileUrl = "FileUrl",
                        Status = Shared.Data.Enum.ReportStatus.PROCESSING
                    });
                });

            mockReportRepository.Setup(mr => mr.CreateReport(It.IsAny<ReportCreateDto>()).Result).Callback(
                (ReportCreateDto target) => {
                    
                    apiReportViewList.ResultObject.Add(new ReportViewDto
                    {

                        ContactCount= 1,
                        RegisteredContactCount =2,
                       Content = "İstanbul",
                    });
                
                }).Returns(apiReportViewList);

            this.MockReportRepository = mockReportRepository.Object;

            #endregion

            #region Contact Information Mock

            var contactInformationList = new List<ContactInformation>
            {
                new ContactInformation {
                    Id = Guid.Parse("2fd0a8ce-e9c7-498c-ba37-c6035667ce3b"),
                    ContactId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                    InformationType = Shared.Data.Enum.InformationEnum.PhoneNumber,
                    Content = "551 2044601"
                }
            };

    
            var mockContactInformationRepository = new Mock<IContactInformationRepository>();
            ApiResult<SuccessResponseDto> addSuccessResponse = new ApiResult<SuccessResponseDto>();
            mockContactInformationRepository.Setup(mr => mr.Add(It.IsAny<ContactInformationAddRequestDto>()).Result).Callback(
                (ContactInformationAddRequestDto target) =>
                {
                    contactInformationList.Add(new ContactInformation
                    {
                        Id = Guid.NewGuid(),
                        ContactId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                        InformationType = Shared.Data.Enum.InformationEnum.Location,
                        Content = "Eyüp"
                    });
                    addSuccessResponse.ResultObject = new SuccessResponseDto
                    {
                        Message = "Contact Information Added"
                    };
                }).Returns(addSuccessResponse);

            ApiResult<SuccessResponseDto> deleteSuccessResponse = new ApiResult<SuccessResponseDto>();

            mockContactInformationRepository.Setup(mr => mr.Delete(It.IsAny<Guid>()).Result).Callback(
               (Guid target) =>
               {
                   contactInformationList.Remove(contactInformationList.Where(x => x.Id == target).FirstOrDefault());
                   deleteSuccessResponse.ResultObject = new SuccessResponseDto
                   {
                       Message = "Contact Information Deleted"
                   };
               }).Returns(deleteSuccessResponse);


            this.MockContactInformationRepository = mockContactInformationRepository.Object;

            #endregion 

        }
        [Fact]
        public void GetAll_Than_Check_Count_Test()
        {
            var expected = this.MockContactRepository.GetAll().Result.ResultObject.Count;


            Assert.True(expected > 0);// Test GetAll returns user objects
        }

        [Fact]
        public void GetById_Than_Check_Correct_Object_Test()
        {
            var actual = new ContactDetailResponseDto { FirstName = "Faruk", LastName = "Durgut", CompanyName = "Company" };

            var expected = this.MockContactRepository.Get(Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e")).Result.ResultObject;

            Assert.NotNull(expected); // Test is not null
            Assert.Equal(actual.FirstName, expected.FirstName); // test correct object found
        }

        [Fact]

        public void GetyId_With_Undefined_Id_Than_Exception_Occurred_Test()
        {
            var expected = this.MockContactRepository.Get(It.IsAny<Guid>()).Result;

            Assert.Null(expected);

        }

        [Fact]
        public void Insert_User_Than_Check_GetAll_Count_Test()
        {
            var actual = this.MockContactRepository.GetAll().Result.ResultObject.Count + 1;

            var user = new ContactAddRequestDto { FirstName = "Ömer", LastName = "Durgut" };

            this.MockContactRepository.Add(user);

            var expected = this.MockContactRepository.GetAll().Result.ResultObject.Count;

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void Delete_User_Than_Check_GetAll_Count_Test()
        {
            var actual = this.MockContactRepository.GetAll().Result.ResultObject.Count - 1;

            this.MockContactRepository.Delete(Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"));

            var expected = this.MockContactRepository.GetAll().Result.ResultObject.Count;

            Assert.Equal(actual, expected);
        }

        [Fact]
        public void GetAllReport_Than_Check_Count_Test()
        {
            var expected = this.MockReportRepository.GetAll().Result.ResultObject.Count;


            Assert.True(expected > 0);// Test GetAll returns user objects
        }

        [Fact]
        public void GetReportById_Than_Check_Correct_Object_Test()
        {
            var actual = new ReportDetailDto { Id = Guid.Parse("d24b2cf7-667b-4516-9aaa-105ebb98cff7") };

            var expected = this.MockReportRepository.Get(Guid.Parse("d24b2cf7-667b-4516-9aaa-105ebb98cff7")).Result.ResultObject;
            Assert.NotNull(expected); // Test is not null
            Assert.Equal(actual.Id, expected.Id); // test correct object found
        }

        [Fact]
        public void CreateReport_Object_Test()
        {
            var request = new ReportCreateDto { ReportId = Guid.Parse("d24b2cf7-667b-4516-9aaa-105ebb98cff7"),Path="Path" };

            var expected = this.MockReportRepository.CreateReport(request).Result.ResultObject;
            Assert.NotNull(expected); // Test is not null
        }

        [Fact]
        public void Insert_ContactInformation_Than_Check_Result_Test()
        {
            
            var contentInformation = new ContactInformationAddRequestDto {
                ContactId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                InformationType = Shared.Data.Enum.InformationEnum.PhoneNumber,
                Content = "551 2044601"
            };

            var expected = this.MockContactInformationRepository.Add(contentInformation).Result.ResultObject;

            
            Assert.NotNull(expected);
        }

        [Fact]
        public void Delete_ContactInformation_Than_Check_Result_Test()
        {
            var expected = this.MockContactInformationRepository.Delete(Guid.Parse("d24b2cf7-667b-4516-9aaa-105ebb98cff7")).Result.ResultObject;


            Assert.NotNull(expected);
        }
    }
}