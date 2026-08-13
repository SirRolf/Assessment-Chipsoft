using Assessment_Chipsoft.Records;
using Microsoft.AspNetCore.Http.HttpResults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

Dictionary<int, List<PatientInfo>> patients = [];//For this task in memory data is acceptable so will implement a database only if I have time left over

app.MapGet("/PatientDatabase/{id}", Results<Ok<PatientInfo>, NotFound> (int id) =>//Authorization not necessary as it falls outside the scope for this task
{
	//Try and find the patient by id  to see if we have him in memory
	if (!patients.TryGetValue(id, out List<PatientInfo>? patient))
	{
		//If not then return 404
		return TypedResults.NotFound();
	}
	//if patient is found then return patient with 200
	return TypedResults.Ok(patient.Last());//Returning only the last Patient Info. Could consider giving the entire list.
});

app.MapPost("/PatientDatabase", (PatientInfo info) =>
{
	//instead of having just one PatientInfo i create a backlog for every patient. In case some human error gets made we would still have precious data of patient.
	if (!patients.TryGetValue(info.Id, out List<PatientInfo>? value))
	{
		List<PatientInfo> patientList = [info];
		patients.Add(info.Id, patientList);
	}
	else
	{
        value.Add(info);
	}
	return TypedResults.Created("/PatientDatabase/{id}", patients);
});

app.Run();