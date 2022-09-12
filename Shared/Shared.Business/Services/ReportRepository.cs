using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using ExcelDataReader;
using Newtonsoft.Json;
using Shared.Business.Abstract;
using Shared.Business.Dto;
using Shared.Data.Enum;
using Shared.Data.Models;
using Shared.Data.UoW;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Shared.Business.Services
{
    public class ReportRepository : IReportRepository
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        public ReportRepository(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<ApiResult<SuccessResponseDto>> Add(ReportAddRequestDto requestDto)
        {
            var result = new ApiResult<SuccessResponseDto>();

            if (requestDto == null)
            {
                result.IsSucceed = false;
                return result;
            }

            var entity = _mapper.Map<Report>(requestDto);
            entity.Id = Guid.NewGuid();
            entity.Status = Data.Enum.ReportStatus.PROCESSING;
            await _uow.ReportRepository.AddAsync(entity);

            var affectedRow = await _uow.Commit();

            if (affectedRow > 0)
            {
                result.ResultObject = new SuccessResponseDto
                {
                    Id = entity.Id,
                    Message = "Report is processing now"
                };
                result.IsSucceed = true;
            }

            return result;
        }

        public async Task<ApiResult<List<ReportViewDto>>> CreateReport(ReportCreateDto requestDto)
        {
            var result = new ApiResult<List<ReportViewDto>>();

            string fullPathWithName = "";

            var getReport = await _uow.ReportRepository.FindByAsync(x => x.Id == requestDto.ReportId && x.Status == ReportStatus.PROCESSING);

            fullPathWithName = $"{requestDto.Path}/{getReport.Id}.xls";

            if (getReport == null)
            {
                result.IsSucceed = false;
                return result;
            }

            var informations = await _uow.ContactInformationRepository.GetAllAsync();
            var locations = informations.Where(x => x.InformationType == InformationEnum.Location).DistinctBy(x => x.Content).ToList();

            List<ReportViewDto> reportViews = new List<ReportViewDto>();

            foreach (var item in locations)
            {
                var contacts = informations.Where(x => x.Content == item.Content).ToList();

                var contactIds = contacts.Select(x => x.ContactId).ToList();

                var registeredContact = informations.Where(x => contactIds.Contains(x.ContactId) && x.InformationType == InformationEnum.PhoneNumber).Count();

                reportViews.Add(new ReportViewDto
                {
                    Content = item.Content,
                    ContactCount = contacts.Count,
                    RegisteredContactCount = registeredContact
                });
            }

            SaveExcelFile(reportViews, fullPathWithName);

            getReport.Status = ReportStatus.COMPLETED;
            getReport.FileUrl = fullPathWithName;
            await _uow.ReportRepository.UpdateAsync(getReport);
            await _uow.Commit();

            result.IsSucceed = true;
            result.ResultObject = reportViews;
            return result;
        }

        public async Task<ApiResult<ReportDetailDto>> Get(Guid id)
        {
            var result = new ApiResult<ReportDetailDto>();

            var report = await _uow.ReportRepository.FindByAsync(x => x.Id == id);

            if (report == null)
            {
                result.IsSucceed = false;
                return result;
            }

            var dto = _mapper.Map<ReportDetailDto>(report); 

            result.IsSucceed = true;
            result.ResultObject = dto;
            return result;
        }

        public async Task<ApiResult<List<ReportListDto>>> GetAll()
        {
            var result = new ApiResult<List<ReportListDto>>();

            var reports = await _uow.ReportRepository.GetAllAsync();

            result.IsSucceed = true;
            result.ResultObject = _mapper.Map<List<ReportListDto>>(reports);
            return result;
        }

        #region Excel Actions
        private void SaveExcelFile(List<ReportViewDto> reportViews, string path)
        {
            if (reportViews != null)
            {
                DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(reportViews), (typeof(DataTable)));

                using (SpreadsheetDocument document = SpreadsheetDocument.Create(path, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    var sheetData = new SheetData();
                    worksheetPart.Worksheet = new Worksheet(sheetData);

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Sheet1" };

                    sheets.Append(sheet);

                    Row headerRow = new Row();

                    List<String> columns = new List<string>();
                    foreach (System.Data.DataColumn column in table.Columns)
                    {
                        columns.Add(column.ColumnName);

                        Cell cell = new Cell();
                        cell.DataType = CellValues.String;
                        cell.CellValue = new CellValue(column.ColumnName);
                        headerRow.AppendChild(cell);
                    }

                    sheetData.AppendChild(headerRow);

                    foreach (DataRow dsrow in table.Rows)
                    {
                        Row newRow = new Row();
                        foreach (String col in columns)
                        {
                            Cell cell = new Cell();
                            cell.DataType = CellValues.String;
                            cell.CellValue = new CellValue(dsrow[col].ToString());
                            newRow.AppendChild(cell);
                        }

                        sheetData.AppendChild(newRow);
                    }

                    workbookPart.Workbook.Save();
                }
            }
        }

        #endregion
    }
}