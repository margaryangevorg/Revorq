using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Revorq.DAL.Context;
using Revorq.DAL.Entities;
using Revorq.DAL.Enums;

namespace Revorq.API.Controllers;

// One-off import of legacy elevator data from Sheet 1 ("ՍՊԱՍԱՐԿՄԱՆ ՑԱՆԿ") of Elevators.xlsx.
// Delete this controller once the import has been run.
[ApiController]
[Route("api/temp-import")]
//[Authorize(Roles = nameof(Role.Admin))]
public class TempImportController : ControllerBase
{
    private readonly AppDbContext _context;

    public TempImportController(AppDbContext context)
    {
        _context = context;
    }

    private static readonly Dictionary<string, BuildingType> BuildingTypeMap = new()
    {
        ["ՀՅՈՒՐԱՆՈՑ"] = BuildingType.Hotel,
        ["ԿՈՄԵՐՑԻՈՆ"] = BuildingType.Commercial,
        ["ԲՆԱԿԱՐԱՆ"] = BuildingType.House,
        ["ԲՆԱԿԵԼԻ"] = BuildingType.Residential,
        ["ՄՇԱԿՈՒՅԹԱՅԻՆ"] = BuildingType.Cultural,
        ["ՄՇԱԿՈՒԹԱՅԻՆ"] = BuildingType.Cultural,
        ["Խ/Ս"] = BuildingType.Shop,
        ["ՀԱՍԱՐԱԿԱԿԱՆ"] = BuildingType.Public,
        ["ՇԻՆԱՐԱՐՈՒԹՅՈՒՆ"] = BuildingType.Cunstruction,
        ["ԱՐԴՅՈՒՆԱԲԵՐԱԿԱՆ"] = BuildingType.Industrial,
        ["ՊԵՏԱԿԱՆ"] = BuildingType.Government,
        ["ԱՅԼ"] = BuildingType.Other,
    };

    private static readonly Dictionary<string, WarrantyType> WarrantyTypeMap = new()
    {
        ["ԵՏԵՐԱՇԽԻՔԱՅԻՆ"] = WarrantyType.PostWarranty,
        ["ԵՐԱՇԽԻՔԱՅԻՆ"] = WarrantyType.Warranty,
        ["ՉՍՊԱՍԱՐԿՎՈՂ"] = WarrantyType.Unsupervised,
    };

    private static readonly Dictionary<string, Priority> PriorityMap = new()
    {
        ["ԲԱՐՁՐ"] = Priority.High,
        ["ՄԻՋԻՆ"] = Priority.Medium,
        ["ՑԱԾՐ"] = Priority.Low,
    };

    private record SheetRow(
        int RowNumber,
        string? CustomerFullName,
        string? BuildingName,
        string? BuildingTypeText,
        string? Address,
        string? Phone,
        string? WarrantyTypeText,
        string? NumberInProject,
        string? Model,
        string? SerialNumber,
        string? Country,
        string? WarrantyDateText,
        string? PriorityText);

    private class ImportReport
    {
        public string Mode { get; set; } = "";
        public int TotalDataRows { get; set; }
        public int SkippedEmptyRows { get; set; }
        public int SkippedMissingSerial { get; set; }
        public int ImportableElevatorCount { get; set; }
        public List<string> DuplicateSerialsSuffixed { get; } = [];
        public List<string> AddressVariantsIgnored { get; } = [];
        public List<string> AddressesDisambiguated { get; } = [];
        public List<string> BuildingTypeConflicts { get; } = [];
        public List<string> UnmappedBuildingTypes { get; } = [];
        public List<string> UnmappedWarrantyTypes { get; } = [];
        public List<string> UnmappedPriorities { get; } = [];
        public List<string> UnparseableWarrantyDates { get; } = [];
        public List<string> SkippedNoResolvedBuilding { get; } = [];
        public List<string> SkippedMissingNumberInProject { get; } = [];
        public int BuildingsCreated { get; set; }
        public int BuildingsReused { get; set; }
        public int ElevatorsCreated { get; set; }
        public List<string> ElevatorsSkippedAlreadyInDb { get; } = [];
    }

    [HttpPost("elevators")]
    public async Task<IActionResult> ImportElevators(IFormFile file, [FromQuery] bool commit = false, [FromQuery] int companyId = 1)
    {
        if (file is null || file.Length == 0)
            return BadRequest("No file uploaded.");

        var report = new ImportReport { Mode = commit ? "commit" : "dry-run" };

        using var stream = file.OpenReadStream();
        using var workbook = new XLWorkbook(stream);
        var ws = workbook.Worksheets.Worksheet(1);

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
        var rows = new List<SheetRow>();

        for (var r = 4; r <= lastRow; r++)
        {
            string? Get(int col)
            {
                var trimmed = ws.Cell(r, col).GetString()?.Trim();
                return string.IsNullOrEmpty(trimmed) ? null : trimmed;
            }

            var row = new SheetRow(
                RowNumber: r,
                CustomerFullName: Get(2),
                BuildingName: Get(3),
                BuildingTypeText: Get(4),
                Address: Get(5),
                Phone: Get(6),
                WarrantyTypeText: Get(7),
                NumberInProject: Get(8),
                Model: Get(9),
                SerialNumber: Get(10),
                Country: Get(13),
                WarrantyDateText: Get(14),
                PriorityText: Get(15));

            if (row.BuildingName is null && row.SerialNumber is null && row.CustomerFullName is null)
            {
                report.SkippedEmptyRows++;
                continue;
            }

            rows.Add(row);
        }

        report.TotalDataRows = rows.Count;

        // Resolve one (Address, BuildingType) per BuildingName - first occurrence in the sheet wins.
        var buildingInfo = new Dictionary<string, (string Address, BuildingType Type, int FirstRow)>();
        foreach (var row in rows)
        {
            if (row.BuildingName is null || row.Address is null || row.BuildingTypeText is null) continue;

            if (!BuildingTypeMap.TryGetValue(row.BuildingTypeText, out var type))
            {
                report.UnmappedBuildingTypes.Add($"Row {row.RowNumber}: '{row.BuildingTypeText}'");
                continue;
            }

            if (!buildingInfo.TryGetValue(row.BuildingName, out var existing))
            {
                buildingInfo[row.BuildingName] = (row.Address, type, row.RowNumber);
                continue;
            }

            if (existing.Address != row.Address)
                report.AddressVariantsIgnored.Add(
                    $"'{row.BuildingName}': kept '{existing.Address}' (row {existing.FirstRow}), ignored '{row.Address}' (row {row.RowNumber})");

            if (existing.Type != type)
                report.BuildingTypeConflicts.Add(
                    $"'{row.BuildingName}': kept {existing.Type} (row {existing.FirstRow}), ignored {type} (row {row.RowNumber})");
        }

        // Disambiguate addresses shared by more than one distinct building name.
        var finalAddress = buildingInfo.ToDictionary(kv => kv.Key, kv => kv.Value.Address);
        var addressGroups = buildingInfo
            .GroupBy(kv => kv.Value.Address)
            .Where(g => g.Select(kv => kv.Key).Distinct().Count() > 1);

        foreach (var group in addressGroups)
        {
            foreach (var kv in group)
            {
                finalAddress[kv.Key] = $"{kv.Value.Address} — {kv.Key}";
                report.AddressesDisambiguated.Add($"'{kv.Key}': '{finalAddress[kv.Key]}'");
            }
        }

        // Filter rows down to the ones that will actually become Elevator records.
        var serialSeen = new Dictionary<string, int>();
        var importable = new List<(SheetRow Row, string Serial, WarrantyType Warranty, Priority? Priority, DateTime? WarrantyDate)>();

        foreach (var row in rows)
        {
            if (row.SerialNumber is null)
            {
                report.SkippedMissingSerial++;
                continue;
            }

            if (row.BuildingName is null || !buildingInfo.ContainsKey(row.BuildingName))
            {
                report.SkippedNoResolvedBuilding.Add($"Row {row.RowNumber}: building '{row.BuildingName}'");
                continue;
            }

            if (row.WarrantyTypeText is null || !WarrantyTypeMap.TryGetValue(row.WarrantyTypeText, out var warranty))
            {
                report.UnmappedWarrantyTypes.Add($"Row {row.RowNumber}: '{row.WarrantyTypeText}'");
                continue;
            }

            if (row.NumberInProject is null)
            {
                report.SkippedMissingNumberInProject.Add($"Row {row.RowNumber}");
                continue;
            }

            Priority? priority = null;
            if (row.PriorityText is not null)
            {
                if (PriorityMap.TryGetValue(row.PriorityText, out var p)) priority = p;
                else report.UnmappedPriorities.Add($"Row {row.RowNumber}: '{row.PriorityText}'");
            }

            DateTime? warrantyDate = null;
            if (row.WarrantyDateText is not null)
            {
                var normalized = row.WarrantyDateText.Replace('․', '.');
                if (DateTime.TryParseExact(normalized, "dd.MM.yyyy", null, System.Globalization.DateTimeStyles.None, out var d))
                    warrantyDate = d;
                else
                    report.UnparseableWarrantyDates.Add($"Row {row.RowNumber}: '{row.WarrantyDateText}'");
            }

            var serial = row.SerialNumber;
            if (serialSeen.TryGetValue(serial, out var count))
            {
                count++;
                serialSeen[serial] = count;
                var suffixed = $"{serial}-{count}";
                report.DuplicateSerialsSuffixed.Add($"Row {row.RowNumber}: '{serial}' -> '{suffixed}'");
                serial = suffixed;
            }
            else
            {
                serialSeen[serial] = 1;
            }

            importable.Add((row, serial, warranty, priority, warrantyDate));
        }

        report.ImportableElevatorCount = importable.Count;

        if (!commit)
            return Ok(report);

        // --- Commit: create buildings, then elevators ---
        var buildingNamesNeeded = importable.Select(i => i.Row.BuildingName!).Distinct().ToList();
        var buildingIdByName = new Dictionary<string, int>();

        var existingBuildings = await _context.Buildings
            .Where(b => b.CompanyId == companyId && buildingNamesNeeded.Contains(b.Name))
            .ToListAsync();
        foreach (var b in existingBuildings)
        {
            buildingIdByName[b.Name] = b.Id;
            report.BuildingsReused++;
        }

        var newBuildings = new Dictionary<string, Building>();
        foreach (var name in buildingNamesNeeded)
        {
            if (buildingIdByName.ContainsKey(name)) continue;

            var info = buildingInfo[name];
            var building = new Building
            {
                Name = name,
                Address = finalAddress[name],
                BuildingType = info.Type,
                CompanyId = companyId
            };
            _context.Buildings.Add(building);
            newBuildings[name] = building;
        }

        if (newBuildings.Count > 0)
        {
            await _context.SaveChangesAsync();
            foreach (var (name, building) in newBuildings)
            {
                buildingIdByName[name] = building.Id;
                report.BuildingsCreated++;
            }
        }

        var serialsToImport = importable.Select(i => i.Serial).ToList();
        var existingSerials = (await _context.Elevators
            .Where(e => serialsToImport.Contains(e.SerialNumber))
            .Select(e => e.SerialNumber)
            .ToListAsync())
            .ToHashSet();

        foreach (var item in importable)
        {
            if (existingSerials.Contains(item.Serial))
            {
                report.ElevatorsSkippedAlreadyInDb.Add($"Row {item.Row.RowNumber}: serial '{item.Serial}' already in DB");
                continue;
            }

            var elevator = new Elevator
            {
                NumberInProject = item.Row.NumberInProject!,
                SerialNumber = item.Serial,
                Model = item.Row.Model,
                ProductionCountry = item.Row.Country,
                CustomerFullName = item.Row.CustomerFullName,
                CustomerPhoneNumber = item.Row.Phone,
                WarrantyType = item.Warranty,
                WarrantyDate = item.WarrantyDate,
                Priority = item.Priority,
                BuildingId = buildingIdByName[item.Row.BuildingName!]
            };
            _context.Elevators.Add(elevator);
            report.ElevatorsCreated++;
        }

        await _context.SaveChangesAsync();

        return Ok(report);
    }
}
