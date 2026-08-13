using Assessment_Chipsoft.Records;
using Microsoft.AspNetCore.Http.HttpResults;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

Dictionary<int, PatientInfo?> patients = [];//For this task in memory data is acceptable so will implement a database only if I have time left over

app.MapGet("/PatientDatabase/{id}", Results<Ok<PatientInfo>, NotFound> (int id) =>//Authorization not necessary as it falls outside the scope for this task
{
	if (!patients.TryGetValue(id, out PatientInfo? patient))
	{
		return TypedResults.NotFound();
	}
	return TypedResults.Ok(patient);
});

app.MapPost("/PatientDatabase", (PatientInfo info) =>
{
	//TODO: Add check if patient is already in the Database
	patients.Add(info.Id,info);
	return TypedResults.Created("/PatientDatabase/{id}", patients);
});

app.Run();