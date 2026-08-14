using System.Text;
using System.Text.Json;
using Assessment_Chipsoft.Records;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

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

//receive latest patient info with given id
app.MapGet("/PatientDatabase/{id}", Results<Ok<PatientInfo>, NotFound> (int id) =>//Authorization not necessary as it falls outside the scope for this task
{
	//Try and find the patient by id to see if we have him in memory
	if (!patients.TryGetValue(id, out List<PatientInfo>? patient))
	{
		sb.AppendLine($"Patient with id: {id} not found");
		
		//If not then return 404
		return TypedResults.NotFound();
	}
	
	//Return latest patient info
	PatientInfo info = patient.Last();
	//append result and 
	sb.AppendLine($"Patient with id: {id} found and returned");
	sb.AppendLine(JsonSerializer.Serialize(info));
	
	//if patient is found then return patient with 200
	return TypedResults.Ok(info);
});

//uploading new patient
app.MapPost("/PatientDatabase", (PatientInfo info) =>
{
	//Try and find the patient by id to see if we have him in memory
	if (!patients.TryGetValue(info.Id, out List<PatientInfo>? value))
	{
		//if the patient doesn't exist
		sb.AppendLine($"Creating new patient in data with id: {info.Id}");
		//create a new list for instances of PatientInfo and add it to the dictionary
		List<PatientInfo> patientList = [info];
		patients.Add(info.Id, patientList);
	}
	else
	{
		//if the patient does exist
		sb.AppendLine($"Patient with id: {info.Id} already exists");
		//add the new value to the patient
		value.Add(info);
	}
	
	sb.AppendLine(JsonSerializer.Serialize(info));
	
	//return 200
	return TypedResults.Created("/PatientDatabase/{id}", info);
});

//upload new file
app.MapPost("/PatientDatabase/upload", ([FromForm]int patientId, IFormFile file) =>
{
	//create appDataPath if it doesn't exist
	if (!Directory.Exists(localAppDataPath))
	{
		Directory.CreateDirectory(localAppDataPath);
	}
	//create new filepath using a GUID to make sure we don't have duplicates
	string filepath = Path.Combine(localAppDataPath, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
	//create the Document Info
	DocumentInfo documentInfo = new(file.FileName, filepath);
	//create copy of file at file path
	using FileStream filestream = File.Create(filepath);
	file.CopyToAsync(filestream);//TODO: error handling for CopyTo as it could return issues
	
	//see if patient is already in data and add if nececary
	if (!patients.TryGetValue(patientId, out List<PatientInfo>? _))
	{
		//add newly made document info to patient
		sb.AppendLine($"Patient with id: {patientId} does not exist, creating new empty patient");
		List<PatientInfo> patientList = [new(patientId, "", new List<string>(), [documentInfo])];
		patients.Add(patientId, patientList);
	}
	else
	{
		//IF patient exists
		sb.AppendLine($"Patient with id: {patientId} exists");
		//Grab the latest PatientInfo
		PatientInfo lastPatientInfo = patients[patientId].Last();
		//Create a new list for the documents In the PatientInfo with the contents of the latest patientInfo
		List<DocumentInfo> documents = new(lastPatientInfo.Documents);
		//Add our new Document info to the new documents list
		documents.Add(documentInfo);
		
		//create a new PatientInfo Instance with the new list of documents including our new DocumentInfo instance
		patients[patientId].Add(lastPatientInfo with { Documents = documents });
	}
	
	sb.AppendLine($"Documents added to patient with id: {patientId}").AppendLine(documentInfo.ToString());
	return TypedResults.Created("/PatientDatabase/{id}", documentInfo);
}).DisableAntiforgery();//disabaling anti Forgery because security falls outside the scope of this task

app.Run();