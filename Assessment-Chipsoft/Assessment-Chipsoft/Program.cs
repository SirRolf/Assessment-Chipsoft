using System.Text;
using System.Text.Json;
using Assessment_Chipsoft.Records;
using Microsoft.AspNetCore.Http.HttpResults;

StringBuilder sb = new();
string logFilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlorisVanDenBerg", "AssessmentChipsoft");//using appData as we are still doing everything locally

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

Dictionary<int, List<PatientInfo>> patients = [];//For this task in memory data is acceptable so will implement a database only if I have time left over

app.Use(async (context, next) =>
{
	sb.AppendLine($"Starting {context.Request.Method} {context.Request.Path} at {DateTime.UtcNow}");
	await next(context);
	sb.AppendLine($"End {context.Request.Method} {context.Request.Path} at {DateTime.UtcNow}").AppendLine("===================");
	//Create directory for the logfile 
	if (!Directory.Exists(logFilePath))
	{
		Directory.CreateDirectory(logFilePath);
	}
	
	//append the text added to the string builder to the log file
	File.AppendAllText(Path.Combine(logFilePath, "log.txt"), sb.ToString());
	sb.Clear();
});

app.MapGet("/PatientDatabase/{id}", Results<Ok<PatientInfo>, NotFound> (int id) =>//Authorization not necessary as it falls outside the scope for this task
{
	//Try and find the patient by id  to see if we have him in memory
	if (!patients.TryGetValue(id, out List<PatientInfo>? patient))
	{
		sb.AppendLine($"Patient {id} not found");
		
		//If not then return 404
		return TypedResults.NotFound();
	}
	
	PatientInfo info = patient.Last();//Returning only the last Patient Info. Could consider giving the entire list.
	sb.AppendLine($"Patient {id} found and returned");
	sb.AppendLine(JsonSerializer.Serialize(info));
	
	//if patient is found then return patient with 200
	return TypedResults.Ok(info);
});

app.MapPost("/PatientDatabase", (PatientInfo info) =>
{
	//instead of having just one PatientInfo i create a backlog for every patient. In case some human error gets made we would still have precious data of patient.
	if (!patients.TryGetValue(info.Id, out List<PatientInfo>? value))
	{
		sb.AppendLine($"Creating new patient in data with id: {info.Id}");
		List<PatientInfo> patientList = [info];
		patients.Add(info.Id, patientList);
	}
	else
	{
		sb.AppendLine($"Patient {info.Id} already exists");
        value.Add(info);
	}
	
	sb.AppendLine(JsonSerializer.Serialize(info));
	
	return TypedResults.Created("/PatientDatabase/{id}", patients);
});

app.Run();