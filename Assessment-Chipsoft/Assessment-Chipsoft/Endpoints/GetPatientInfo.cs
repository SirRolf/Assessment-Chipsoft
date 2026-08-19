using System.Text;
using System.Text.Json;
using Assessment_Chipsoft.Records;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Assessment_Chipsoft.Endpoints;

public static class GetPatientInfo
{
	public static void MapGetLatestPatientInfo(this IEndpointRouteBuilder app, Dictionary<int, List<PatientInfo>> patients, StringBuilder sb)
	{
		//receive latest patient info with given id
		app.MapGet("/PatientDatabase/{id}", Results<Ok<PatientInfo>, NotFound> (int id) =>//Authorization not necessary as it falls outside the scope for this task
		{
			return GetPatient(patients, sb, id);
		});
	}
	
	public static void MapGetPatientInfo(this IEndpointRouteBuilder app, Dictionary<int, List<PatientInfo>> patients, StringBuilder sb)
	{
		//receive latest patient info with given id
		app.MapGet("/PatientDatabase/{id}/{version}", Results<Ok<PatientInfo>, NotFound> (int id, int version) =>//Could use type Version but using int as proof of concept
		{
			return GetPatient(patients, sb, id, version);
		});
	}

	private static Results<Ok<PatientInfo>, NotFound> GetPatient(Dictionary<int, List<PatientInfo>> patients, StringBuilder sb, int id, int version = -1)
	{
		//Try and find the patient by id to see if we have him in memory
		if (!patients.TryGetValue(id, out List<PatientInfo>? patient))
		{
			sb.AppendLine($"Patient with id: {id} not found");
		
			//If not then return 404
			return TypedResults.NotFound();
		}

		PatientInfo info;
		if (version != -1)
		{
			if (patients.Count < version)
			{
				sb.AppendLine($"Patient with id: {id} version {version} not found");
				
				return TypedResults.NotFound();
			}
			
			info = patient[version];
		}
		else
		{
			//Return latest patient info
			info =  patient.Last();
		}
		//append result and 
		sb.AppendLine($"Patient with id: {id} found and returned");
		sb.AppendLine(JsonSerializer.Serialize(info));
	
		//if patient is found then return patient with 200
		return TypedResults.Ok(info);
	}
}