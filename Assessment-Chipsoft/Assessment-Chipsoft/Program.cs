using System.Text;
using System.Text.Json;
using Assessment_Chipsoft.Records;
using Microsoft.AspNetCore.Http.HttpResults;

StringBuilder sb = new();
string localAppDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "FlorisVanDenBerg", "AssessmentChipsoft");//using appData as we are still doing everything locally

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

Dictionary<int, List<PatientInfo>> patients = [];//For this task in memory data is acceptable so will implement a database only if I have time left over

app.Use(async (context, next) =>
{
	sb.AppendLine($"Starting {context.Request.Method} {context.Request.Path} at {DateTime.UtcNow}");
	await next(context);
	sb.AppendLine($"End {context.Request.Method} {context.Request.Path} at {DateTime.UtcNow}").AppendLine("===================");
	//Create directory for the logfile 
	if (!Directory.Exists(localAppDataPath))
	{
		Directory.CreateDirectory(localAppDataPath);
	}
	
	//append the text added to the string builder to the log file
	File.AppendAllText(Path.Combine(localAppDataPath, "log.txt"), sb.ToString());
	sb.Clear();
});

app.MapGet("/PatientDatabase/{id}", Results<Ok<PatientInfo>, NotFound> (int id) =>//Authorization not necessary as it falls outside the scope for this task
{
	//Try and find the patient by id  to see if we have him in memory
	if (!patients.TryGetValue(id, out List<PatientInfo>? patient))
	{
		sb.AppendLine($"Patient with id: {id} not found");
		
		//If not then return 404
		return TypedResults.NotFound();
	}
	
	PatientInfo info = patient.Last();//Returning only the last Patient Info. Could consider giving the entire list.
	sb.AppendLine($"Patient with id: {id} found and returned");
	sb.AppendLine(JsonSerializer.Serialize(info));
	
	//if patient is found then return patient with 200
	return TypedResults.Ok(info);
});

//uploading new patient
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
		sb.AppendLine($"Patient with id: {info.Id} already exists");
        value.Add(info);
	}
	
	sb.AppendLine(JsonSerializer.Serialize(info));
	
	return TypedResults.Created("/PatientDatabase/{id}", patients);
});

//upload new file
app.MapPost("/PatientDatabase/upload", (int patientId, IFormFile file) =>
{
	if (!Directory.Exists(localAppDataPath))
	{
		Directory.CreateDirectory(localAppDataPath);
	}
	//create new filepath using a GUID to make sure we don't have duplicates
	string filepath = Path.Combine(localAppDataPath, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
	DocumentInfo documentInfo = new(file.FileName, filepath);
	//create copy of file at file path
	using FileStream filestream = File.Create(filepath);
	file.CopyToAsync(filestream);
	
	//see if patient is already in data and add if nececary
	if (!patients.TryGetValue(patientId, out List<PatientInfo>? value))
	{
		//add newly made document info to patient
		sb.AppendLine($"Patient with id: {patientId} does not exist, creating new empty patient");
		List<PatientInfo> patientList = [new(patientId, "", new List<string>(), [documentInfo])];
		patients.Add(patientId, patientList);
	}
	else
	{
		sb.AppendLine($"Patient with id: {patientId} exists");
		PatientInfo lastPatientInfo = patients[patientId].Last();//TODO: this could be cleaned up a bit
		List<DocumentInfo> newDocuments =
		[
			..lastPatientInfo.Documents,
			documentInfo
		];
		patients[patientId].Add(lastPatientInfo with { Documents = newDocuments });
	}
	
	sb.AppendLine($"Documents added to patient with id: {patientId}").AppendLine(documentInfo.ToString());
	return TypedResults.Created("/PatientDatabase/{id}", documentInfo);
});

app.Run();