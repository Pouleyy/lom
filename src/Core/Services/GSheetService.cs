using Core.Services.Interface;
using Core.Services.Models;
using Entities.Context;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;
using Microsoft.Extensions.Configuration;

namespace Core.Services;

public class GSheetService : IGSheetService
{
    private readonly LomDbContext _context;
    static readonly string[] Scopes = [SheetsService.Scope.Spreadsheets];
    private SheetsService _sheetsService { get; set; }
    private string _leaderBoardSheetId { get; set; }
    private string _leaderBoardSheetName { get; set; }
    
    public GSheetService(LomDbContext context, IConfiguration configuration)
    {
        _context = context;
        _leaderBoardSheetId = configuration.GetSection("GSheet:Leaderboard:SheetId").Value ?? throw new ArgumentNullException("Missing leaderboard sheet id in configuration");
        _leaderBoardSheetName = configuration.GetSection("GSheet:Leaderboard:SheetName").Value ?? throw new ArgumentNullException("Missing leaderboard sheet name in configuration");
        var credentialsFile = configuration.GetSection("GSheet:CredentialsPath").Value ?? throw new ArgumentNullException("Missing credentials file path in configuration");
        var credentials = GoogleCredential.FromFile(credentialsFile).CreateScoped(Scopes);
        _sheetsService = new SheetsService(new BaseClientService.Initializer
        {
            HttpClientInitializer = credentials,
            ApplicationName = "Lom"
        });
    }
    
    public async Task WriteTop10Guilds(List<IEnumerable<FamilyLeadboard>> families, CancellationToken cancellationToken = default)
    {
        var range = $"{_leaderBoardSheetName}!A2";
        var valueRange = new ValueRange();
        valueRange.Values = new List<IList<object>>();
        foreach (var familyLeadboard in families.SelectMany(family => family))
        {
            valueRange.Values.Add(new List<object> { familyLeadboard.ServerId, familyLeadboard.FamilyName, familyLeadboard.Power });
        } 
        var request = _sheetsService.Spreadsheets.Values.Update(valueRange, _leaderBoardSheetId, range);
        request.ValueInputOption = SpreadsheetsResource.ValuesResource.UpdateRequest.ValueInputOptionEnum.RAW;
        await request.ExecuteAsync(cancellationToken);
    }
}