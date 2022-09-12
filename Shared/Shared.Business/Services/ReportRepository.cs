using AutoMapper;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
            dto.ReportDatas = ReadExcelFile(dto.FileUrl);
            
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

        private List<ReportViewDto> ReadExcelFile(string path)
        {
            List<ReportViewDto> result = new List<ReportViewDto>();
            try
            {
                if (String.IsNullOrWhiteSpace(path))
                    return result;

                DataTable dataTable = new DataTable();
                using (SpreadsheetDocument spreadSheetDocument = SpreadsheetDocument.Open(path, false))
                {
                    WorkbookPart workbookPart = spreadSheetDocument.WorkbookPart;
                    IEnumerable<Sheet> sheets = spreadSheetDocument.WorkbookPart.Workbook.GetFirstChild<Sheets>().Elements<Sheet>();
                    string relationshipId = sheets.First().Id.Value;
                    WorksheetPart worksheetPart = (WorksheetPart)spreadSheetDocument.WorkbookPart.GetPartById(relationshipId);
                    Worksheet workSheet = worksheetPart.Worksheet;
                    SheetData sheetData = workSheet.GetFirstChild<SheetData>();
                    IEnumerable<Row> rows = sheetData.Descendants<Row>();

                    foreach (Cell cell in rows.ElementAt(0))
                    {
                        dataTable.Columns.Add(GetCellValue(spreadSheetDocument, cell));
                    }

                    foreach (Row row in rows)
                    {
                        DataRow dataRow = dataTable.NewRow();
                        for (int i = 0; i < row.Descendants<Cell>().Count(); i++)
                        {
                            dataRow[i] = GetCellValue(spreadSheetDocument, row.Descendants<Cell>().ElementAt(i));
                        }

                        dataTable.Rows.Add(dataRow);
                    }

                }
                dataTable.Rows.RemoveAt(0);
                var jsonList = JsonConvert.SerializeObject(dataTable);

                result = JsonConvert.DeserializeObject<List<ReportViewDto>>(jsonList);
            }
            catch (Exception)
            {

            }
            return result;
        }

        private static string GetCellValue(SpreadsheetDocument document, Cell cell)
        {
            SharedStringTablePart stringTablePart = document.WorkbookPart.SharedStringTablePart;
            string value = cell.CellValue.InnerXml;

            if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
            {
                return stringTablePart.SharedStringTable.ChildElements[Int32.Parse(value)].InnerText;
            }
            else
            {
                return value;
            }
        }
        #endregion
    }
}