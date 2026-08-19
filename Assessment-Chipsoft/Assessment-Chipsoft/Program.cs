using System.Text;
using Assessment_Chipsoft.Endpoints;
using Assessment_Chipsoft.Records;

StringBuilder sb = new();
string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlorisVanDenBerg", "AssessmentChipsoft");//using appData as we are still doing everything locally

//setup builder and create webApplication
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

//In Memory Data
Dictionary<int, List<PatientInfo>> patients = [];

//Middleware for logging system.
app.Use(async (context, next) =>
{
	//append starting point, await the rest of the httpRequest and add end point of log
	sb.AppendLine($"Starting {context.Request.Method} {context.Request.Path} at {DateTime.UtcNow}");
	await next(context);
	sb.AppendLine($"End {context.Request.Method} {context.Request.Path} at {DateTime.UtcNow}").AppendLine("===================");
	//Create directory for the logfile if it does not exist
	if (!Directory.Exists(localAppDataPath))
	{
		Directory.CreateDirectory(localAppDataPath);
	}
	
	//append the text added to the string builder to the log file and clear string builder
	File.AppendAllText(Path.Combine(localAppDataPath, "log.txt"), sb.ToString());
	sb.Clear();
});

app.MapGetLatestPatientInfo(patients, sb);

app.MapUploadNewPatient(patients, sb);

app.MapUploadDocument(patients,localAppDataPath, sb);

app.Run();