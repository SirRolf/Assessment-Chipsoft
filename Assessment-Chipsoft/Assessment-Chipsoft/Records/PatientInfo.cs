namespace Assessment_Chipsoft.Records;

/// <summary>
/// record containing all information for given patient
/// </summary>
/// <param name="Id">ID of patient</param>
/// <param name="Name">Name of patient</param>
/// <param name="Allergies">string of allergies by name</param>
public record PatientInfo(int Id, string Name, List<string> Allergies);//chose to use a list of strings as opposed to enums because of the wide range of niche allergies