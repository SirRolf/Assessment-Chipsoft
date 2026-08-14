namespace Assessment_Chipsoft.Records;

/// <summary>
/// record containing all information for given patient
/// </summary>
/// <param name="Id">ID of patient</param>
/// <param name="Name">Name of patient</param>
/// <param name="Allergies">List of allergies</param>
/// <param name="Documents">list of documents</param>
public record PatientInfo(int Id, string Name, List<string> Allergies, List<DocumentInfo> Documents);//chose to use a list of strings as opposed to enums because of the wide range of niche allergies