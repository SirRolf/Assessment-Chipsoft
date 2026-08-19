using System.Text;
using Assessment_Chipsoft.Records;
using Microsoft.AspNetCore.Mvc;

namespace Assessment_Chipsoft.Endpoints;

public static class UploadDocument
{
	/// <summary>
	/// Upload new document
	/// </summary>
	/// <param name="app">Enpoint Route Builder build in Program.cs</param>
	/// <param name="patients">Dictionary of patients used as database</param>
	/// <param name="path">Path where file will be saved</param>
	/// <param name="sb">Stringbuilder for logging</param>
	public static void MapUploadDocument(this IEndpointRouteBuilder app, Dictionary<int, List<PatientInfo>> patients, string path, StringBuilder sb)
	{
		//upload new file
		app.MapPost("/PatientDatabase/upload", async ([FromForm]int patientId, IFormFile file) =>
		{
			//create appDataPath if it doesn't exist
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
			//create new filepath using a GUID to make sure we don't have duplicates
			string filepath = Path.Combine(path, $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}");
			//create the Document Info
			DocumentInfo documentInfo = new(file.FileName, filepath);
			//create copy of file at file path
			await using FileStream filestream = File.Create(filepath);
			await file.CopyToAsync(filestream);//TODO: error handling for CopyTo as it could return issues
			
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
	}
}