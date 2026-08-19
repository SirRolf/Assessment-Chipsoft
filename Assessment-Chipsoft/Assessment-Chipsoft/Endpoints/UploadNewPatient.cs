using System.Text;
using System.Text.Json;
using Assessment_Chipsoft.Records;

namespace Assessment_Chipsoft.Endpoints;

public static class UploadNewPatient
{
	public static void MapUploadNewPatient(this IEndpointRouteBuilder app, Dictionary<int, List<PatientInfo>> patients, StringBuilder sb)
	{
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
	}
}